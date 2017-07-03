namespace Parliament.Photo.Api
{
    using ImageMagick;
    using Parliament.Photo.Api.Controllers;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    public class ImageFormatter : MediaTypeFormatter
    {
        public ImageFormatter(MediaTypeMapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }

            this.SupportedMediaTypes.Add(mapping.MediaType);
            this.MediaTypeMappings.Add(mapping);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(Image).IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var image = value as Image;
            var format = this.SupportedMediaTypes.Single().MediaType;
            var encoder = ChooseEncoder(format);

            return Task.Factory.StartNew(() =>
            {
                image.Bitmap.Seek(0, SeekOrigin.Begin);
                using (var magick = new MagickImage(image.Bitmap))
                {
                    magick.Write(writeStream, encoder);
                }
            });
        }

        private MagickFormat ChooseEncoder(string format)
        {
            var mapping = Global.mappingData.ToDictionary(row => row.MediaType, row => row.Formatter);

            if (!mapping.TryGetValue(format, out MagickFormat encoderType))
            {
                var supportedFormats = string.Join(", ", mapping.Keys);
                throw new NotSupportedException(string.Format("Supported formats are {0}.", supportedFormats));
            }

            return encoderType;
        }
    }
}