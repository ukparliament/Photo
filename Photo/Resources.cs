namespace Photo
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Resources
    {
        public static JObject OpenApiDocument
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Photo.OpenApiDocumentTemplate.json"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            dynamic document = JObject.Load(jsonReader);
                            document.components.responses.imageResponse.content = new JObject(Configuration.Mappings.Select(m => new JProperty(m.MediaType, new JObject())));
                            document.paths["/{id}.{extension}"].get.parameters[1].schema["enum"] = new JArray(Configuration.Mappings.Select(m => m.Extension));

                            return document;
                        }
                    }
                }
            }
        }
    }
}
