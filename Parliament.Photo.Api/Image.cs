namespace Parliament.Photo.Api.Controllers
{
    using System.IO;
    using System.Net;
    using XmpCore;

    public class Image
    {
        public Stream Bitmap;
        public IXmpMeta Metadata;
        public HttpStatusCode Status;
    }
}
