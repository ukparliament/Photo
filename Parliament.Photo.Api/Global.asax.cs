namespace Parliament.Photo.Api
{
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;

    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            Global.ConfigureTelemetry(config);
            Global.ConfigureContentNegotiation(config);
            Global.ConfigureRouting(config);

            config.DependencyResolver = new DependencyResolver();
        }

        private static void ConfigureRouting(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("DefaultWithExt", "{controller}/{id}.{ext}");
            config.Routes.MapHttpRoute("Default", "{controller}/{id}");
        }

        private static void ConfigureTelemetry(HttpConfiguration config)
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];
            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());
        }

        private static void ConfigureContentNegotiation(HttpConfiguration config)
        {
            config.Services.Replace(typeof(IContentNegotiator), new ImageStreamNegotiator());
            config.MessageHandlers.Add(new NotAcceptablePayloadHandler());

            Global.AddFormatters(config);
        }

        private static void AddFormatters(HttpConfiguration config)
        {
            var mappingData = new[] {
                new {
                    MediaType = "image/png",
                    Extensions = new[] {
                        "png"
                    }
                },
                new {
                    MediaType = "image/jpeg",
                    Extensions = new[] {
                        "jpg",
                        "jpe",
                        "jpeg"
                    }
                },
                new {
                    MediaType = "image/gif",
                    Extensions = new[] {
                        "gif"
                    }
                },
                new {
                    MediaType = "image/tiff",
                    Extensions = new[] {
                        "tif",
                        "tiff"
                    }
                },
                new {
                    MediaType = "image/bmp",
                    Extensions = new[] {
                        "bmp"
                    }
                }
            };

            var mappings =
                // First add all the extension mappings.
                mappingData
                    .SelectMany(mapping => mapping.Extensions
                        .Select(extension => new UriPathExtensionMapping(extension, mapping.MediaType) as MediaTypeMapping))
                // Then add the query string mappings.
                .Union(mappingData
                    .Select(mapping => new QueryStringMapping("format", mapping.MediaType, mapping.MediaType) as MediaTypeMapping))
                // Finally add the accept header mappings.
                .Union(mappingData
                    .Select(mapping => new RequestHeaderMapping("Accept", mapping.MediaType, StringComparison.OrdinalIgnoreCase, false, mapping.MediaType) as MediaTypeMapping))
                .Select(mapping => new ImageStreamFormatter(mapping));

            config.Formatters.AddRange(mappings);
        }
    }
}