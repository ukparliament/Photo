namespace Parliament.Photo.Api.Controllers
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        public IHttpActionResult Get(string id, int? width = null, int? height = null, string crop = null)
        {
            var imageBlob = ImageController.GetImageBlob(id);

            if (!imageBlob.Exists())
            {
                return this.NotFound();
            }

            var image = new Image
            {
                Bitmap = imageBlob.OpenRead(),
                Metadata = new MetadataController().Get(id)
            };

            return this.Ok(image);
        }

        private static CloudBlob GetImageBlob(string id)
        {
            var connectionString = ConfigurationManager.AppSettings["PhotoStorage"];
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");

            return container.GetBlobReference(id);
        }
    }
}
