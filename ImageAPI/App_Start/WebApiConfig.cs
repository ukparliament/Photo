using System.Net.Http.Formatting;
using System.Web.Http;

namespace ImageAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("DefaultWithExt", "{controller}/{id}.{ext}");
            config.Routes.MapHttpRoute("Default", "{controller}/{id}");

            config.Services.Replace(typeof(IContentNegotiator), new ImageContentNegotiator());

            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/bmp",new[] { "bmp" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/gif", new[] { "gif" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/jpeg", new[] { "jpe","jpeg","jpg" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/png", new[] { "png" })));
            config.Formatters.Add(new ImageMediaFormatter(new ImageRequestFormat("image/tiff", new[] { "tif", "tiff" })));

            config.MessageHandlers.Add(new CustomMessageHandler());
        }
    }
}
