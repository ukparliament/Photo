using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Web.Http;

namespace ImageAPI.Controllers
{
    public class ImageController : ApiController
    {
        public Stream Get(string id, string ext = null, string format = null, int? width = null, int? height = null, string crop = null)
        {
            Stream source = getRawSource(id);
            return source;
        }

        private Stream getRawSource(string id)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=testws123;AccountKey=2Bchwt3opW0s7CQIt4Ei3wHbnjb01J8X5iZUswdzS5CW1UbuTjN5Y8FgM+DspYGjCTTKIwxY/TNEi9OL37F9QQ==;EndpointSuffix=core.windows.net");
            CloudBlobClient blob = account.CreateCloudBlobClient();
            CloudBlobContainer container = blob.GetContainerReference("images");
            CloudBlob imageBlob = container.GetBlobReference($"{id}.tif");
            return imageBlob.OpenRead();
        }
    }
}
