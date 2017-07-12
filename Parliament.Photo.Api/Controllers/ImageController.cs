namespace Parliament.Photo.Api.Controllers
{
    using ImageMagick;
    using Microsoft.ApplicationInsights;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using XmpCore;
    using XmpCore.Options;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        private TelemetryClient telemetry = new TelemetryClient();

        /// <param name="quality"><seealso cref="https://www.imagemagick.org/script/command-line-options.php#quality"/></param>
        public async Task<HttpResponseMessage> Get(string id, int? width = null, int? height = null, string crop = null, bool? download = null, int? quality = null)
        {
            var image = await GetImage(id);

            Crop(image, 1800, 1000, crop);
            Resize(image, width, height);
            AddMetadata(id, image);

            if (quality != null)
            {
                image.Quality = quality.Value;

                telemetry.TrackEvent(
                    "Resized",
                        metrics: new Dictionary<string, double> {
                        { "quality", (long)quality }
                    });
            }

            var response = Request.CreateResponse(image);

            Download(id, download, response);

            return response;
        }

        private async Task<MagickImage> GetImage(string id)
        {
            var blob = await GetRawSource(id);

            using (var stream = await blob.OpenReadAsync())
            {
                var magick = new MagickImage(stream);
                this.Request.RegisterForDispose(magick);

                return magick;
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

                case "MCU_3:4":
                    magick.Crop(x - 1553, y - 789, 3108, 2072);
                    break;

                case "MCU_3:2":
                    magick.Crop(x - 789, y - 789, 1554, 2072);
                    break;

                case "CU_1:1":
                    magick.Crop(x - 789, y - 789, 1554, 1554);
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
                "Cropped",
                new Dictionary<string, string> {
                    { "crop", crop }
                },
                new Dictionary<string, double> {
                    { "x", x },
                    { "y", y},
                });
        }

        private void AddMetadata(string id, MagickImage magick)
        {
            using (var metadataController = new MetadataController())
            {
                var xmp = metadataController.Get(id);
                var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
                var xmpProfile = new XmpProfile(xmpBuffer);

                magick.AddProfile(xmpProfile);
            }

            telemetry.TrackEvent("Added metadata");
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

        private async static Task<CloudBlob> GetRawSource(string id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["PhotoStorage"].ConnectionString;
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");
            var blob = container.GetBlobReference(id);

            if (!await blob.ExistsAsync())
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return blob;
        }
    }
}
