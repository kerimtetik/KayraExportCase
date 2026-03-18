namespace Product.Domain.Events;

public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
