namespace Parliament.Photo.Api
{
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Configuration;
    using System.Net.Http.Formatting;
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

            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/bmp", new[] { "bmp" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/gif", new[] { "gif" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/jpeg", new[] { "jpe", "jpeg", "jpg" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/png", new[] { "png" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/tiff", new[] { "tif", "tiff" })));

            config.MessageHandlers.Add(new CustomMessageHandler());

            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());

            config.DependencyResolver = new DependencyResolver();
        }
    }
}