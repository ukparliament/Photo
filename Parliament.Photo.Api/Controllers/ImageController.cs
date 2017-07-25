namespace Parliament.Photo.Api.Controllers
{
    using ImageMagick;
    using Microsoft.ApplicationInsights;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;
    using XmpCore;
    using XmpCore.Options;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        private TelemetryClient telemetry = new TelemetryClient();

        /// <param name="quality"><seealso cref="https://www.imagemagick.org/script/command-line-options.php#quality"/></param>
        public async Task<HttpResponseMessage> Get(string id, int? width = null, int? height = null, string crop = null, bool? download = null, int? quality = null)
        {
            Query(id, out Uri member, out string givenName, out string familyName, out int x, out int y);

            var cachedImage = new CachedImage();

            var response = CreateResponse(id, width, height, crop, quality, cachedImage);

            if (!await cachedImage.CacheBlob.ExistsAsync())
            {
                var image = await GetImage(id);

                Crop(image, x, y, crop);
                Resize(image, width, height);
                AddMetadata(id, image, member, givenName, familyName);
                Quality(quality, image);

                cachedImage.Image = image;
            }

            Download(id, download, response);

            return response;
        }

        private HttpResponseMessage CreateResponse(string id, int? width, int? height, string crop, int? quality, CachedImage cachedImage)
        {
            var response = Request.CreateResponse(cachedImage);
            var content = response.Content as ObjectContent<CachedImage>;
            var formatter = content.Formatter as ImageFormatter;
            var mimeType = HttpUtility.UrlEncode(formatter.SupportedMediaTypes.Single().ToString());

            var key = $"{id}/{mimeType}/crop_{crop}/dimensions_{width}x{height}/quality-{quality}";

            cachedImage.CacheBlob = FindCacheBlob(key);

            return response;
        }

        private static CloudBlockBlob FindCacheBlob(string key)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Cache"].ConnectionString;
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("cache");

            return container.GetBlockBlobReference(key);
        }

        private static void Query(string id, out Uri member, out string givenName, out string familyName, out int x, out int y)
        {
            var queryString = @"
PREFIX :<http://id.ukpds.org/schema/>

SELECT ?member ?givenName ?familyName ?x ?y
WHERE {
    BIND (@id AS ?image)

    ?image
        :memberImageHasMember ?member ;
        :personImageFaceCentrePoint ?point .
    ?member
        :personGivenName ?givenName ;
        :personFamilyName ?familyName .

    BIND(STRBEFORE(STRAFTER(STR(?point), ""(""), "" "") AS ?x)
    BIND(STRBEFORE(STRAFTER(STR(?point), "" ""), "")"") AS ?y)
}
";

            var query = new SparqlParameterizedString(queryString);

            var idUri = new Uri("http://id.ukpds.org/");
            var imageUri = new Uri(idUri, id);

            query.SetUri("id", imageUri);

            var endpointString = ConfigurationManager.ConnectionStrings["SparqlEndpoint"].ConnectionString;
            var endpoint = new Uri(endpointString);

            using (var connector = new SparqlConnector(endpoint))
            {
                using (var results = connector.Query(query.ToString()) as SparqlResultSet)
                {
                    var result = results.SingleOrDefault();

                    if (result == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }

                    member = new Uri((result["member"] as ILiteralNode).Value);
                    givenName = (result["givenName"] as ILiteralNode).Value;
                    familyName = (result["familyName"] as ILiteralNode).Value;
                    x = int.Parse((result["x"] as ILiteralNode).Value);
                    y = int.Parse((result["y"] as ILiteralNode).Value);
                }
            }
        }

        private void Quality(int? quality, MagickImage image)
        {
            if (quality != null)
            {
                image.Quality = quality.Value;

                telemetry.TrackEvent(
                    "Quality",
                        metrics: new Dictionary<string, double> {
                        { "quality", (long)quality }
                    });
            }
        }

        private async Task<MagickImage> GetImage(string id)
        {
            var blob = GetRawSource(id);

            try
            {
                using (var stream = await blob.OpenReadAsync())
                {
                    var magick = new MagickImage(stream);

                    telemetry.TrackEvent("Loaded");

                    this.Request.RegisterForDispose(magick);

                    return magick;
                }
            }
            catch (StorageException e) when (e.Message.Contains("404"))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        private void Resize(MagickImage magick, int? width, int? height)
        {
            if (width == null && height == null)
            {
                return;
            }

            var size = new MagickGeometry(
                width.GetValueOrDefault(magick.Width),
                height.GetValueOrDefault(magick.Height));

            magick.ColorSpace = ColorSpace.RGB;
            magick.Scale(size);
            magick.RePage();
            magick.ColorSpace = ColorSpace.sRGB;

            telemetry.TrackEvent(
                "Resized",
                metrics: new Dictionary<string, double> {
                    { "width", size.Width },
                    { "height", size.Height},
                });
        }

        private void Crop(MagickImage magick, int x, int y, string crop)
        {
            switch (crop)
            {
                case null:
                    return;

                case "MCU_3:2":
                    magick.Crop(x - 1553, y - 789, 3108, 2072);
                    break;

                case "MCU_3:4":
                    magick.Crop(x - 789, y - 789, 1554, 2072);
                    break;

                case "CU_1:1":
                    magick.Crop(x - 789, y - 706, 1554, 1554);
                    break;

                case "CU_5:2":
                    magick.Crop(1, y - 670, 3795, 1518);
                    break;

                default:
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent("Allowed crop values:\n\tMCU_3:4\n\tMCU_3:2\n\tCU_1:1\n\tCU_5:2")
                        });
            }

            telemetry.TrackEvent(
                "Crop",
                new Dictionary<string, string> {
                    { "crop", crop }
                },
                new Dictionary<string, double> {
                    { "x", x },
                    { "y", y},
                });
        }

        private void AddMetadata(string id, MagickImage magick, Uri member, string givenName, string familyName)
        {
            using (var metadataController = new MetadataController())
            {
                var xmp = metadataController.Get(id, member, givenName, familyName);
                var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
                var xmpProfile = new XmpProfile(xmpBuffer);

                magick.AddProfile(xmpProfile);
            }

            telemetry.TrackEvent("Metadata");
        }

        private void Download(string id, bool? download, HttpResponseMessage response)
        {
            if (download.GetValueOrDefault())
            {
                var content = response.Content as ObjectContent;
                var formatterMapping = content.Formatter.MediaTypeMappings.First();
                var mediaType = formatterMapping.MediaType.MediaType;
                var mapping = Global.mappingData.Single(row => row.MediaType == mediaType);
                var extension = mapping.Extensions.First();
                var fileName = string.Format("{0}.{1}", id, extension);
                var disposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };

                content.Headers.ContentDisposition = disposition;

                telemetry.TrackEvent("Download");
            }
        }

        private static CloudBlob GetRawSource(string id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["PhotoStorage"].ConnectionString;
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");
            var blob = container.GetBlobReference(id);

            return blob;
        }
    }
}
