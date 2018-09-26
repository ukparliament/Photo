namespace Photo
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.SwaggerUI;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Program.Configuration = configuration.Get<Configuration>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(Startup.SetupMvc);
            services.Configure<RouteOptions>(Startup.ConfigureRouteOptions);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRewriter(new RewriteOptions().AddRewrite("^$", "swagger/index.html", false).AddRewrite("^(swagger|favicon)(.+)$", "swagger/$1$2", true));
            app.UseMvc();
            app.UseSwaggerUI(Startup.ConfigureSwaggerUI);
        }

        private static void SetupMvc(MvcOptions mvc)
        {
            mvc.RespectBrowserAcceptHeader = true;
            mvc.ReturnHttpNotAcceptable = true;

            mvc.OutputFormatters.Add(new MetadataFormatter());

            foreach (var mapping in Configuration.Mappings)
            {
                mvc.OutputFormatters.Add(new ImageFormatter(mapping.MediaType, mapping.Format));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(mapping.Extension, mapping.MediaType);
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(mapping.MediaType, mapping.MediaType);
            }
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
