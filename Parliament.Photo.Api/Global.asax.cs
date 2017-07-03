namespace Parliament.Photo.Api
{
    using ImageMagick;
    using Microsoft.ApplicationInsights.Extensibility;
    using Parliament.Photo.Api.Controllers;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.ExceptionHandling;

    public class Global : HttpApplication
    {
        public static readonly IEnumerable<MappingData> mappingData = new MappingData[] {
                new MappingData {
                    MediaType = "image/png",
                    Extensions = new[] {
                        "png"
                    },
                    MetadataFormat = "png",
                    Formatter = MagickFormat.Png
                },
                new MappingData {
                    MediaType = "image/jpeg",
                    Extensions = new[] {
                        "jpg",
                        "jpe",
                        "jpeg"
                    },
                    MetadataFormat = "jpg",
                    Formatter = MagickFormat.Jpg
                },
                new MappingData {
                    MediaType = "image/gif",
                    Extensions = new[] {
                        "gif"
                    },
                    MetadataFormat = "gif",
                    Formatter = MagickFormat.Gif
                },
                new MappingData {
                    MediaType = "image/tiff",
                    Extensions = new[] {
                        "tif",
                        "tiff"
                    },
                    MetadataFormat = "tiff",
                    Formatter = MagickFormat.Tif
                },
                new MappingData {
                    MediaType = "image/bmp",
                    Extensions = new[] {
                        "bmp"
                    },
                    Formatter = MagickFormat.Bmp
                }
            };

        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];

            var imagePipeline = HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(config),
                new DelegatingHandler[] {
                    new NotAcceptablePayloadHandler(),
                    new NotFoundPayloadHandler()
            });

            config.Routes.MapHttpRoute("Metadata", "{id}.xmp", new { controller = "Metadata" });
            config.Routes.MapHttpRoute("ImageWithExtension", "{id}.{ext}", new { controller = "Image" }, null, imagePipeline);
            config.Routes.MapHttpRoute("ImageNoExtension", "{id}", new { controller = "Image" }, null, imagePipeline);
            config.Routes.MapHttpRoute("CatchAllBadRequest", "{*uri}", new { controller = "BadRequest" });

            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());

            config.Formatters.Clear();
            config.Formatters.Add(new HttpErrorJsonFormatter());
            config.Formatters.Add(new HttpErrorXmlFormatter());
        }
    }
}