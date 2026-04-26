using System.IO;
using Xunit;

namespace CannabisCOA.Parser.Core.Tests;

public class DigipathTerpeneFixtureTests
{
    private static string LoadFixture(string name)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Labs",
            name);

        return File.ReadAllText(path);
    }

    [Fact]
    public void Parses_Digipath_TotalTerpenes_From_Table_Total_Row()
    {
        var text = LoadFixture("Digipath_Flower.txt");

        var result = CoaParser.Parse(text);

        Assert.Equal(1.7871m, result.Terpenes.TotalTerpenes);
    }
}