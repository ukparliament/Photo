namespace ImageAPI
{
    public class ImageRequestFormat
    {
        public string MimeType { get; private set; }
        public string[] Extensions { get; private set; }

        public ImageRequestFormat(string mimeType, string[] extensions)
        {
            MimeType = mimeType;
            Extensions = extensions;
        }

    }
}