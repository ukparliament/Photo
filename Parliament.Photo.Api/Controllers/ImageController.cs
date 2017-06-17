namespace Parliament.Photo.Api.Controllers
{
    using System;
    using System.IO;
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
            //TODO: Implement sample image
            throw new NotImplementedException();
        }
    }
}
