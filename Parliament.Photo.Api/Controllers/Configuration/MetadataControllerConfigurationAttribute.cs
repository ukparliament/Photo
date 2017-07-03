namespace Parliament.Photo.Api.Controllers
{
    using System;
    using System.Net.Http.Formatting;
    using System.Web.Http.Controllers;

    public class MetadataControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Add(new XmpFormatter(new UriPathExtensionMapping("xmp", "application/rdf+xml")));
        }
    }
}
