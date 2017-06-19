namespace Parliament.Photo.Api
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using XmpCore;

    public class XmpFormatter : MediaTypeFormatter
    {
        public XmpFormatter(MediaTypeMapping mapping)
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
            return typeof(IXmpMeta).IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                // Must serialize to an interime memory stream because XmpCore reads the position property of the stream it writes to, which is not supported for http response streams.
                using (var interim = new MemoryStream())
                {
                    XmpMetaFactory.Serialize(value as IXmpMeta, interim);
                    interim.WriteTo(writeStream);
                }
            });
        }
    }
}