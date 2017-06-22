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

            var mappings =
                // First add all the extension mappings.
                Global.mappingData
                    .SelectMany(mapping => mapping.Extensions
                        .Select(extension => new UriPathExtensionMapping(extension, mapping.MediaType) as MediaTypeMapping))
                // Then add the query string mappings.
                .Union(Global.mappingData
                    .Select(mapping => new QueryStringMapping("format", mapping.MediaType, mapping.MediaType) as MediaTypeMapping))
                // Finally add the accept header mappings.
                .Union(Global.mappingData
                    .Select(mapping => new RequestHeaderMapping("Accept", mapping.MediaType, StringComparison.OrdinalIgnoreCase, false, mapping.MediaType) as MediaTypeMapping))
                .Select(mapping => new ImageFormatter(mapping));

            controllerSettings.Formatters.AddRange(mappings);
        }
    }
}
