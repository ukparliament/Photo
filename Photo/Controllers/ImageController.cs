namespace Photo
{
    using System.Linq;
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
        [HttpGet("{id}.{format:image?}")]
        [FormatFilter]
        [TypeFilter(typeof(ImageFilter))]
        public async Task<ActionResult> Get(ImageParameters parameters)
        {
            try
            {
                return await this.Process(parameters);
            }
            catch (ImageNotFoundException)
            {
                return this.NotFound(parameters);
            }
            catch (UnkownCropException)
            {
                return this.IllegalCrop(parameters);
            }
        }

        private async Task<ActionResult> Process(ImageParameters parameters)
        {
            var details = await ImageDetails.GetById(parameters.Id);
            var image = await ImageController.GetImage(parameters.Id);
            this.Response.RegisterForDispose(image);

            ImageController.Crop(image, parameters, details);
            ImageController.Resize(image, parameters);
            ImageController.Quality(image, parameters);
            ImageController.Metadata(image, details);

            return this.Ok(image);
        }

        private ActionResult NotFound(ImageParameters parameters)
        {
            var image = ImageController.GetCaption("Not found");
            this.Response.RegisterForDispose(image);

            ImageController.Crop(image, parameters, new ImageDetails(3797 / 2, 5315 / 2));
            ImageController.Resize(image, parameters);
            ImageController.Quality(image, parameters);

            return this.NotFound(image);
        }

        private ActionResult IllegalCrop(ImageParameters parameters)
        {
            var caption = $"Acceptable crops:\n{string.Join("\n", Configuration.Crops.Select(m => $"{m.Name}"))}";
            var image = ImageController.GetCaption(caption);
            this.Response.RegisterForDispose(image);

            ImageController.Resize(image, parameters);

            return this.BadRequest(image);
        }

        private static void Crop(MagickImage image, ImageParameters parameters, ImageDetails details)
        {
            if (parameters.Crop is null)
            {
                return;
            }

            var crop = Configuration.Crops.SingleOrDefault(c => c.Name == parameters.Crop);

            if (crop.Name is null)
            {
                throw new UnkownCropException();
            }

            var x = 1;
            if (!(crop.OffsetX is null))
            {
                x = details.CenterX - crop.OffsetX.Value;
            }

            image.Crop(x, details.CenterY - crop.OffsetY, crop.Width, crop.Height);
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

        private static void Metadata(MagickImage image, ImageDetails details)
        {
            var xmp = MetadataController.Get(details);
            var xmpBuffer = XmpMetaFactory.SerializeToBuffer(xmp, new SerializeOptions());
            var xmpProfile = new XmpProfile(xmpBuffer);

            image.AddProfile(xmpProfile);
                       
            var iptcProfile = new IptcProfile();
            iptcProfile.SetValue(IptcTag.City, "London");
            iptcProfile.SetValue(IptcTag.Country, "UK");
            iptcProfile.SetValue(IptcTag.Contact, "chris@mcandrewphoto.co.uk, +447740424810,http://www.mcandrewphoto.co.uk");

            iptcProfile.SetValue(IptcTag.CopyrightNotice, "Attribution 3.0 Unported (CC BY 3.0)");
            iptcProfile.SetValue(IptcTag.Title, $"{details.GivenName} {details.FamilyName}");

            iptcProfile.SetValue(IptcTag.Source, "Chris McAndrew / UK Parliament");
            iptcProfile.SetValue(IptcTag.Credit, "Chris McAndrew / UK Parliament (Attribution 3.0 Unported (CC BY 3.0))");

            image.AddProfile(iptcProfile);
        }

        private static MagickImage GetCaption(string caption)
        {
            var image = new MagickImage(MagickColors.White, 3797, 5315);

            using (var label = new MagickImage($"label:{caption}", new MagickReadSettings { TextGravity = Gravity.Center, FontPointsize = 200 }))
            {
                image.Composite(label, Gravity.Center);
            }

            return image;
        }

        private static CloudBlob GetRawSource(string id)
        {
            var account = CloudStorageAccount.Parse(Program.Configuration.Storage.ConnectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(Program.Configuration.Storage.Container);
            var blob = container.GetBlobReference(id);

            return blob;
        }

        private static async Task<MagickImage> GetImage(string id)
        {
            var blob = GetRawSource(id);

            try
            {
                using (var stream = await blob.OpenReadAsync())
                {
                    return new MagickImage(stream);
                }
            }
            catch (StorageException e) when (e.Message == "The specified blob does not exist.")
            {
                throw new ImageNotFoundException();
            }
        }
    }
}
