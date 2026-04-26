using UglyToad.PdfPig;

public static class PdfTextExtractor
{
    public static string Extract(string filePath)
    {
        using var document = PdfDocument.Open(filePath);

        var text = new System.Text.StringBuilder();

        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString();
    }
}