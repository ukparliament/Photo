namespace Photo
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using XmpCore;
    using XmpCore.Options;

    internal class MetadataFormatter : TextOutputFormatter
    {
        internal MetadataFormatter()
        {
            this.SupportedMediaTypes.Add("application/rdf+xml");
            this.SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IXmpMeta).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
        {
            var xmp = XmpMetaFactory.SerializeToString(context.Object as IXmpMeta, new SerializeOptions());

            using (var writer = context.WriterFactory(context.HttpContext.Response.Body, encoding))
            {
                return writer.WriteAsync(xmp);
            }
        }
    }
}
