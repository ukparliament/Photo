namespace PhotoTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.OpenApi;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IntegrationTests
    {
        public static IEnumerable<object[]> Extensions
        {
            get
            {
                var noExtension = new[] { string.Empty };
                var allowedExtensions = Enum.GetNames(typeof(OpenApiFormat)).Select(name => $".{name.ToLower()}");

                return noExtension.Union(allowedExtensions).Select(e => (new[] { e }));
            }
        }

        [TestMethod]
        [DynamicData(nameof(Extensions))]
        public void OpenApi_endpoint_works(string extension)
        {
            using (var factory = new WebApplicationFactory<Photo.Startup>())
            {
                using (var client = factory.CreateClient())
                {
                    using (var response = client.GetAsync($"/openapi{extension}").Result)
                    {
                        var reader = new OpenApiStreamReader();

                        using (var stream = response.Content.ReadAsStreamAsync().Result)
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
