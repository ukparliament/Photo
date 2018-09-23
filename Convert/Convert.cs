namespace FunctionApp1
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ImageMagick;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json.Linq;

    public static class Convert
    {
        [FunctionName("Convert")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req)
        {
            var bodyJson = await req.Content.ReadAsAsync<JObject>();
            var photoId = bodyJson["PhotoID"].Value<string>();
            var imageResourceUri = bodyJson["ImageResourceUri"].Value<string>();
            var storage = bodyJson["Storage"].Value<string>();

            var account = CloudStorageAccount.Parse(storage);
            var client = account.CreateCloudBlobClient();
            
            var original = client.GetContainerReference("originals").GetBlockBlobReference(photoId);
            var modified = client.GetContainerReference("photo").GetBlockBlobReference(imageResourceUri);
            modified.Properties.ContentType = "image/tiff";

            using (var image = new MagickImage())
            {
                using (var stream = await original.OpenReadAsync())
                {
                    image.Read(stream);
                }

                image.Settings.Compression = CompressionMethod.LZW;

                using (var stream = await modified.OpenWriteAsync())
                {
                    image.Write(stream);
                }
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
