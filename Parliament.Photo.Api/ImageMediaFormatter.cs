namespace Parliament.Photo.Api
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;

    public class ImageMediaFormatter : MediaTypeFormatter
    {
        public ImageMediaFormatter(ImageRequestFormat imageRequestFormat)
        {
            var mediaType = new MediaTypeHeaderValue(imageRequestFormat.MimeType);
            SupportedMediaTypes.Add(mediaType);
            foreach (string extension in imageRequestFormat.Extensions)
            {
                this.AddUriPathExtensionMapping(extension, mediaType);
            }

            this.AddQueryStringMapping("format", imageRequestFormat.MimeType, mediaType);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(Stream);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var imageFormat = getFormatOutput(HttpContext.Current.Request);
            return Task.Factory.StartNew(() =>
            {
                var img = Image.FromStream(((Stream)value));
                img.Save(writeStream, imageFormat);
            });
        }

        public ImageFormat getFormatOutput(HttpRequest request)
        {
            ImageFormat imageFormat = null;
            switch (this.SupportedMediaTypes[0].MediaType)
            {
                case "image/bmp":
                    imageFormat = ImageFormat.Bmp;
                    break;

                case "image/gif":
                    imageFormat = ImageFormat.Gif;
                    break;

                case "image/jpeg":
                    imageFormat = ImageFormat.Jpeg;
                    break;

                case "image/png":
                    imageFormat = ImageFormat.Png;
                    break;

                case "image/tiff":
                    imageFormat = ImageFormat.Tiff;
                    break;
            }

            return imageFormat;
        }
    }
}