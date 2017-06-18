namespace Parliament.Photo.Api
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    public class ImageStreamFormatter : MediaTypeFormatter
    {
        public ImageStreamFormatter(MediaTypeMapping mapping)
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
            return typeof(Stream).IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var format = this.ChooseFormat();

            return Task.Factory.StartNew(() =>
            {
                var img = Image.FromStream(value as Stream);
                img.Save(writeStream, format);
            });
        }

        private ImageFormat ChooseFormat()
        {
            var mapping = new Dictionary<string, ImageFormat>
            {
                { "image/png", ImageFormat.Png },
                { "image/jpeg", ImageFormat.Jpeg },
                { "image/tiff", ImageFormat.Tiff },
                { "image/gif", ImageFormat.Gif },
                { "image/bmp", ImageFormat.Bmp },
            };

            var current = this.SupportedMediaTypes.Single().MediaType;
            if (mapping.TryGetValue(current, out ImageFormat format))
            {
                return format;
            }

            throw new NotSupportedException();
        }
    }
}