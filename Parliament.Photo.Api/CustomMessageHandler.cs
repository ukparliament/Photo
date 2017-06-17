namespace Parliament.Photo.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class CustomMessageHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotAcceptable)
            {
                var = request.GetConfiguration()
                    .Formatters
                    .Where(mediaFormatter => mediaFormatter.GetType() == typeof(ImageMediaFormatter))
                    .SelectMany(mediaFormatter => mediaFormatter.SupportedMediaTypes)
                    .Select(mediaType => mediaType.MediaType)
                    .Distinct()
                    .ToArray();

                response.Content = new StringContent(string.Join(",", mediaTypes));
            }

            return response;
        }

    }
}