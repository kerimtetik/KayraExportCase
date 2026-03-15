namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}