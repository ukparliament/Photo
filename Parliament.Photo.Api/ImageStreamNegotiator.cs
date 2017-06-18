namespace Parliament.Photo.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Formatting;

    public class ImageStreamNegotiator : DefaultContentNegotiator
    {
        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            // Let the framework do the actual content negotiation.
            var result = base.Negotiate(type, request, formatters);

            // Is this an image?
            if (typeof(Stream).IsAssignableFrom(type))
            {
                // Is it a non-image formatter?
                if (!(result.Formatter is ImageStreamFormatter))
                {
                    // Ignore this result. This is how we generate a 406.
                    return null;
                }
            }

            // Otherwise let the framework serialize it.
            return result;
        }
    }
}