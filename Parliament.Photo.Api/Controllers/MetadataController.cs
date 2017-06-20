namespace Parliament.Photo.Api.Controllers
{
    using System;
    using System.Web.Http;
    using XmpCore;
    using XmpCore.Options;

    [MetadataControllerConfiguration]
    public class MetadataController : ApiController
    {
        public IXmpMeta Get(string id)
        {
            var xmp = XmpMetaFactory.Create();

            XmpMetaFactory.SchemaRegistry.RegisterNamespace("http://purl.org/dc/elements/1.1/", "dc");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace("https://id.parliament.uk/schema/", "parl");

            xmp.SetProperty("http://purl.org/dc/elements/1.1/", "title", "TITLE");
            xmp.SetProperty("http://purl.org/dc/elements/1.1/", "description", "DESCRIPTION");
            xmp.SetProperty("https://id.parliament.uk/schema/", "Person", "PERSON");
            xmp.SetProperty("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "type", new Uri("http://example.com/Person"), new PropertyOptions { IsUri = true });

            return xmp;
        }
    }
}
