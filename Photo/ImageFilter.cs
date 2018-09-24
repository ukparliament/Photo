namespace Photo
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ImageMagick;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Headers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class ImageFilter : IAsyncActionFilter
    {
        private readonly MvcOptions options;
        private readonly OutputFormatterSelector selector;

        public ImageFilter(IOptions<MvcOptions> options, OutputFormatterSelector selector)
        {
            this.options = options.Value;
            this.selector = selector;
        }

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var parameters = this.GetParameters(context);

            var blob = ImageFilter.FindCacheBlob(parameters.ToString());

            var headers = new ResponseHeaders(context.HttpContext.Response.Headers);

            try
            {
                var cacheStream = await blob.OpenReadAsync();
                context.HttpContext.Response.RegisterForDispose(cacheStream);

                context.Result = parameters.CacheOnly ? new OkResult() as ActionResult : new FileStreamResult(cacheStream, parameters.MediaType) as ActionResult;

                headers.CacheControl = new CacheControlHeaderValue() { Public = true };
                headers.LastModified = blob.Properties.LastModified;
            }
            catch
            {
                var executedContext = await next();

                if (executedContext.Result is OkObjectResult)
                {
                    var cacheStream = await blob.OpenWriteAsync();
                    context.HttpContext.Response.RegisterForDispose(cacheStream);

                    var responseStream = parameters.CacheOnly ? Stream.Null : context.HttpContext.Response.Body;
                    context.HttpContext.Response.Body = new CacheStream(responseStream, cacheStream);

                    headers.CacheControl = new CacheControlHeaderValue() { Public = true };
                    headers.LastModified = DateTimeOffset.UtcNow;
                }
            }

            ImageFilter.Download(parameters, context.HttpContext);
        }

        private ImageParameters GetParameters(ActionExecutingContext context)
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

            key.MediaType = ((ImageFormatter)formatter)?.SupportedMediaTypes?.Single();

            return key;
        }

        private static CloudBlockBlob FindCacheBlob(string key)
        {
            var account = CloudStorageAccount.Parse(Program.Configuration.Cache.ConnectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(Program.Configuration.Cache.Container);

            return container.GetBlockBlobReference(key);
        }

        private static void Download(ImageParameters parameters, HttpContext context)
        {
            if (parameters.Download)
            {
                var mapping = Program.Configuration.Mappings.Single(row => row.MediaType == parameters.MediaType);
                var extension = mapping.Extension;
                var fileName = string.Format("{0}.{1}", parameters.Id, extension);
                var disposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };

                context.Response.Headers[HeaderNames.ContentDisposition] = disposition.ToString();
            }
        }
    }
}
