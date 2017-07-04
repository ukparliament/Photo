namespace Parliament.Photo.Api.Controllers
{
    using ImageMagick;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        public async Task<HttpResponseMessage> Get(string id, int? width = null, int? height = null, string crop = null, bool? download = null)
        {
            var blob = ImageController.GetRawSource(id);
            var exists = await blob.ExistsAsync();
            if (!exists)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var stream = await blob.OpenReadAsync();
            this.Request.RegisterForDispose(stream);

            if (width != null || height != null || crop != null)
            {
                stream = ResizeAndCrop(stream, width, height, crop);
                this.Request.RegisterForDispose(stream);
            }

            var metadataController = new MetadataController();
            this.Request.RegisterForDispose(metadataController);

            var image = new Image
            {
                Bitmap = stream,
                Metadata = metadataController.Get(id)
            };

            var response = this.Request.CreateResponse(image);

            if (download.GetValueOrDefault())
            {
                MakeDownload(id, response);
            }

            var etag = string.Format("\"{0}\"", string.Join("|", id, width, height, crop, download).GetHashCode());
            response.Headers.ETag = new EntityTagHeaderValue(etag);

            return response;
        }

        private static Stream ResizeAndCrop(Stream stream, int? width, int? height, string crop)
        {
            using (var magick = new MagickImage(stream))
            {
                if (crop != null)
                {
                    var x = 1800;
                    var y = 1000;
                    if (crop == "MCU_3:2")
                    {
                        magick.Crop(x - 1553, y - 789, 3108, 2072);
                    }
                    if (crop == "MCU_3:4")
                    {
                        magick.Crop(x - 789, y - 789, 1554, 2072);
                    }
                    if (crop == "CU_1:1")
                    {
                        magick.Crop(x - 789, y - 789, 1554, 1554);
                    }
                    if (crop == "CU_5:2")
                    {
                        magick.Crop(1, y - 670, 3795, 1518);
                    }
                }

                if (width != null || height != null)
                {
                    var size = new MagickGeometry(width.GetValueOrDefault(), height.GetValueOrDefault());
                    magick.ColorSpace = ColorSpace.RGB;
                    magick.Scale(size);
                    magick.ColorSpace = ColorSpace.sRGB;
                }

                var interim = new MemoryStream();
                magick.Write(interim);

                return interim;
            }
        }

        private static void MakeDownload(string id, HttpResponseMessage response)
        {
            var content = response.Content as ObjectContent;
            var formatterMapping = content.Formatter.MediaTypeMappings.First();
            var mediaType = formatterMapping.MediaType.MediaType;
            var mapping = Global.mappingData.Single(row => row.MediaType == mediaType);
            var extension = mapping.Extensions.First();
            var fileName = string.Format("{0}.{1}", id, extension);
            var disposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            content.Headers.ContentDisposition = disposition;
        }

        private static CloudBlob GetRawSource(string id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["PhotoStorage"].ConnectionString;
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");

            return container.GetBlobReference(id);
        }
    }
}
