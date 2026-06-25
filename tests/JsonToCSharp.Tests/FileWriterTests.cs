using System.IO;
using System.Text;
using JsonToCSharp.App;
using JsonToCSharp.App.Models;

namespace JsonToCSharp.Tests;

public class FileWriterTests
{
    [Fact]
    public void WriteClasses_CreatesOneFilePerClass()
    {
        var classes = new List<ClassDefinition>
        {
            new ClassDefinition
            {
                ClassName = "Person",
                Properties =
                [
                    new PropertyDefinition { JsonName = "name", PropertyName = "Name", TypeName = "string" }
                ]
            },
            new ClassDefinition
            {
                ClassName = "Address",
                Properties =
                [
                    new PropertyDefinition { JsonName = "city", PropertyName = "City", TypeName = "string" }
                ]
            }
        };

        var tempDir = Path.Combine(Path.GetTempPath(), "jsontocsharp_test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            FileWriter.WriteClasses(classes, tempDir, "TestNamespace");

            var personFile = Path.Combine(tempDir, "Person.cs");
            var addressFile = Path.Combine(tempDir, "Address.cs");

            Assert.True(File.Exists(personFile));
            Assert.True(File.Exists(addressFile));

            var personContent = File.ReadAllText(personFile);
            var addressContent = File.ReadAllText(addressFile);

            Assert.Contains("public class Person", personContent);
            Assert.Contains("[JsonPropertyName(\"name\")]", personContent);
            Assert.Contains("public string Name { get; set; }", personContent);
            Assert.Contains("namespace TestNamespace", personContent);

            Assert.Contains("public class Address", addressContent);
            Assert.Contains("[JsonPropertyName(\"city\")]", addressContent);
            Assert.Contains("public string City { get; set; }", addressContent);
            Assert.Contains("namespace TestNamespace", addressContent);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
