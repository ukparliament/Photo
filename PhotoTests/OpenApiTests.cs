namespace PhotoTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OpenApiTests
    {
        [TestMethod]
        public void OpenApi_document_is_valid()
        {
            var document = Photo.Resources.OpenApiDocument;
        }
    }
}
