namespace Photo
{
    using System.Linq;
    using System.Reflection;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Exceptions;
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Readers;

    public static class Resources
    {
        public static OpenApiDocument OpenApiDocument
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Photo.Resources.OpenApiDocumentTemplate.json"))
                {
                    var reader = new OpenApiStreamReader();
                    var document = reader.Read(stream, out var diagnostic);

                    if (diagnostic.Errors.Any())
                    {
                        throw new OpenApiException(diagnostic.Errors.First().Message);
                    }

                    document.Components.Responses["imageResponse"].Content = Configuration.PhotoMappings.ToDictionary(m => m.MediaType, m => new OpenApiMediaType());
                    document.Components.Parameters["crop"].Schema.Enum = Configuration.Crops.Select(m => new OpenApiString(m.Name) as IOpenApiAny).ToList();
                    document.Paths["/{id}.{extension}"].Operations[OperationType.Get].Parameters[1].Schema.Enum = Configuration.PhotoMappings.Select(m => new OpenApiString(m.Extension) as IOpenApiAny).ToList();

                    return document;
                }
            }
        }
    }
}
