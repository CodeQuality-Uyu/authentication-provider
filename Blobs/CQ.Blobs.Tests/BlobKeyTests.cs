namespace CQ.Blobs.Tests;

[TestClass]
public sealed class BlobKeyTests
{
    [DataTestMethod]
    [DataRow("Creative Color Labs", "creative-color-labs")]
    [DataRow("  CQ  ", "cq")]
    [DataRow("a/b", "a-b")]
    [DataRow("a\\b", "a-b")]
    public void Slug_normalizes_the_segment(string segment, string expected)
    {
        Assert.AreEqual(expected, BlobKey.Slug(segment));
    }

    [TestMethod]
    public void Slug_rejects_a_blank_segment()
    {
        Assert.ThrowsException<ArgumentException>(() => BlobKey.Slug("   "));
    }

    [TestMethod]
    public void Combine_joins_without_repeated_slashes()
    {
        Assert.AreEqual("temporary/cq/app/x.png", BlobKey.Combine("temporary/", "/cq", "app//x.png"));
    }

    [TestMethod]
    public void Folder_normalizes_each_level()
    {
        Assert.AreEqual("tenant-x/app-y", BlobKey.Folder("/Tenant X/App Y/"));
    }

    [DataTestMethod]
    [DataRow("x.png", true)]
    [DataRow("/cq/app/x.png", true)]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("cq//x.png", false)]
    [DataRow("cq/", false)]
    [DataRow("cq/../x.png", false)]
    [DataRow("./x.png", false)]
    public void IsValid_accepts_only_file_keys(string? key, bool expected)
    {
        Assert.AreEqual(expected, BlobKey.IsValid(key));
    }

    [DataTestMethod]
    [DataRow("temporary/cq/x.png", "x.png")]
    [DataRow("x.png", "x.png")]
    public void FileName_is_the_last_segment(string key, string expected)
    {
        Assert.AreEqual(expected, BlobKey.FileName(key));
    }

    [DataTestMethod]
    [DataRow("image/png", "png")]
    [DataRow("image/svg+xml", "svg")]
    [DataRow("text/csv; charset=utf-8", "csv")]
    [DataRow("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx")]
    [DataRow("IMAGE/JPEG", "jpeg")]
    public void Extension_comes_from_the_content_type(string contentType, string expected)
    {
        Assert.AreEqual(expected, BlobKey.Extension(contentType));
    }

    [DataTestMethod]
    [DataRow("png")]
    [DataRow("image/")]
    [DataRow("/png")]
    public void Extension_rejects_an_invalid_content_type(string contentType)
    {
        Assert.ThrowsException<ArgumentException>(() => BlobKey.Extension(contentType));
    }
}
