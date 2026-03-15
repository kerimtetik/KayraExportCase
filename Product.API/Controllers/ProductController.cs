using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Commands.UpdateProduct;
using Product.Application.Features.Products.Queries.GetProducts;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("product service is running");
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        CreateProductRequestDto request,
        [FromServices] CreateProductCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> Update(
        Guid id,
        UpdateProductRequestDto request,
        [FromServices] UpdateProductCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        var result = await handler.Handle(command, cancellationToken);

        if (result is null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAll(
        [FromServices] GetProductsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }
}