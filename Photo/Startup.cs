namespace Photo
{
    using ImageMagick;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.SwaggerUI;

    internal class Startup
    {
        internal static readonly (string MediaType, string Extension, MagickFormat Format)[] Mappings = new[] {
            ("image/jpeg", "jpg", MagickFormat.Jpg),
            ("image/png", "png", MagickFormat.Png),
            ("image/webp", "webp", MagickFormat.WebP),
            ("image/gif", "gif", MagickFormat.Gif),
            ("image/tiff", "tif", MagickFormat.Tif)
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<RouteOptions>(Startup.ConfigureRouteOptions);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRewriter(new RewriteOptions().AddRewrite("^$", "swagger/index.html", false).AddRewrite("^swagger-(.+)$", "swagger/swagger-$1", true));
            app.UseMvc();
            app.UseStaticFiles();
            app.UseSwaggerUI(Startup.ConfigureSwaggerUI);
        }

        private static void ConfigureRouteOptions(RouteOptions routes)
        {
            routes.ConstraintMap.Add("extension", typeof(ExtensionConstraint));
        }

        private static void ConfigureSwaggerUI(SwaggerUIOptions swaggerUI)
        {
            swaggerUI.SwaggerEndpoint("/openapi.json", "live");
        }
    }
}
