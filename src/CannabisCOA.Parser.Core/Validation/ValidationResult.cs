namespace CannabisCOA.Parser.Core.Validation;

public class ValidationResult
{
    public bool IsValid => Warnings.Count == 0;
    public List<ValidationWarning> Warnings { get; set; } = new();
}