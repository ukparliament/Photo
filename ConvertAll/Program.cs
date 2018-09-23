namespace ConsoleApp1
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
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
            var config = Program.Config;
            Program.functionUrl = config.GetValue<string>("FunctionUrl");
            Program.storage = config.GetValue<string>("Storage");

            Program.Run().Wait();
        }

        private static async Task Run()
        {
            var config = Config;
            var mappings = Mappings;
            var functionUrl = config.GetValue<string>("FunctionUrl");
            var storage = config.GetValue<string>("Storage");

            Parallel.ForEach<JToken>(Mappings.Children(), new ParallelOptions { MaxDegreeOfParallelism = 50 }, X);


            //for (var i = 0; i < mappings.Count; i++)
            //{
            //    var mapping = mappings[i];
            //    mapping["Storage"] = storage;

            //    using (var client = new HttpClient())
            //    {
            //        Console.WriteLine("{0} / {1}", i, mappings.Count);
            //        Console.WriteLine(mapping["PhotoID"]);
            //        Console.WriteLine(mapping["ImageResourceUri"]);

            //        var timer = Stopwatch.StartNew();

            //        var response = await client.PostAsync(functionUrl, new StringContent(mapping.ToString(), Encoding.UTF8, "application/json"));

            //        Console.WriteLine(response.StatusCode);
            //        Console.WriteLine(timer.Elapsed);
            //        Console.WriteLine();
            //    }
            //}
        }

        private static void X(JToken mapping, ParallelLoopState arg2, long arg3)
        {
            mapping["Storage"] = Program.storage;

            using (var client = new HttpClient())
            {
                Console.WriteLine("Start {0} ({1})", mapping["PhotoID"], arg3);

                var timer = Stopwatch.StartNew();

                var response = client.PostAsync(Program.functionUrl, new StringContent(mapping.ToString(), Encoding.UTF8, "application/json")).Result;

                Console.WriteLine("Finish {0} ({1}) with {2} in {3}", mapping["PhotoID"], arg3, response.StatusCode, timer.Elapsed);
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

        private static JArray Mappings
        {
            get
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("ConsoleApp1.SharePointExport.json"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            return JArray.Load(jsonReader);
                        }
                    }
                }
            }
        }
    }
}
