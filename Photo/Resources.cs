namespace Photo
{
    using System.Reflection;
    using ImageMagick;

    public static class Resources
    {
        internal static MagickImage NotFoundImage
        {
            get
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Photo.NotFound.png"))
                {
                    return new MagickImage(stream);
                }
            }
        }
    }
}
