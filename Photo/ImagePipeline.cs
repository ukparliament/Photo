namespace Photo
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    internal class ImagePipeline
    {
        public void Configure(IOptions<MvcOptions> mvcOptions)
        {
            var mvc = mvcOptions.Value;

            mvc.RespectBrowserAcceptHeader = true;
            mvc.ReturnHttpNotAcceptable = true;

            mvc.OutputFormatters.Clear();

            foreach (var mapping in Startup.Mappings)
            {
                mvc.OutputFormatters.Add(new ImageFormatter(mapping.MediaType, mapping.Format));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(mapping.Extension, mapping.MediaType);
            }
        }
    }
}
