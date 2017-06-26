namespace Parliament.Photo.Api.Controllers
{
    using System.Web.Http;
    using XmpCore;
    using XmpCore.Options;

    [MetadataControllerConfiguration]
    public class MetadataController : ApiController
    {
        public IXmpMeta Get(string id)
        {
            var xmp = XmpMetaFactory.Create();

            XmpMetaFactory.SchemaRegistry.RegisterNamespace("http://id.parliament.uk/", "id");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace("http://id.parliament.uk/schema/", "schema");

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

            // <rdf:Description rdf:about="http://id.parliament.uk/IMAGE1" />
            xmp.SetObjectName("http://id.parliament.uk/IMAGE1");

            // id:IMAGE1 a schema:Image
            xmp.SetProperty(XmpConstants.NsRdf, "type", "http://id.parliament.uk/schema/Image", new PropertyOptions { IsUri = true });

            // id:IMAGE1 schema:parlHasSubject id:PERSON1
            xmp.SetProperty("http://id.parliament.uk/schema/", "imageHasSubject", "http://id.parliament.uk/PERSON1", new PropertyOptions { IsUri = true });

            // id:PERSON1 a schema:Person
            xmp.SetQualifier("http://id.parliament.uk/schema/", "imageHasSubject", XmpConstants.NsRdf, "type", "http://id.parliament.uk/schema/Person", new PropertyOptions { IsUri = true });

            return xmp;
        }
    }
}
