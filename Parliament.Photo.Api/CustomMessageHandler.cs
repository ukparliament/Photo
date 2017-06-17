namespace Parliament.Photo.Api
{
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class CustomMessageHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if ((response != null) && (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable))
            {
                string[] mediaTypes = request.GetConfiguration()
                    .Formatters
                    .Where(mediaFormatter => mediaFormatter.GetType() == typeof(ImageMediaFormatter))
                    .SelectMany(mediaFormatter => mediaFormatter.SupportedMediaTypes)
                    .Select(mediaType => mediaType.MediaType)
                    .ToArray();
                response.Content = new StringContent(string.Join(",", mediaTypes), Encoding.UTF8, "text/plain");
            }

            return response;
        }

    }
}