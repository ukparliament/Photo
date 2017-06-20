namespace Parliament.Photo.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web.Http.Controllers;

    public class ImageControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Services.Replace(typeof(IContentNegotiator), new NoDefaultContentNegotiator());
            controllerSettings.Formatters.Clear();

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

            controllerSettings.Formatters.AddRange(mappings);
        }
    }
}
