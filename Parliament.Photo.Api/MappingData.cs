namespace Parliament.Photo.Api.Controllers
{
    using ImageMagick;
    using System.Collections.Generic;

    public class MappingData
    {
        public string MediaType;
        public IEnumerable<string> Extensions;
        public string MetadataFormat;
        public MagickFormat Formatter;
    }
}