namespace CQ.Blobs.AspNetCore.Tests;

[TestClass]
public sealed class BlobHttpErrorsTests
{
    /// <summary>
    /// Una excepción nueva sin entrada en el catálogo llegaría a las APIs como un 500.
    /// </summary>
    [TestMethod]
    public void The_catalog_covers_every_blob_exception()
    {
        var exceptionTypes = new[] { typeof(BlobException).Assembly, typeof(BlobHttpErrors).Assembly }
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => !t.IsAbstract && typeof(BlobException).IsAssignableFrom(t))
            .ToList();

        var missing = exceptionTypes
            .Where(t => BlobHttpErrors.All.All(e => e.ExceptionType != t))
            .Select(t => t.Name)
            .ToList();

        Assert.IsTrue(exceptionTypes.Count >= 6);
        Assert.AreEqual(0, missing.Count, $"Missing: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void Each_code_matches_the_exception_message()
    {
        foreach (var error in BlobHttpErrors.All)
        {
            // Primer parámetro "x" (la key o el content type) y el resto null: Activator no
            // completa los parámetros opcionales.
            var constructor = error.ExceptionType.GetConstructors().Single();
            var arguments = constructor
                .GetParameters()
                .Select((_, index) => index == 0 ? (object?)"x" : null)
                .ToArray();
            var exception = (BlobException)constructor.Invoke(arguments);

            Assert.AreEqual(error.Code, exception.Code, error.ExceptionType.Name);
            Assert.AreEqual(error.Code, exception.Message, error.ExceptionType.Name);
        }
    }

    [TestMethod]
    public void For_returns_null_for_other_exceptions()
    {
        Assert.IsNull(BlobHttpErrors.For(new InvalidOperationException("x")));
    }
}
