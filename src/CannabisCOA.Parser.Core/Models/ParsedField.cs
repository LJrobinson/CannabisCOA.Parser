namespace CannabisCOA.Parser.Core.Models;

public class ParsedField<T>
{
    public string FieldName { get; set; } = "";
    public T? Value { get; set; }
    public string SourceText { get; set; } = "";
    public decimal Confidence { get; set; }
}