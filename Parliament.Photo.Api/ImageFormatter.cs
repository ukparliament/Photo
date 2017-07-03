namespace Parliament.Photo.Api
{
    using FreeImageAPI;
    using FreeImageAPI.Metadata;
    using ImageMagick;
    using Parliament.Photo.Api.Controllers;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using XmpCore;
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
            var format = this.SupportedMediaTypes.Single().MediaType;

            var encodedStream = Encode(image.Bitmap, format);
            var withMetadataStream = x(encodedStream, format, image.Metadata);

            withMetadataStream.Seek(0, SeekOrigin.Begin); // Needed?
            return withMetadataStream.CopyToAsync(writeStream);
        }

        private static MemoryStream Encode(Stream bitmap, string format)
        {
            var encoder = ChooseEncoder(format);
            var result = new MemoryStream();

            using (var magick = new MagickImage(bitmap))
            {
                magick.Write(result, encoder);
            }

            return result;
        }

        private Stream x(Stream originalStream, string format, IXmpMeta xmp)
        {

            var bitmap = FreeImage.LoadFromStream(originalStream);

            // FreeImage couldn't load
            if (bitmap.IsNull)
            {
                originalStream.Seek(0, SeekOrigin.Begin); // Needed?

                return originalStream;
            }

            // Strip existing metadata
            foreach (var item in new ImageMetadata(bitmap, true).List)
            {
                item.DestoryModel();
            }

            var xmpString = XmpMetaFactory.SerializeToString(xmp, new SerializeOptions());
            var fiXmp = new MDM_XMP(bitmap);
            fiXmp.Xml = xmpString;

            var fiFormat = FreeImage.GetFIFFromMime(format);

            // Because FI tries to seek on the stream.
            var resultStream = new MemoryStream();

            FreeImage.SaveToStream(bitmap, resultStream, fiFormat);
            resultStream.Seek(0, SeekOrigin.Begin); // Needed?

            return resultStream;
        }

        private static MagickFormat ChooseEncoder(string format)
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