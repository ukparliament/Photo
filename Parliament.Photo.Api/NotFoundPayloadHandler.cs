namespace Parliament.Photo.Api
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class NotFoundPayloadHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response = await this.SendDefaultImageRequestAsync(request, cancellationToken);
                response.StatusCode = HttpStatusCode.NotFound;
            }

            return response;
        }

        private async Task<HttpResponseMessage> SendDefaultImageRequestAsync(HttpRequestMessage oldRequest, CancellationToken cancellationToken)
        {
            var newRequest = NotFoundPayloadHandler.Clone(oldRequest);

            return await base.SendAsync(newRequest, cancellationToken);
        }

        private static HttpRequestMessage Clone(HttpRequestMessage oldRequest)
        {
            var notFoundImageId = ConfigurationManager.AppSettings["NotFoundImageId"];
            var routeData = oldRequest.GetRouteData();
            var oldImageId = routeData.Values["id"] as string;
            routeData.Values["id"] = notFoundImageId;

            var newRequestUri = oldRequest.RequestUri.ToString().Replace(oldImageId, notFoundImageId);
            var newRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(newRequestUri));
            newRequest.SetRouteData(routeData);

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