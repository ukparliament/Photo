namespace Parliament.Photo.Api
{
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Configuration;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];

            config.Routes.MapHttpRoute("DefaultWithExt", "{controller}/{id}.{ext}");
            config.Routes.MapHttpRoute("Default", "{controller}/{id}");

            config.Services.Replace(typeof(IContentNegotiator), new ImageContentNegotiator());

            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("png", "image/png")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("jpg", "image/jpeg")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("jpe", "image/jpeg")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("jpeg", "image/jpeg")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("gif", "image/gif")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("tif", "image/tiff")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("tiff", "image/tiff")));
            config.Formatters.Add(new ImageMediaFormatter(new UriPathExtensionMapping("bmp", "image/bmp")));
            config.Formatters.Add(new ImageMediaFormatter(new QueryStringMapping("format", "image/png", "image/png")));
            config.Formatters.Add(new ImageMediaFormatter(new QueryStringMapping("format", "image/jpeg", "image/jpeg")));
            config.Formatters.Add(new ImageMediaFormatter(new QueryStringMapping("format", "image/tiff", "image/tiff")));
            config.Formatters.Add(new ImageMediaFormatter(new QueryStringMapping("format", "image/gif", "image/gif")));
            config.Formatters.Add(new ImageMediaFormatter(new QueryStringMapping("format", "image/bmp", "image/bmp")));
            config.Formatters.Add(new ImageMediaFormatter(new RequestHeaderMapping("Accept", "image/png", StringComparison.OrdinalIgnoreCase, false, new MediaTypeHeaderValue("image/png"))));
            config.Formatters.Add(new ImageMediaFormatter(new RequestHeaderMapping("Accept", "image/jpeg", StringComparison.OrdinalIgnoreCase, false, new MediaTypeHeaderValue("image/jpeg"))));
            config.Formatters.Add(new ImageMediaFormatter(new RequestHeaderMapping("Accept", "image/tiff", StringComparison.OrdinalIgnoreCase, false, new MediaTypeHeaderValue("image/tiff"))));
            config.Formatters.Add(new ImageMediaFormatter(new RequestHeaderMapping("Accept", "image/gif", StringComparison.OrdinalIgnoreCase, false, new MediaTypeHeaderValue("image/gif"))));
            config.Formatters.Add(new ImageMediaFormatter(new RequestHeaderMapping("Accept", "image/bmp", StringComparison.OrdinalIgnoreCase, false, new MediaTypeHeaderValue("image/bmp"))));

            config.MessageHandlers.Add(new CustomMessageHandler());

            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());

            config.DependencyResolver = new DependencyResolver();
        }
    }
}