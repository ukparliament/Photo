namespace Parliament.Photo.Api
{
    using Parliament.Photo.Api.Controllers;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using XmpCore;
    using XmpCore.Impl;
    using XmpCore.Options;

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
            var bitmap = image.Bitmap;
            var format = this.SupportedMediaTypes.Single().MediaType;
            var metadata = ConvertMetadata(image.Metadata, format);
            var encoder = ChooseEncoder(format);

            encoder.Frames.Add(BitmapFrame.Create(bitmap, bitmap.Thumbnail, metadata, bitmap.ColorContexts));

            using (var interim = new MemoryStream())
            {
                encoder.Save(interim);
                interim.Seek(0, SeekOrigin.Begin);
                return interim.CopyToAsync(writeStream);
            }
        }

        private BitmapEncoder ChooseEncoder(string format)
        {
            var mapping = Global.mappingData.ToDictionary(row => row.MediaType, row => row.Formatter);

            if (!mapping.TryGetValue(format, out Type encoderType))
            {
                var supportedFormats = string.Join(", ", mapping.Keys);
                throw new NotSupportedException(string.Format("Supported formats are {0}.", supportedFormats));
            }

            return Activator.CreateInstance(encoderType) as BitmapEncoder;
        }

        private BitmapMetadata ConvertMetadata(IXmpMeta xmp, string mime)
        {
            var mapping = Global.mappingData.Where(row => row.MetadataFormat != null).ToDictionary(row => row.MediaType, row => row.MetadataFormat);

            if (!mapping.TryGetValue(mime, out string format))
            {
                return null;
            }

            var metadata = new BitmapMetadata(format);

            if (mime == "image/png")
            {
                var xmpString = XmpMetaFactory.SerializeToString(xmp, new SerializeOptions());

                metadata.SetQuery("/iTXt/Keyword", "XML:com.adobe.xmp".ToCharArray());
                metadata.SetQuery("/iTXt/TextEntry", xmpString);
            }
            else
            {
                var root = "/xmp";

                if (mime == "image/tiff")
                {
                    root = "/ifd/xmp";
                }

                foreach (var item in xmp.Properties)
                {
                    if (!item.Options.IsSchemaNode)
                    {
                        var qName = new QName(item.Path);

                        metadata.SetQuery($"{root}/{{wstr={item.Namespace}}}:{qName.GetLocalName()}", item.Value);
                    }
                }
            }

            return metadata;
        }
    }
}