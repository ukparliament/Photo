namespace Parliament.Photo.Api
{
    using ImageMagick;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class CachedImage
    {
        public MagickImage Image { get; set; }

        public CloudBlockBlob CacheBlob { get; set; }
    }
}