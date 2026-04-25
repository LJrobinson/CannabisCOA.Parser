namespace CannabisCOA.Parser.Core.Validation;

public class ValidationWarning
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string Severity { get; set; } = "warning";
}