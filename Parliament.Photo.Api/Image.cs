namespace Parliament.Photo.Api.Controllers
{
    using System.IO;
    using XmpCore;

    public class Image
    {
        public Stream Bitmap;
        public IXmpMeta Metadata;
    }
}
