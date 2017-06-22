namespace Parliament.Photo.Api.Controllers
{
    using System.IO;
    using System.Net;
    using System.Web.Hosting;
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
            var path = HostingEnvironment.MapPath($"~/Images/{id}");

            if (!File.Exists(path))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return BitmapFrame.Create(File.OpenRead(path));
        }
    }
}
