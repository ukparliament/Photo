namespace PhotoTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.OpenApi;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Photo;

    [TestClass]
    public class IntegrationTests
    {
        private static WebApplicationFactory<Startup> factory;
        private static HttpClient client;

        public static IEnumerable<object[]> OpenApiExtensions
        {
            get
            {
                var noExtension = new[] { string.Empty };
                var allowedExtensions = Enum.GetNames(typeof(OpenApiFormat)).Select(name => $".{name.ToLower()}");

                return noExtension.Union(allowedExtensions).Select(e => (new[] { e }));
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            factory = new WebApplicationFactory<Startup>();
            client = factory.CreateClient();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            client.Dispose();
            factory.Dispose();
        }

        [TestMethod]
        [DynamicData(nameof(OpenApiExtensions))]
        public async Task OpenApi_endpoint_works(string extension)
        {
            using (var factory = new WebApplicationFactory<Startup>())
            {
                using (var client = factory.CreateClient())
                {
                    using (var response = await client.GetAsync($"/openapi{extension}"))
                    {
                        var reader = new OpenApiStreamReader();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            reader.Read(stream, out var diagnostic);

                            Assert.IsFalse(diagnostic.Errors.Any(), string.Join(",", diagnostic.Errors));
                        }
                    }
                }
            }
        }
    }
}
