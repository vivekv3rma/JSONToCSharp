using JsonToCSharp.App;
using JsonToCSharp.App.Models;

namespace JsonToCSharp.Tests;

public class CSharpGeneratorTests
{
    [Fact]
    public void GenerateClass_WithSingleProperty_ProducesValidCSharp()
    {
        var classDef = new ClassDefinition
        {
            ClassName = "Person",
            Properties =
            [
                new PropertyDefinition
                {
                    JsonName = "name",
                    PropertyName = "Name",
                    TypeName = "string"
                }
            ]
        };

        var result = CSharpGenerator.GenerateClass(classDef);

        var expected =
            """
            public class Person
            {
                [JsonPropertyName("name")]
                public string Name { get; set; }
            }
            """;

        Assert.Equal(expected, result);
    }
}
