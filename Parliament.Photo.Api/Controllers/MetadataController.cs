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
            ImageController.Query(id, out Uri member, out string givenName, out string familyName, out int x, out int y);

            return this.Get(id, member, givenName, familyName);
        }

        public IXmpMeta Get(string id, Uri member, string givenName, string familyName)
        {
            var xmp = XmpMetaFactory.Create();

            // TODO: Change to proper namespace
            var idNs = "https://id.parliament.uk/";
            var schemaNs = $"{idNs}schema/";

            XmpMetaFactory.SchemaRegistry.RegisterNamespace(idNs, "id");
            XmpMetaFactory.SchemaRegistry.RegisterNamespace(schemaNs, "schema");

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
            xmp.SetProperty(XmpConstants.NsDC, "title", $"{givenName} {familyName}");
            xmp.SetProperty(XmpConstants.NsDC, "description", $"{givenName} {familyName} - UK Parliament official portraits 2017");

            // <rdf:Description rdf:about="http://id.parliament.uk/IMAGE1" />
            xmp.SetObjectName($"{idNs}{id}");

            // id:IMAGE1 a schema:Image
            xmp.SetProperty(XmpConstants.NsRdf, "type", $"{schemaNs}Image", new PropertyOptions { IsUri = true });

            // id:IMAGE1 schema:parlHasSubject id:PERSON1
            xmp.SetProperty(schemaNs, "imageHasSubject", member, new PropertyOptions { IsUri = true });

            // id:PERSON1 a schema:Person
            xmp.SetQualifier(schemaNs, "imageHasSubject", XmpConstants.NsRdf, "type", $"{schemaNs}Person", new PropertyOptions { IsUri = true });

            return xmp;
        }
    }
}
