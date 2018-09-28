using ImageMagick;

namespace Photo
{
    internal class Configuration
    {
        internal static readonly (string MediaType, string Extension, MagickFormat Format)[] Mappings = new[] {
            ("image/jpeg", "jpg", MagickFormat.Jpg),
            ("image/png", "png", MagickFormat.Png),
            ("image/webp", "webp", MagickFormat.WebP),
            ("image/gif", "gif", MagickFormat.Gif),
            ("image/tiff", "tif", MagickFormat.Tif),
            ("image/x-icon", "ico", MagickFormat.Ico),
            ("application/pdf", "pdf", MagickFormat.Pdf)
        };

        internal static readonly (string Name, int? OffsetX, int OffsetY, int Width, int Height)[] Crops = new[] {
            ("MCU_3:2", 1553, 789, 3108, 2072),
            ("MCU_3:4", 789, 789, 1554, 2072),
            ("CU_1:1", 789, 706, 1554, 1554),
            ("CU_5:2", null as int?, 670, 3795, 1518)
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