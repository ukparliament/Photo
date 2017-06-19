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

            config.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());

            config.MessageHandlers.Add(new NotAcceptablePayloadHandler());

            config.MapHttpAttributeRoutes();

            config.EnsureInitialized();
        }
    }
}