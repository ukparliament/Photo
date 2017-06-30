namespace Parliament.Photo.Api
{
    using System;
    using System.Net.Http.Formatting;
    using System.Web.Http;

    public class HttpErrorJsonFormatter : JsonMediaTypeFormatter
    {
        public override bool CanWriteType(Type type)
        {
            return typeof(HttpError).IsAssignableFrom(type);
        }
    }
}