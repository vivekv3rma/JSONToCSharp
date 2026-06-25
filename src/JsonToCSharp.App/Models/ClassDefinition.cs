namespace JsonToCSharp.App.Models;

public class ClassDefinition
{
    public required string ClassName { get; set; }
    public required List<PropertyDefinition> Properties { get; set; }
}
