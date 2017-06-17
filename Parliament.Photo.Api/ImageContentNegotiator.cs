namespace Parliament.Photo.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;

    public class ImageContentNegotiator : DefaultContentNegotiator
    {
        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            var mediaTypes = formatters
                .Where(mediaFormatter => mediaFormatter.GetType() == typeof(ImageMediaFormatter))
                .SelectMany(mediaFormatter => mediaFormatter.SupportedMediaTypes.Select(mediaType =>
                    new KeyValuePair<string, MediaTypeFormatter>(mediaType.MediaType, mediaFormatter)))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var extensions = formatters
                .Where(mediaFormatter => mediaFormatter.GetType() == typeof(ImageMediaFormatter))
                .SelectMany(mediaFormatter => mediaFormatter.MediaTypeMappings.Where(mappings =>
                    mappings.GetType() == typeof(UriPathExtensionMapping))
                    .Select(mediaType =>
                        new KeyValuePair<string, MediaTypeFormatter>(((UriPathExtensionMapping)mediaType).UriPathExtension, mediaFormatter)))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            if ((request.Headers != null) && (request.Headers.Accept != null) && (mediaTypes.ContainsKey(request.Headers.Accept.ToString())))
            {
                return new ContentNegotiationResult(mediaTypes[request.Headers.Accept.ToString()], new System.Net.Http.Headers.MediaTypeHeaderValue(request.Headers.Accept.ToString()));
            }
            else
            {
                var queryNameValue = request.GetQueryNameValuePairs().FirstOrDefault(nameValue => nameValue.Key.Equals("format", StringComparison.CurrentCultureIgnoreCase) && mediaTypes.ContainsKey(nameValue.Value));
                if (queryNameValue.Equals(default(KeyValuePair<string, string>)) == false)
                {
                    return new ContentNegotiationResult(mediaTypes[queryNameValue.Value], new System.Net.Http.Headers.MediaTypeHeaderValue(queryNameValue.Value));
                }
                else
                {
                    if ((request.GetRouteData() != null) && (request.GetRouteData().Values["ext"] != null) && (extensions.ContainsKey(request.GetRouteData().Values["ext"].ToString())))
                    {
                        return new ContentNegotiationResult(extensions[request.GetRouteData().Values["ext"].ToString()], new System.Net.Http.Headers.MediaTypeHeaderValue(extensions[request.GetRouteData().Values["ext"].ToString()].SupportedMediaTypes.FirstOrDefault().MediaType));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}