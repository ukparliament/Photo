namespace ConsoleApp1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class Program
    {
        private static string functionUrl;
        private static string storage;

        private static void Main(string[] args)
        {
            while (true)
            {
                var config = Program.Config;

                Console.WriteLine("c: Convert");
                Console.WriteLine("w: Warm");
                Console.WriteLine("esc: Exit");
                Console.WriteLine();

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Console.WriteLine("Converting");
                        Console.WriteLine();

                        Program.functionUrl = config.GetValue<string>("FunctionUrl");
                        Program.storage = config.GetValue<string>("Storage");

                        Parallel.ForEach(Mappings, new ParallelOptions { MaxDegreeOfParallelism = 50 }, Convert);

                        Console.WriteLine();
                        Console.WriteLine("Done converting");
                        Console.WriteLine();

                        break;

                    case ConsoleKey.W:
                        Console.WriteLine("Warming cache");

                        var photoServiceUrl = config.GetValue<string>("PhotoServiceUrl");

                        Console.WriteLine("Getting usage data");

                        var rows = QueryAI().Result;

                        Console.WriteLine($"Got {rows.Count()} rows of usage data");

                        var resources = Mappings.Where(m => !m.Value<bool>("Photo Taken Down")).Select(m => m.Value<string>("ImageResourceUri"));
                        var urls = resources.SelectMany(resource => rows.Select(row => $"{photoServiceUrl}{resource}{row}"));

                        Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 50 }, Warm);

                        Console.WriteLine();
                        Console.WriteLine("Done warming cache");
                        Console.WriteLine();

                        break;

                    case ConsoleKey.Escape:
                        return;

                    default:
                        Console.WriteLine("Unkown");
                        Console.WriteLine();

                        break;
                }
            }
        }

        private static void Warm(string url, ParallelLoopState arg2, long arg3)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("{0:d} ({1})", response.StatusCode, url);
                }
            }
        }

        private static async Task<IEnumerable<Row>> QueryAI()
        {
            var aiApp = "330aa411-57af-491f-b22a-1cab7d08a6a9";
            var aiKey = "asi4quq3wdffokim0b2dpln8k8rya6gfl5n5g62g";
            var aiPeriod = "100"; // days

            var period = System.Xml.XmlConvert.ToString(TimeSpan.Parse(aiPeriod));
            var aiApi = $"https://api.applicationinsights.io/v1/apps/{aiApp}/query?timespan={period}";
            var query = JsonConvert.SerializeObject(new { query = PhotoUsageQuery });

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-api-key", aiKey);

                using (var content = new StringContent(query))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync(aiApi, content))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                using (var json = new JsonTextReader(reader))
                                {
                                    return JObject.Load(json)["tables"][0]["rows"].Select(r => new Row(r));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Convert(JToken mapping, ParallelLoopState arg2, long arg3)
        {
            mapping["Storage"] = Program.storage;

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(Program.functionUrl, new StringContent(mapping.ToString(), Encoding.UTF8, "application/json")).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("{0:d} {1} {2}", response.StatusCode, mapping["PhotoID"], mapping["ImageResourceUri"]);
                }
            }
        }

        private static IConfigurationRoot Config
        {
            get
            {
                var configBuilder = new ConfigurationBuilder();

                configBuilder.AddUserSecrets<Program>();

                return configBuilder.Build();
            }
        }

        private static JEnumerable<JToken> Mappings
        {
            get
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("ConvertAll.SharePointExport.json"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            return JArray.Load(jsonReader).Children();
                        }
                    }
                }
            }
        }

        private static string PhotoUsageQuery
        {
            get
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("ConvertAll.GetPhotoUsage.query"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }

    internal class Row
    {
        private readonly JToken original;

        public Row(JToken original)
        {
            this.original = original;
        }

        public string Format => this.original.Value<string>(0);

        public string Crop => this.original.Value<string>(1);

        public int? Width => this.original.Value<int?>(2);

        public int? Height => this.original.Value<int?>(3);

        public int? Quality => this.original.Value<int?>(4);

        public override string ToString()
        {
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(this.Format))
            {
                parameters.Add($"format={this.Format}");
            }

            if (!string.IsNullOrEmpty(this.Crop))
            {
                parameters.Add($"crop={this.Crop}");
            }

            if (!(this.Width is null))
            {
                parameters.Add($"width={this.Width}");
            }

            if (!(this.Height is null))
            {
                parameters.Add($"height={this.Height}");
            }

            if (!(this.Quality is null))
            {
                parameters.Add($"quality={this.Quality}");
            }

            parameters.Add("cacheonly=true");

            return $"?{string.Join("&", parameters)}";
        }
    }
}
