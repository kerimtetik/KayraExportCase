using System.Text;
using System.Text.Json;

namespace Product.Application.Features.Products.Queries.GetProductsCursor;

public static class ProductCursorHelper
{
    public static string Encode(ProductCursorModel cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static ProductCursorModel Decode(string cursor)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var model = JsonSerializer.Deserialize<ProductCursorModel>(json);

            if (model is null || model.Id == Guid.Empty)
            {
                throw new ArgumentException("Cursor geçersiz.");
            }

            return model;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException)
        {
            throw new ArgumentException("Cursor geçersiz.", ex);
        }
    }
}
