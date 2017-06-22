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

            XmpMetaFactory.SchemaRegistry.RegisterNamespace(XmpConstants.NsDC, "dc");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace(XmpConstants.NsIptccore, "Iptc4xmpCore");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace(XmpConstants.NsRdf, "rdf");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace("https://id.parliament.uk/schema/", "parl");

            xmp.SetProperty(XmpConstants.NsIptccore, "CiAdrCity", "London");
            xmp.SetProperty(XmpConstants.NsIptccore, "CiAdrCtry", "UK");
            xmp.SetProperty(XmpConstants.NsIptccore, "CiAdrRegion", "London");
            xmp.SetProperty(XmpConstants.NsIptccore, "CiEmailWork", "chris@mcandrewphoto.co.uk");
            xmp.SetProperty(XmpConstants.NsIptccore, "CiTelWork", "+447740424810");
            xmp.SetProperty(XmpConstants.NsIptccore, "CiUrlWork", "http://www.mcandrewphoto.co.uk");

            xmp.SetProperty(XmpConstants.NsPhotoshop, "Source", "Chris McAndrew / UK Parliament");
            xmp.SetProperty(XmpConstants.NsPhotoshop, "Credit", "Chris McAndrew / UK Parliament (Attribution 3.0 Unported (CC BY 3.0))");
            xmp.SetPropertyDate(XmpConstants.NsPhotoshop, "DateCreated", XmpDateTimeFactory.Create(2017, 6, 17, 11, 30, 41, 0));

            xmp.SetProperty(XmpConstants.NsDC, "rights", "Attribution 3.0 Unported (CC BY 3.0)");
            xmp.SetProperty(XmpConstants.NsDC, "title", ":firsNtame :lastName");
            xmp.SetProperty(XmpConstants.NsDC, "description", ":firstName :lastName - UK Parliament official portraits 2017");

            xmp.SetProperty("https://id.parliament.uk/schema/", "Person", "PERSON");
            xmp.SetProperty(XmpConstants.NsRdf, "type", new Uri("http://example.com/Person"), new PropertyOptions { IsUri = true });

            return xmp;
        }
    }
}
