namespace Photo
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using ImageMagick;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class ImageCaching : IActionFilter
    {
        private readonly MvcOptions options;
        private readonly OutputFormatterSelector selector;

        public ImageCaching(IOptions<MvcOptions> options, OutputFormatterSelector selector)
        {
            this.options = options.Value;
            this.selector = selector;
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            var key = this.GetKey(context);

            var blob = FindCacheBlob(key.ToString());

            if (blob.ExistsAsync().Result)
            {
                var cacheStream = blob.OpenReadAsync().Result;
                context.HttpContext.Response.RegisterForDispose(cacheStream);
                context.Result = new FileStreamResult(cacheStream, key.MediaType);
            }
            else
            {
                //var cacheStream = blob.OpenWriteAsync().Result;
                //context.HttpContext.Response.RegisterForDispose(cacheStream);
                //context.HttpContext.Response.Body = new CacheStream(context.HttpContext.Response.Body, cacheStream);
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context) { }

        private ImageParameters GetKey(ActionExecutingContext context)
        {
            var key = (ImageParameters)context.ActionArguments["parameters"];

            var mediaTypes = new MediaTypeCollection();
            if (!(key.Format is null))
            {
                mediaTypes.Add(
                    new MediaTypeHeaderValue(
                        this.options.FormatterMappings.GetMediaTypeMappingForFormat(
                            key.Format)));
            }


            var formatter = this.selector.SelectFormatter(
                new OutputFormatterWriteContext(
                    context.HttpContext,
                    (Stream s, Encoding e) => TextWriter.Null,
                    typeof(MagickImage),
                    null),
                this.options.OutputFormatters,
                mediaTypes);

            key.MediaType = ((ImageFormatter)formatter).SupportedMediaTypes.Single();

            return key;
        }

        private static CloudBlockBlob FindCacheBlob(string key)
        {
            var connectionString = "";
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("cache");

            return container.GetBlockBlobReference(key);
        }
    }
}
