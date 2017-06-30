namespace Parliament.Photo.Api
{
    using FreeImageAPI;
    using FreeImageAPI.Metadata;
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
            var stream = image.Bitmap;

            var bitmap = FreeImage.LoadFromStream(stream);

            foreach (var item in new ImageMetadata(bitmap, true).List)
            {
                item.DestoryModel();
            }

            var xmpMetadata = image.Metadata;
            var xmpString = XmpMetaFactory.SerializeToString(xmpMetadata, new SerializeOptions());
            var xmp = new MDM_XMP(bitmap);
            xmp.Xml = xmpString;

            var mime = this.SupportedMediaTypes.Single().MediaType;
            var format = FreeImage.GetFIFFromMime(mime);
            return Task.Factory.StartNew(() => FreeImage.SaveToStream(bitmap, writeStream, format));//TODO: fails on tiff regardless of xmp.
        }
    }
}