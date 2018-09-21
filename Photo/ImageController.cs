namespace Photo
{
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
        [TypeFilter(typeof(ImageFilter))]
        public async Task<ActionResult> Get(ImageParameters parameters)
        {
            ImageDetails details;
            MagickImage image;

            try
            {
                details = await ImageDetails.GetById(parameters.Id);
                image = await this.GetImage(parameters.Id);

                ImageController.AddMetadata(image, details);
                ImageController.Process(image, parameters, details);

                return this.Ok(image);
            }
            catch (ImageNotFoundException)
            {
                image = Resources.NotFoundImage;
                this.Response.RegisterForDispose(image);
                details = new ImageDetails(1898, 1300);

                ImageController.Process(image, parameters, details);

                return this.NotFound(image);
            }
        }

        private static void Process(MagickImage image, ImageParameters parameters, ImageDetails details)
        {
            ImageController.Crop(image, parameters, details);
            ImageController.Resize(image, parameters);
            ImageController.Quality(image, parameters);
        }

        private static CloudBlob GetRawSource(string id)
        {
            var account = CloudStorageAccount.Parse(Program.Configuration.Storage.ConnectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(Program.Configuration.Storage.Container);
            var blob = container.GetBlobReference(id);

            return blob;
        }

        private async Task<MagickImage> GetImage(string id)
        {
            var blob = GetRawSource(id);

            try
            {
                using (var stream = await blob.OpenReadAsync())
                {
                    var magick = new MagickImage(stream);

                    this.Response.RegisterForDispose(magick);

                    return magick;
                }
            }
            catch (StorageException e) when (e.Message == "The specified blob does not exist.")
            {
                throw new ImageNotFoundException();
            }
        }

        private static void Resize(MagickImage image, ImageParameters parameters)
        {
            if (parameters.Width is null && parameters.Height is null)
            {
                return;
            }

            var size = new MagickGeometry(
                parameters.Width.GetValueOrDefault(image.Width),
                parameters.Height.GetValueOrDefault(image.Height));

            image.ColorSpace = ColorSpace.RGB;
            image.Scale(size);
            image.RePage();
            image.ColorSpace = ColorSpace.sRGB;
        }

        private static void Quality(MagickImage image, ImageParameters parameters)
        {
            if (!(parameters.Quality is null))
            {
                image.Quality = parameters.Quality.Value;
            }
        }

        private static void Crop(MagickImage magick, ImageParameters parameters, ImageDetails details)
        {
            switch (parameters.Crop)
            {
                case null:
                    return;

                case "MCU_3:2":
                    magick.Crop(details.CenterX - 1553, details.CenterY - 789, 3108, 2072);
                    break;

                case "MCU_3:4":
                    magick.Crop(details.CenterX - 789, details.CenterY - 789, 1554, 2072);
                    break;

                case "CU_1:1":
                    magick.Crop(details.CenterX - 789, details.CenterY - 706, 1554, 1554);
                    break;

                case "CU_5:2":
                    magick.Crop(1, details.CenterY - 670, 3795, 1518);
                    break;
            }
        }

        private static void AddMetadata(MagickImage magick, ImageDetails details)
        {
            var xmp = MetadataController.Get(details);
            var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
            var xmpProfile = new XmpProfile(xmpBuffer);

            magick.AddProfile(xmpProfile);
        }

    }
}
