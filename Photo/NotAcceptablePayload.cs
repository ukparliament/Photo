namespace Photo
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Headers;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class NotAcceptablePayload
    {
        private readonly RequestDelegate next;

        public NotAcceptablePayload(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await this.next(context);

            if (context.Response.StatusCode == StatusCodes.Status406NotAcceptable)
            {
                var mediaTypes = Program.Configuration.Mappings.Select(m => m.MediaType);
                var json = new JObject(new JProperty("acceptable-content-types", mediaTypes));

                var headers = new ResponseHeaders(context.Response.Headers);
                headers.ContentType = new MediaTypeHeaderValue("application/json");
                headers.LastModified = null;
                headers.CacheControl = null;

                // Add them to the response as per https://tools.ietf.org/html/rfc7231#section-6.5.6
                using (var writer = new StreamWriter(context.Response.Body))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        await json.WriteToAsync(jsonWriter);
                    }
                }
            }
        }
    }
}
