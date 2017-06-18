namespace Parliament.Photo.Api.Controllers
{
    using System.IO;
    using System.Net;
    using System.Web.Hosting;
    using System.Web.Http;

    public class ImageController : ApiController
    {
        public Stream Get(string id, string ext = null, string format = null, int? width = null, int? height = null, string crop = null)
        {
            var source = ImageController.GetRawSource(id);
            return source;
        }

        private static Stream GetRawSource(string id)
        {
            var path = HostingEnvironment.MapPath($"~/{id}");

            if (!File.Exists(path))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return File.OpenRead(path);
        }
    }
}
