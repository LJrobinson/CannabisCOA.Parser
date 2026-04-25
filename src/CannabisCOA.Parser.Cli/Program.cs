using System.Text.Json;
using CannabisCOA.Parser.Core.Analysis;

if (args.Length == 0)
{
    Console.WriteLine("CannabisCOA.Parser CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  cannabis-coa \"THC: 0.42% THCA: 24.88%\"");
    Console.WriteLine("  cannabis-coa --file fixtures/digipath-flower.txt");
    return;
}

string inputText;

if (args.Length >= 2 && args[0] == "--file")
{
    var filePath = args[1];

    if (!File.Exists(filePath))
    {
        Console.Error.WriteLine($"File not found: {filePath}");
        Environment.Exit(1);
        return;
    }

    inputText = File.ReadAllText(filePath);
}
else
{
    inputText = string.Join(" ", args);
}

var result = CoaAnalyzer.Analyze(inputText);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));