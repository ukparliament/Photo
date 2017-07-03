namespace Parliament.Photo.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class NotAcceptablePayloadHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Let the framework do its job.
            var response = await base.SendAsync(request, cancellationToken);

            // Did content negotiation fail (i.e. no adequate formatter was found)?
            if (response.StatusCode == HttpStatusCode.NotAcceptable)
            {
                // Get the image mime-types we support.
                var mediaTypes = request
                    .GetConfiguration()
                    .Formatters
                    .SelectMany(mediaFormatter => mediaFormatter.SupportedMediaTypes)
                    .Select(mediaType => mediaType.MediaType)
                    .Distinct();

                // Add them to the response as per https://tools.ietf.org/html/rfc7231#section-6.5.6
                response.Content = new StringContent(string.Join(",", mediaTypes));
            }

            return response;
        }
    }
}