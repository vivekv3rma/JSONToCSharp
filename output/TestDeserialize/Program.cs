using System.Text.Json;
using MyApp.Models;

var json = """
{
  "id": 1,
  "name": "Widget Pro",
  "price": 29.99,
  "tags": ["sale", "popular"],
  "category": {
    "id": 5,
    "name": "Electronics",
    "parent": null
  },
  "supplier": {
    "id": 100,
    "company_name": "Acme Corp",
    "contact": {
      "email": "sales@acme.com",
      "phone": "+1-555-0100"
    }
  }
}
""";

var product = JsonSerializer.Deserialize<Root>(json);

Console.WriteLine($"Product: {product.Name}");
Console.WriteLine($"Price: {product.Price}");
Console.WriteLine($"Tags: {string.Join(", ", product.Tags)}");
Console.WriteLine($"Category: {product.Category.Name} (Parent: {product.Category.Parent ?? "none"})");
Console.WriteLine($"Supplier: {product.Supplier.CompanyName}");
Console.WriteLine($"Contact: {product.Supplier.Contact.Email}, {product.Supplier.Contact.Phone}");
