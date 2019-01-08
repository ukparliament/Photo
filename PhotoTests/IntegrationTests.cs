// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
