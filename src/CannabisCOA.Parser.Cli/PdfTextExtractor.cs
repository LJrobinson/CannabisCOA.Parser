using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

public static class PdfTextExtractor
{
    private const double LineTolerance = 2.5;

    public static string Extract(string filePath)
    {
        using var document = PdfDocument.Open(filePath);

        var text = new System.Text.StringBuilder();

        foreach (var page in document.GetPages())
        {
            var lines = ReconstructLines(page.GetWords());

            foreach (var line in lines)
            {
                text.AppendLine(line);
            }

            text.AppendLine();
        }

        return text.ToString();
    }

    private static IEnumerable<string> ReconstructLines(IEnumerable<Word> words)
    {
        var orderedWords = words
            .Where(word => !string.IsNullOrWhiteSpace(word.Text))
            .Select(word => new PositionedWord(
                word.Text,
                word.BoundingBox.Left,
                (word.BoundingBox.Bottom + word.BoundingBox.Top) / 2))
            .OrderByDescending(word => word.CenterY)
            .ThenBy(word => word.Left)
            .ToList();

        var lines = new List<List<PositionedWord>>();

        foreach (var word in orderedWords)
        {
            var line = lines.FirstOrDefault(existingLine =>
                Math.Abs(existingLine[0].CenterY - word.CenterY) <= LineTolerance);

            if (line == null)
            {
                lines.Add(new List<PositionedWord> { word });
                continue;
            }

            line.Add(word);
        }

        return lines
            .Select(line => string.Join(" ", line
                .OrderBy(word => word.Left)
                .Select(word => word.Text)))
            .Where(line => !string.IsNullOrWhiteSpace(line));
    }

    private sealed record PositionedWord(string Text, double Left, double CenterY);
}
