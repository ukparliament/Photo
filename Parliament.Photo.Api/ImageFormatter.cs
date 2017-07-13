namespace Parliament.Photo.Api
{
    using ImageMagick;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Web;
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
            return typeof(MagickImage).IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var magick = value as MagickImage;
            var format = this.SupportedMediaTypes.Single().MediaType;
            var encoder = ImageFormatter.ChooseEncoder(format);
            var telemetryClient = ImageFormatter.InitializeTelemetryClient(format, out EventTelemetry eventTelemetry);

            return Task.Factory.StartNew(() =>
            {
                magick.Write(writeStream, encoder);

                telemetryClient.TrackEvent(eventTelemetry);
            });
        }

        private static TelemetryClient InitializeTelemetryClient(string format, out EventTelemetry telemetry)
        {
            telemetry = new EventTelemetry("Write");
            telemetry.Properties.Add("format", format);

            // This makes the event part of the overall request context.
            var initializer = new OperationCorrelationTelemetryInitializer();
            initializer.Initialize(telemetry);

            return new TelemetryClient();
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