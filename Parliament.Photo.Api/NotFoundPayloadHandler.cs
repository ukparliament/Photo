namespace Parliament.Photo.Api
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Routing;

    public class NotFoundPayloadHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response = await this.SendDefaultImageRequestAsync(request, cancellationToken);
                response.StatusCode = HttpStatusCode.NotFound;
            };

            return response;
        }

        private async Task<HttpResponseMessage> SendDefaultImageRequestAsync(HttpRequestMessage oldRequest, CancellationToken cancellationToken)
        {
            var newRequest = NotFoundPayloadHandler.Clone(oldRequest);

            return await base.SendAsync(newRequest, cancellationToken);
        }

        private static HttpRequestMessage Clone(HttpRequestMessage oldRequest)
        {
            var defaultImageId = ConfigurationManager.AppSettings["defaultImageId"];
            var defaultImageUri = new Uri(oldRequest.RequestUri, defaultImageId);
            var newRequest = new HttpRequestMessage(HttpMethod.Get, defaultImageUri);

            var data = oldRequest.GetRouteData();
            data.Values["id"] = defaultImageId;

            newRequest.SetRouteData(data);

            NotFoundPayloadHandler.CloneHeaders(oldRequest, newRequest);

            return newRequest;
        }

        private static void CloneHeaders(HttpRequestMessage oldRequest, HttpRequestMessage newRequest)
        {
            foreach (var item in oldRequest.Headers.Accept)
            {
                newRequest.Headers.Accept.Add(item);
            }
        }
    }
}