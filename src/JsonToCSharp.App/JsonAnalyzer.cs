using System.Text.Json;
using JsonToCSharp.App.Models;

namespace JsonToCSharp.App;

public static class JsonAnalyzer
{
    public static List<ClassDefinition> Analyze(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Root JSON element must be an object.");

        var classes = new List<ClassDefinition>();
        AnalyzeObject("Root", root, classes);
        return classes;
    }

    private static void AnalyzeObject(string className, JsonElement element, List<ClassDefinition> classes)
    {
        var properties = new List<PropertyDefinition>();

        foreach (var prop in element.EnumerateObject())
        {
            var jsonName = prop.Name;
            var propertyName = ToPascalCase(jsonName);
            var typeName = MapJsonType(prop.Value, prop.Name, classes);
            properties.Add(new PropertyDefinition
            {
                JsonName = jsonName,
                PropertyName = propertyName,
                TypeName = typeName
            });
        }

        classes.Add(new ClassDefinition
        {
            ClassName = className,
            Properties = properties
        });
    }

    private static string MapJsonType(JsonElement element, string propertyName, List<ClassDefinition> classes)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number when element.TryGetInt32(out _) => "int",
            JsonValueKind.Number => "double",
            JsonValueKind.True or JsonValueKind.False => "bool",
            JsonValueKind.Null => "string?", // Default to nullable string for null values
            JsonValueKind.Object => HandleNestedObject(element, propertyName, classes),
            JsonValueKind.Array => HandleArray(element, propertyName, classes),
            _ => "object"
        };
    }

    private static string HandleNestedObject(JsonElement element, string propertyName, List<ClassDefinition> classes)
    {
        var childClassName = ToPascalCase(propertyName);

        // Check if we already have a class with this name and matching structure
        var childProperties = new List<PropertyDefinition>();
        foreach (var prop in element.EnumerateObject())
        {
            childProperties.Add(new PropertyDefinition
            {
                JsonName = prop.Name,
                PropertyName = ToPascalCase(prop.Name),
                TypeName = MapJsonType(prop.Value, prop.Name, classes)
            });
        }

        // Check for duplicate by STRUCTURE (not name) - reuse existing class if shape matches
        foreach (var existing in classes)
        {
            if (PropertiesMatch(existing.Properties, childProperties))
            {
                return existing.ClassName;
            }
        }

        // No structural match - create new class
        classes.Add(new ClassDefinition
        {
            ClassName = childClassName,
            Properties = childProperties
        });

        return childClassName;
    }

    private static string HandleArray(JsonElement element, string propertyName, List<ClassDefinition> classes)
    {
        if (element.GetArrayLength() == 0)
            return "List<object>";

        var firstItem = element[0];
        var singularName = Singularize(propertyName);
        var elementType = MapJsonType(firstItem, singularName, classes);

        return $"List<{elementType}>";
    }

    internal static string Singularize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            return name;
        if (name.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 3) + "y";
        if (name.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 2);
        if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 1);
        return name;
    }

    private static bool PropertiesMatch(List<PropertyDefinition> a, List<PropertyDefinition> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].PropertyName != b[i].PropertyName ||
                a[i].TypeName != b[i].TypeName)
                return false;
        }
        return true;
    }

    internal static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Split on non-alphanumeric characters (underscores, hyphens, spaces)
        var parts = name.Split(['_', '-', ' ', '.'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return name;

        var result = string.Concat(parts.Select(p =>
            char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()));

        return result;
    }
}
