namespace Parliament.Photo.Api.Controllers
{
    using ImageMagick;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
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
        /// <param name="quality"><seealso cref="https://www.imagemagick.org/script/command-line-options.php#quality"/></param>
        public async Task<HttpResponseMessage> Get(string id, int? width = null, int? height = null, string crop = null, bool? download = null, int? quality = 100)
        {
            var image = await GetImage(id);

            Crop(image, 1800, 1000, crop);
            Resize(image, width, height);
            AddMetadata(id, image);

            image.Quality = quality.GetValueOrDefault(100);

            var response = Request.CreateResponse(image);

            if (download.GetValueOrDefault())
            {
                Download(id, response);
            }

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

        private static void Resize(MagickImage magick, int? width, int? height)
        {
            var size = new MagickGeometry(
                width.GetValueOrDefault(magick.Width),
                height.GetValueOrDefault(magick.Height));

            magick.ColorSpace = ColorSpace.RGB;
            magick.Scale(size);
            magick.RePage();
            magick.ColorSpace = ColorSpace.sRGB;
        }

        private static void Crop(MagickImage magick, int x, int y, string crop)
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
        }

        private static void AddMetadata(string id, MagickImage magick)
        {
            using (var metadataController = new MetadataController())
            {
                var xmp = metadataController.Get(id);
                var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
                var xmpProfile = new XmpProfile(xmpBuffer);

                magick.AddProfile(xmpProfile);
            }
        }

        private static void Download(string id, HttpResponseMessage response)
        {
            var content = response.Content as ObjectContent;
            var formatterMapping = content.Formatter.MediaTypeMappings.First();
            var mediaType = formatterMapping.MediaType.MediaType;
            var mapping = Global.mappingData.Single(row => row.MediaType == mediaType);
            var extension = mapping.Extensions.First();
            var fileName = string.Format("{0}.{1}", id, extension);
            var disposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };

            content.Headers.ContentDisposition = disposition;
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
