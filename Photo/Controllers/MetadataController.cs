// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Photo
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using XmpCore;
    using XmpCore.Options;

    public class MetadataController : Controller
    {
        [HttpGet("{id}.xmp")]
        [Produces("application/rdf+xml")]
        public async Task<IXmpMeta> Get(string id)
        {
            var details = await ImageDetails.GetById(id);

            return MetadataController.Get(details);
        }

        internal static IXmpMeta Get(ImageDetails details)
        {
            var xmp = XmpMetaFactory.Create();

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
            xmp.SetProperty(XmpConstants.NsDC, "title", $"{details.GivenName} {details.FamilyName}");
            xmp.SetProperty(XmpConstants.NsDC, "description", $"{details.GivenName} {details.FamilyName} - UK Parliament official portraits 2017");
            xmp.SetProperty(XmpConstants.NsDC, "creator", "Chris McAndrew / UK Parliament");

            // <rdf:Description rdf:about="http://id.parliament.uk/IMAGE1" />
            xmp.SetObjectName($"{idNs}{details.Id}");

            // id:IMAGE1 a schema:Image
            xmp.SetProperty(XmpConstants.NsRdf, "type", $"{schemaNs}Image", new PropertyOptions { IsUri = true });

            // id:IMAGE1 schema:parlHasSubject id:PERSON1
            xmp.SetProperty(schemaNs, "imageHasSubject", details.MemberUri, new PropertyOptions { IsUri = true });

            // id:PERSON1 a schema:Person
            xmp.SetQualifier(schemaNs, "imageHasSubject", XmpConstants.NsRdf, "type", $"{schemaNs}Person", new PropertyOptions { IsUri = true });

            return xmp;
        }
    }
}