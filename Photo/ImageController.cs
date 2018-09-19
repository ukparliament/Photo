namespace Photo
{
    using System;
    using System.Threading.Tasks;
    using ImageMagick;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using XmpCore;
    using XmpCore.Options;

    public class ImageController : Controller
    {
        [HttpGet("{id}.{format:extension?}")]
        [MiddlewareFilter(typeof(ImagePipeline))]
        [FormatFilter]
        [TypeFilter(typeof(ImageCaching))]
        public async Task<ActionResult> Get(ImageParameters parameters)
        {
            try
            {
                var image = await this.GetImage(parameters.Id);

                ImageController.Crop(image, 0, 0, parameters.Crop);
                ImageController.Resize(image, parameters.Width, parameters.Height);
                ImageController.AddMetadata(parameters.Id, image, new Uri("urn:x"), "given", "family");
                ImageController.Quality(image, parameters.Quality);

                return this.Ok(image);
            }
            catch (StorageException e) when (e.Message == "The specified blob does not exist.")
            {
                return this.NotFound();
            }
        }

        private static CloudBlob GetRawSource(string id)
        {
            var connectionString = "";
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("photo");
            var blob = container.GetBlobReference(id);

            return blob;
        }

        private async Task<MagickImage> GetImage(string id)
        {
            var blob = GetRawSource(id);

            using (var stream = await blob.OpenReadAsync())
            {
                var magick = new MagickImage(stream);

                this.Response.RegisterForDispose(magick);

                return magick;
            }
        }

        private static void Resize(MagickImage image, int? width, int? height)
        {
            if (width is null && height is null)
            {
                return;
            }

            var size = new MagickGeometry(
                width.GetValueOrDefault(image.Width),
                height.GetValueOrDefault(image.Height));

            image.ColorSpace = ColorSpace.RGB;
            image.Scale(size);
            image.RePage();
            image.ColorSpace = ColorSpace.sRGB;
        }

        private static void Quality(MagickImage image, int? quality)
        {
            if (!(quality is null))
            {
                image.Quality = quality.Value;
            }
        }

        private static void Crop(MagickImage magick, int x, int y, string crop)
        {
            switch (crop)
            {
                case null:
                    return;

                case "MCU_3:2":
                    magick.Crop(x - 1553, y - 789, 3108, 2072);
                    break;

                case "MCU_3:4":
                    magick.Crop(x - 789, y - 789, 1554, 2072);
                    break;

                case "CU_1:1":
                    magick.Crop(x - 789, y - 706, 1554, 1554);
                    break;

                case "CU_5:2":
                    magick.Crop(1, y - 670, 3795, 1518);
                    break;

                default:
                    break;
                    //throw new HttpResponseException(
                    //    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    //    {
                    //        Content = new StringContent("Allowed crop values:\n\tMCU_3:4\n\tMCU_3:2\n\tCU_1:1\n\tCU_5:2")
                    //    });
            }
        }

        private static void AddMetadata(string id, MagickImage magick, Uri member, string givenName, string familyName)
        {
            var xmp = MetadataController.Get(id, member, givenName, familyName);
            var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
            var xmpProfile = new XmpProfile(xmpBuffer);

            magick.AddProfile(xmpProfile);
        }

    }
}
