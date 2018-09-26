namespace PhotoTests
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MyTestClass
    {
        [TestMethod]
        public void OpenApi_endpoint_works()
        {
            using (var factory = new WebApplicationFactory<Photo.Startup>())
            {
                using (var client = factory.CreateClient())
                {
                    using (var response = client.GetAsync("/openapi.json").Result)
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
