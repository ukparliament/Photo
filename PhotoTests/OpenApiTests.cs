namespace PhotoTests
{
    using System.Linq;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OpenApiTests
    {
        [TestMethod]
        public void OpenApi_document_is_valid()
        {
            var reader = new OpenApiStringReader();
            reader.Read(Photo.Resources.OpenApiDocument.ToString(), out var diagnostic);

            Assert.IsFalse(diagnostic.Errors.Any(), string.Join(",", diagnostic.Errors));
        }
    }
}
