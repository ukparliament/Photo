namespace Parliament.Photo.Api
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    public class ImageMediaFormatter : MediaTypeFormatter
    {
        public ImageMediaFormatter(MediaTypeMapping mapping)
        {
            this.SupportedMediaTypes.Add(mapping.MediaType);

            this.MediaTypeMappings.Add(mapping);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(Stream).IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var format = ChooseFormat();

            return Task.Factory.StartNew(() =>
            {
                var img = Image.FromStream(value as Stream);
                img.Save(writeStream, format);
            });
        }

        private ImageFormat ChooseFormat()
        {
            switch (this.SupportedMediaTypes.Single().MediaType)
            {
                case "image/png":
                    return ImageFormat.Png;

                case "image/jpeg":
                    return ImageFormat.Jpeg;

                case "image/tiff":
                    return ImageFormat.Tiff;

                case "image/gif":
                    return ImageFormat.Gif;

                case "image/bmp":
                    return ImageFormat.Bmp;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}