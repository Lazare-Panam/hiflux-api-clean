using Hiflux.Api.Controllers;
using Hiflux.Core.Dtos;
using Hiflux.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hiflux.UnitTests;

public class ProductControllerTests
{
    // Hand-rolled stub (no mocking library needed) — each property is the canned
    // return value for the matching service method.
    private sealed class FakeProductService : IProductService
    {
        public IReadOnlyList<CategoryDto> Categories = [];
        public IReadOnlyList<CategoryFilterDto>? Filters;
        public IReadOnlyList<CatalogProductDto>? Products;
        public CatalogProductDto? Product;
        public ConfiguratorStateDto? State;

        public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct) => Task.FromResult(Categories);
        public Task<IReadOnlyList<CategoryFilterDto>?> GetCategoryFiltersAsync(int categoryId, CancellationToken ct) => Task.FromResult(Filters);
        public Task<IReadOnlyList<CatalogProductDto>?> GetProductsAsync(int categoryId, IReadOnlyDictionary<string, string>? filters, CancellationToken ct) => Task.FromResult(Products);
        public Task<CatalogProductDto?> GetProductByPartNumberAsync(string partNumber, CancellationToken ct) => Task.FromResult(Product);
        public Task<ConfiguratorStateDto?> GetConfiguratorStateAsync(int categoryId, IReadOnlyDictionary<string, string> selection, CancellationToken ct) => Task.FromResult(State);
    }

    private static ProductController CreateController(FakeProductService fake) =>
        new(fake, NullLogger<ProductController>.Instance);

    [Fact]
    public async Task GetCategories_returns_Ok()
    {
        var fake = new FakeProductService { Categories = [new CategoryDto { CategoryId = 10, Name = "Ball Valves" }] };

        var result = await CreateController(fake).GetCategories(default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(fake.Categories, ok.Value);
    }

    [Fact]
    public async Task GetCategoryFilters_returns_Ok_when_found()
    {
        var fake = new FakeProductService { Filters = [] };

        var result = await CreateController(fake).GetCategoryFilters(10, default);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCategoryFilters_returns_NotFound_when_null()
    {
        var fake = new FakeProductService { Filters = null };

        var result = await CreateController(fake).GetCategoryFilters(999, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProducts_returns_Ok_when_found()
    {
        var fake = new FakeProductService { Products = [] };

        var result = await CreateController(fake).GetProducts(10, null, default);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetProducts_returns_NotFound_when_null()
    {
        var fake = new FakeProductService { Products = null };

        var result = await CreateController(fake).GetProducts(999, null, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Configure_returns_Ok_when_found()
    {
        var fake = new FakeProductService { State = new ConfiguratorStateDto() };

        var result = await CreateController(fake).Configure(10, null, default);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Configure_returns_NotFound_when_null()
    {
        var fake = new FakeProductService { State = null };

        var result = await CreateController(fake).Configure(999, null, default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProduct_returns_Ok_when_found()
    {
        var fake = new FakeProductService { Product = new CatalogProductDto { PartNumber = "BV-100", ProductStatus = "Active" } };

        var result = await CreateController(fake).GetProduct("BV-100", default);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetProduct_returns_NotFound_when_null()
    {
        var fake = new FakeProductService { Product = null };

        var result = await CreateController(fake).GetProduct("NOPE", default);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
