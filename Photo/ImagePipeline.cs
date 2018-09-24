namespace Photo
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    internal class ImagePipeline
    {
        public void Configure(IApplicationBuilder app, IOptions<MvcOptions> mvcOptions)
        {
            app.UseMiddleware<NotAcceptablePayload>();

            var mvc = mvcOptions.Value;

            mvc.RespectBrowserAcceptHeader = true;
            mvc.ReturnHttpNotAcceptable = true;

            mvc.OutputFormatters.Clear();

            foreach (var mapping in Program.Configuration.Mappings)
            {
                mvc.OutputFormatters.Add(new ImageFormatter(mapping.MediaType, mapping.Format));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(mapping.Extension, mapping.MediaType);
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(mapping.MediaType, mapping.MediaType);
            }
        }
    }
}
