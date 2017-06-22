namespace Parliament.Photo.Api
{
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;

    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];

            config.Routes.MapHttpRoute("Metadata", "{id}.xmp", new { controller = "Metadata" });
            config.Routes.MapHttpRoute("ImageWithExtension", "{id}.{ext}", new { controller = "Image" });
            config.Routes.MapHttpRoute("ImageNoExtension", "{id}", new { controller = "Image" });
            config.Routes.MapHttpRoute("CatchAllBadRequest", "{*uri}", new { controller = "BadRequest" });

            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());

            config.MessageHandlers.Add(new NotAcceptablePayloadHandler());
        }
    }
}