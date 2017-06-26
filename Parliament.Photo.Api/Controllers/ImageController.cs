namespace Parliament.Photo.Api.Controllers
{
    using Microsoft.WindowsAzure.Storage;
    using System.Configuration;
    using System.Net;
    using System.Web.Http;
    using System.Windows.Media.Imaging;

    [ImageControllerConfiguration]
    public class ImageController : ApiController
    {
        public Image Get(string id, int? width = null, int? height = null, string crop = null)
        {
            var source = ImageController.GetRawSource(id);
            var metadata = new MetadataController().Get(id);

            return new Image
            {
                Bitmap = source,
                Metadata = metadata
            };
        }

        private static BitmapFrame GetRawSource(string id)
        {
            var connectionString = ConfigurationManager.AppSettings["PhotoStorage"];
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");
            var blob = container.GetBlobReference(id);

            if (!blob.Exists())
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return BitmapFrame.Create(blob.OpenRead());
        }
    }
}
