using System.Text;
using JsonToCSharp.App.Models;

namespace JsonToCSharp.App;

public static class CSharpGenerator
{
    public static string GenerateClass(ClassDefinition classDef)
    {
        var sb = new StringBuilder();
        sb.Append("public class ");
        sb.Append(classDef.ClassName);
        sb.Append('\n');
        sb.Append("{\n");

        foreach (var prop in classDef.Properties)
        {
            sb.Append("    [JsonPropertyName(\"");
            sb.Append(prop.JsonName);
            sb.Append("\")]\n");
            sb.Append("    public ");
            sb.Append(prop.TypeName);
            sb.Append(' ');
            sb.Append(prop.PropertyName);
            sb.Append(" { get; set; }\n");
        }

        sb.Append('}');
        return sb.ToString();
    }
}
