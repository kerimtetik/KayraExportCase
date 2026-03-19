namespace Product.Application.Features.Products.Queries.GetProductsCursor;

public class GetProductsCursorQuery
{
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
}
