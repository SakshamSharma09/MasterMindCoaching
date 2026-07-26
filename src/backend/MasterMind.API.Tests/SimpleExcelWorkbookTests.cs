using System.IO.Compression;
using MasterMind.API.Utilities;
using Xunit;

namespace MasterMind.API.Tests;

public class SimpleExcelWorkbookTests
{
    [Fact]
    public void Create_ProducesReadableXlsxWithStudentSheet()
    {
        var bytes = SimpleExcelWorkbook.Create(
            new[] { "Student ID", "Student Name" },
            new[] { (IReadOnlyList<string?>)new string?[] { "1", "Asha & Co" } });

        Assert.Equal((byte)'P', bytes[0]);
        Assert.Equal((byte)'K', bytes[1]);

        using var stream = new MemoryStream(bytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var sheet = archive.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(sheet);
        using var reader = new StreamReader(sheet!.Open());
        var content = reader.ReadToEnd();
        Assert.Contains("Asha &amp; Co", content);
        Assert.Contains("Student Name", content);
    }
}
