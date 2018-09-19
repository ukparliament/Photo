namespace Photo
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    internal class MetadataPipeline
    {
        public void Configure(IOptions<MvcOptions> mvcOptions)
        {
            var mvc = mvcOptions.Value;

            mvc.OutputFormatters.Clear();
            mvc.OutputFormatters.Add(new MetadataFormatter());
        }
    }
}