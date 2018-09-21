namespace Photo
{
    using System.Net;

    public class ImageParameters
    {
        public string Id { get; set; }

        public string MediaType { get; set; }

        public string Crop { get; set; }

        public int? Quality { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Format { get; set; }

        public bool Download { get; set; }

        public override string ToString()
        {
            var mediaType = WebUtility.UrlEncode(this.MediaType)?.ToLower() ?? string.Empty;

            return $"{this.Id}/{mediaType}/crop_{this.Crop}/dimensions_{this.Width}x{this.Height}/quality-{this.Quality}";
        }
    }
}
