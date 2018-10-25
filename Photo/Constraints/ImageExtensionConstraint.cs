namespace Photo
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    internal class ImageExtensionConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var extension = values[routeKey] as string;

            return Configuration.PhotoMappings.Any(mapping => mapping.Extension == extension);
        }
    }
}
