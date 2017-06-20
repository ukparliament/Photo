namespace Parliament.Photo.Api
{
    using System.Collections.Generic;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;

    public class NoDefaultContentNegotiator : DefaultContentNegotiator
    {
        protected override bool ShouldMatchOnType(IEnumerable<MediaTypeWithQualityHeaderValue> sortedAcceptValues)
        {
            // When the request has no Accept header,
            // the base implementation still matches on type
            // (even if ExcludeMatchOnTypeOnly is true.)
            // This stops it.
            return false;
        }
    }
}