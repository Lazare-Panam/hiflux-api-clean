using Hiflux.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hiflux.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }


    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        _logger.LogInformation("GetCategories called");
        var categories = await _productService.GetCategoriesAsync(ct);
        return Ok(categories);
    }


    [HttpGet("categories/{categoryId:int}/filters")]
    public async Task<IActionResult> GetCategoryFilters(int categoryId, CancellationToken ct)
    {
        _logger.LogInformation("GetCategoryFilters called for {CategoryId}", categoryId);
        var filters = await _productService.GetCategoryFiltersAsync(categoryId, ct);

        if (filters is null)
        {
            _logger.LogWarning("Category {CategoryId} not found", categoryId);
            return NotFound($"No category found for ID: {categoryId}");
        }

        return Ok(filters);
    }


    [HttpGet("categories/{categoryId:int}/products")]
    public async Task<IActionResult> GetProducts(int categoryId, [FromQuery] Dictionary<string, string>? filters, CancellationToken ct)
    {
        _logger.LogInformation("GetProducts called for {CategoryId} with {FilterCount} filter(s)", categoryId, filters?.Count ?? 0);
        var products = await _productService.GetProductsAsync(categoryId, filters, ct);

        if (products is null)
        {
            _logger.LogWarning("Category {CategoryId} not found", categoryId);
            return NotFound($"No category found for ID: {categoryId}");
        }

        return Ok(products);
    }


    [HttpGet("categories/{categoryId:int}/configure")]
    public async Task<IActionResult> Configure(int categoryId, [FromQuery] Dictionary<string, string>? filters, CancellationToken ct)
    {
        _logger.LogInformation("Configure called for {CategoryId} with {FilterCount} selection(s)", categoryId, filters?.Count ?? 0);
        var state = await _productService.GetConfiguratorStateAsync(categoryId, filters ?? new Dictionary<string, string>(), ct);

        if (state is null)
        {
            _logger.LogWarning("Category {CategoryId} not found", categoryId);
            return NotFound($"No category found for ID: {categoryId}");
        }

        return Ok(state);
    }


    [HttpGet("products/{partNumber}")]
    public async Task<IActionResult> GetProduct(string partNumber, CancellationToken ct)
    {
        _logger.LogInformation("GetProduct called for {PartNumber}", partNumber);
        var product = await _productService.GetProductByPartNumberAsync(partNumber, ct);

        if (product is null)
        {
            _logger.LogWarning("Product {PartNumber} not found", partNumber);
            return NotFound($"No product found for part number: {partNumber}");
        }

        return Ok(product);
    }
}
