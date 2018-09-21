using ImageMagick;

namespace Photo
{
    internal class Configuration
    {
        internal readonly (string MediaType, string Extension, MagickFormat Format)[] Mappings = new[] {
            ("image/jpeg", "jpg", MagickFormat.Jpg),
            ("image/png", "png", MagickFormat.Png),
            ("image/webp", "webp", MagickFormat.WebP),
            ("image/gif", "gif", MagickFormat.Gif),
            ("image/tiff", "tif", MagickFormat.Tif)
        };

        public StorageConfiguration Storage { get; set; }

        public StorageConfiguration Cache { get; set; }

        public QueryConfiguration Query { get; set; }

        internal class StorageConfiguration
        {
            public string ConnectionString { get; set; }

            public string Container { get; set; }
        }

        internal class QueryConfiguration
        {
            public string Endpoint { get; set; }

            public string ApiVersion { get; set; }

            public string SubscriptionKey { get; set; }
        }
    }
}