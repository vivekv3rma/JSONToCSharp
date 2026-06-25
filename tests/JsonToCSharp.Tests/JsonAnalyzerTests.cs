using JsonToCSharp.App;
using JsonToCSharp.App.Models;

namespace JsonToCSharp.Tests;

public class JsonAnalyzerTests
{
    [Fact]
    public void Analyze_FlatJson_ReturnsClassWithCorrectProperties()
    {
        var json = """
            {
                "name": "John",
                "age": 30,
                "is_active": true,
                "score": 95.5
            }
            """;

        var result = JsonAnalyzer.Analyze(json);

        Assert.Single(result);
        Assert.Equal("Root", result[0].ClassName);
        Assert.Equal(4, result[0].Properties.Count);

        var name = result[0].Properties[0];
        Assert.Equal("name", name.JsonName);
        Assert.Equal("Name", name.PropertyName);
        Assert.Equal("string", name.TypeName);

        var age = result[0].Properties[1];
        Assert.Equal("age", age.JsonName);
        Assert.Equal("Age", age.PropertyName);
        Assert.Equal("int", age.TypeName);

        var isActive = result[0].Properties[2];
        Assert.Equal("is_active", isActive.JsonName);
        Assert.Equal("IsActive", isActive.PropertyName);
        Assert.Equal("bool", isActive.TypeName);

        var score = result[0].Properties[3];
        Assert.Equal("score", score.JsonName);
        Assert.Equal("Score", score.PropertyName);
        Assert.Equal("double", score.TypeName);
    }

    [Fact]
    public void Analyze_NestedJson_ReturnsMultipleClasses()
    {
        var json = """
            {
                "id": 1,
                "address": {
                    "city": "Seattle",
                    "zip": "98101"
                }
            }
            """;

        var result = JsonAnalyzer.Analyze(json);

        // We expect two classes: "Address" and "Root"
        Assert.Equal(2, result.Count);

        // One of them is Root
        var rootClass = result.FirstOrDefault(c => c.ClassName == "Root");
        Assert.NotNull(rootClass);
        Assert.Equal(2, rootClass.Properties.Count);

        var addressProp = rootClass.Properties.FirstOrDefault(p => p.PropertyName == "Address");
        Assert.NotNull(addressProp);
        Assert.Equal("Address", addressProp.TypeName);

        // The other is Address
        var addressClass = result.FirstOrDefault(c => c.ClassName == "Address");
        Assert.NotNull(addressClass);
        Assert.Equal(2, addressClass.Properties.Count);

        var cityProp = addressClass.Properties.FirstOrDefault(p => p.PropertyName == "City");
        Assert.NotNull(cityProp);
        Assert.Equal("string", cityProp.TypeName);
    }

    [Fact]
    public void Analyze_PrimitiveArray_ReturnsListProperty()
    {
        var json = """
            {
                "tags": ["new", "sale"]
            }
            """;

        var result = JsonAnalyzer.Analyze(json);
        Assert.Single(result);
        var root = result[0];
        var tagsProp = root.Properties.FirstOrDefault(p => p.PropertyName == "Tags");
        Assert.NotNull(tagsProp);
        Assert.Equal("List<string>", tagsProp.TypeName);
    }

    [Fact]
    public void Analyze_ObjectArray_ReturnsListPropertyAndNewClass()
    {
        var json = """
            {
                "items": [
                    { "name": "item1" },
                    { "name": "item2" }
                ]
            }
            """;

        var result = JsonAnalyzer.Analyze(json);
        // We expect Root and Item class definitions
        Assert.Equal(2, result.Count);

        var root = result.FirstOrDefault(c => c.ClassName == "Root");
        Assert.NotNull(root);
        var itemsProp = root.Properties.FirstOrDefault(p => p.PropertyName == "Items");
        Assert.NotNull(itemsProp);
        Assert.Equal("List<Item>", itemsProp.TypeName);

        var itemClass = result.FirstOrDefault(c => c.ClassName == "Item");
        Assert.NotNull(itemClass);
        var nameProp = itemClass.Properties.FirstOrDefault(p => p.PropertyName == "Name");
        Assert.NotNull(nameProp);
        Assert.Equal("string", nameProp.TypeName);
    }

    [Fact]
    public void Analyze_DuplicateShapes_ReusesClassDefinition()
    {
        var json = """
            {
                "billing_address": {
                    "street": "123 Main St",
                    "city": "Seattle"
                },
                "shipping_address": {
                    "street": "456 Pine St",
                    "city": "Portland"
                }
            }
            """;

        var result = JsonAnalyzer.Analyze(json);

        // We should have 2 classes: Root and BillingAddress (which is reused for shipping_address)
        // OR Address (if we name it generically). Reusing BillingAddress is fine, but we definitely
        // want to verify that shipping_address's TypeName points to the same class as billing_address.
        Assert.Equal(2, result.Count);

        var root = result.FirstOrDefault(c => c.ClassName == "Root");
        Assert.NotNull(root);

        var billingProp = root.Properties.FirstOrDefault(p => p.PropertyName == "BillingAddress");
        var shippingProp = root.Properties.FirstOrDefault(p => p.PropertyName == "ShippingAddress");

        Assert.NotNull(billingProp);
        Assert.NotNull(shippingProp);
        
        // They should share the exact same TypeName (class name)
        Assert.Equal(billingProp.TypeName, shippingProp.TypeName);
    }

    [Fact]
    public void Analyze_NullProperty_ReturnsNullableType()
    {
        var json = """
            {
                "name": "John",
                "middle_name": null,
                "age": 30
            }
            """;

        var result = JsonAnalyzer.Analyze(json);
        Assert.Single(result);
        var root = result[0];

        var middleNameProp = root.Properties.FirstOrDefault(p => p.PropertyName == "MiddleName");
        Assert.NotNull(middleNameProp);
        // Should be nullable string since the value is null
        Assert.Equal("string?", middleNameProp.TypeName);
    }
}
