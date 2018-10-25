namespace Photo
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class ImageDetails
    {
        private readonly JToken json;

        public ImageDetails(int x, int y)
        {
            this.json = new JObject(new JProperty("x", new JObject(new JProperty("value", x))), new JProperty("y", new JObject(new JProperty("value", y))));
        }

        internal ImageDetails(string id, JObject json)
        {
            this.Id = id;
            this.json = json["results"]["bindings"][0];
        }

        internal string Id { get; private set; }

        internal string MemberUri => this.GetValue<string>("member");

        internal int CenterX => this.GetValue<int>("x");

        internal int CenterY => this.GetValue<int>("y");

        internal string GivenName => this.GetValue<string>("givenName");

        internal string FamilyName => this.GetValue<string>("familyName");

        internal static async Task<ImageDetails> GetById(string id)
        {
            var endpoint = new Uri(new Uri(Program.Configuration.Query.Endpoint), $"photo_details?photo_id={id}");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Program.Configuration.Query.SubscriptionKey);
                client.DefaultRequestHeaders.Add("Api-Version", Program.Configuration.Query.ApiVersion);

                var serializer = new JsonSerializer();

                try
                {
                    using (var stream = await client.GetStreamAsync(endpoint))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            using (var jsonReader = new JsonTextReader(reader))
                            {
                                return new ImageDetails(id, serializer.Deserialize(jsonReader) as JObject);
                            }
                        }
                    }
                }
                catch (HttpRequestException e) when (e.Message.Contains("404"))
                {
                    throw new ImageNotFoundException();
                }
            }
        }

        private T GetValue<T>(string key)
        {
            return this.json[key]["value"].Value<T>();
        }
    }
}
