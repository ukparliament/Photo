namespace Photo
{
    using System;
    using System.Threading.Tasks;
    using ImageMagick;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;

    internal class ImageFormatter : OutputFormatter
    {
        private readonly MagickFormat format;

        public ImageFormatter(string mediaType, MagickFormat format)
        {
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
            this.format = format;
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IMagickImage).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            return new TaskFactory().StartNew(() =>
            {
                var image = (IMagickImage)context.Object;
                image.Write(context.HttpContext.Response.Body, this.format);
            });
        }
    }
}
