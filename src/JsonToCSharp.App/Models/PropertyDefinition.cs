namespace JsonToCSharp.App.Models;

public class PropertyDefinition
{
    public required string JsonName { get; set; }
    public required string PropertyName { get; set; }
    public required string TypeName { get; set; }
}
