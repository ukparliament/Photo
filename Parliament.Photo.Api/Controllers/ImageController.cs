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
    using System.Web.Http;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        public HttpResponseMessage Get(string id, int? width = null, int? height = null, string crop = null, bool? download = null)
        {
            var blob = ImageController.GetRawSource(id);

            if (!blob.Exists())
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var stream = blob.OpenRead();

            if (width != null || height != null || crop != null)
            {
                stream = ResizeAndCrop(stream, width, height, crop);
            }

            var image = new Image
            {
                Bitmap = stream,
                Metadata = new MetadataController().Get(id)
            };

            var response = this.Request.CreateResponse(image);

            if (download.GetValueOrDefault())
            {
                MakeDownload(id, response);
            }

            return response;
        }

        private static Stream ResizeAndCrop(Stream stream, int? width, int? height, string crop)
        {
            using (var magick = new MagickImage(stream))
            {
                if (crop != null)
                {
                    if (crop == "1")
                    {
                        magick.Crop(100, 100, 100, 100);
                    }
                    if (crop == "2")
                    {
                        magick.Crop(100, 100, 300, 200);
                    }
                    if (crop == "3")
                    {
                        magick.Crop(100, 100, 200, 300);
                    }
                }

                if (width != null || height != null)
                {
                    var size = new MagickGeometry(width.GetValueOrDefault(), height.GetValueOrDefault());
                    magick.Resize(size);
                }

                var interim = new MemoryStream();
                magick.Write(interim);
                stream.Dispose();

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
