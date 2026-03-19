namespace Product.Application.DTOs;

public class ProductCursorPageResponseDto
{
    public List<ProductResponseDto> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasNext { get; set; }
    public int Limit { get; set; }
}
