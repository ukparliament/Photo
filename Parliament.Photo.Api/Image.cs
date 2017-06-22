namespace Parliament.Photo.Api.Controllers
{
    using System.Windows.Media.Imaging;
    using XmpCore;

    public class Image
    {
        public BitmapFrame Bitmap;
        public IXmpMeta Metadata;
    }
}
