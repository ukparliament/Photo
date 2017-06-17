namespace Parliament.Photo.Api.Controllers
{
    using System.IO;
    using System.Web.Hosting;
    using System.Web.Http;

    public class ImageController : ApiController
    {
        public Stream Get(string id, string ext = null, string format = null, int? width = null, int? height = null, string crop = null)
        {
            var source = getRawSource(id);
            return source;
        }

        private Stream getRawSource(string id)
        {
            return File.OpenRead(HostingEnvironment.MapPath("~/rdf.png"));
        }
    }
}
