namespace Parliament.Photo.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Formatting;

    public class ImageContentNegotiator : DefaultContentNegotiator
    {
        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            // Let the framework find a formatter.
            var baseResult = base.Negotiate(type, request, formatters);

            // Is this an image?
            if (typeof(Stream).IsAssignableFrom(type))
            {
                // Is it a non-image formatter?
                if (!(baseResult.Formatter is ImageMediaFormatter))
                {
                    // We shouldn't try to serialize it.
                    return null;
                }
            }

            // Otherwise let the framework serialize it.
            return baseResult;
        }
    }
}