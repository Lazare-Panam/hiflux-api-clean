using Hiflux.Core.Entities.Products;
using Hiflux.Infrastructure.Data;
using Hiflux.Infrastructure.Data.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hiflux.UnitTests;

public class ProductServiceTests
{
    private static (ApplicationDbContext Context, SqliteConnection Connection) CreateSeededContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        context.Categories.Add(new Category { CategoryId = 10, Name = "Ball Valves" });
        context.Filters.AddRange(
            new Filter { FilterId = 1, Name = "Size" },
            new Filter { FilterId = 2, Name = "End Connection" });
        context.CategoryFilters.AddRange(
            new CategoryFilter { CategoryId = 10, FilterId = 1, SortOrder = 0 },
            new CategoryFilter { CategoryId = 10, FilterId = 2, SortOrder = 1 });
        context.CatalogProducts.AddRange(
            new CatalogProduct { ProductId = 100, CategoryId = 10, PartNumber = "BV-100", ProductStatus = "Active" },
            new CatalogProduct { ProductId = 101, CategoryId = 10, PartNumber = "BV-101", ProductStatus = "Active" });
        context.ProductFilterValues.AddRange(
            new ProductFilterValue { Id = 1, ProductId = 100, CategoryId = 10, FilterId = 1, Value = "1/2\"" },
            new ProductFilterValue { Id = 2, ProductId = 100, CategoryId = 10, FilterId = 2, Value = "NPT" },
            new ProductFilterValue { Id = 3, ProductId = 101, CategoryId = 10, FilterId = 1, Value = "3/4\"" },
            new ProductFilterValue { Id = 4, ProductId = 101, CategoryId = 10, FilterId = 2, Value = "NPT" });
        context.SaveChanges();

        return (context, connection);
    }

    private static ProductService CreateService(ApplicationDbContext context) =>
        new(context, NullLogger<ProductService>.Instance);

    [Fact]
    public async Task GetCategoryFiltersAsync_returns_filters_in_sort_order_with_distinct_values()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var result = await service.GetCategoryFiltersAsync(10, default);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);

            // ordered by SortOrder
            Assert.Equal("Size", result[0].Name);
            Assert.Equal(["1/2\"", "3/4\""], result[0].Values);


            Assert.Equal("End Connection", result[1].Name);
            Assert.Equal(["NPT"], result[1].Values);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetCategoryFiltersAsync_returns_null_when_category_does_not_exist()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var result = await service.GetCategoryFiltersAsync(999, default);

            Assert.Null(result);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetProductsAsync_returns_all_products_with_specs_when_no_filters()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var result = await service.GetProductsAsync(10, null, default);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);

            // ordered by PartNumber
            Assert.Equal("BV-100", result[0].PartNumber);
            Assert.Equal("BV-101", result[1].PartNumber);

            // specs are flattened from the product's filter values
            Assert.Contains(result[0].Specs, s => s.Filter == "Size" && s.Value == "1/2\"");
            Assert.Contains(result[0].Specs, s => s.Filter == "End Connection" && s.Value == "NPT");
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetProductsAsync_filters_by_a_single_facet()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var filters = new Dictionary<string, string> { ["Size"] = "1/2\"" };
            var result = await service.GetProductsAsync(10, filters, default);

            Assert.NotNull(result);
            var product = Assert.Single(result!);
            Assert.Equal("BV-100", product.PartNumber);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetProductsAsync_ANDs_multiple_facets()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var filters = new Dictionary<string, string>
            {
                ["Size"] = "3/4\"",
                ["End Connection"] = "NPT"
            };
            var result = await service.GetProductsAsync(10, filters, default);

            Assert.NotNull(result);
            var product = Assert.Single(result!);
            Assert.Equal("BV-101", product.PartNumber);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetProductsAsync_returns_empty_when_no_product_matches_the_filter()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var filters = new Dictionary<string, string> { ["Size"] = "99\"" };
            var result = await service.GetProductsAsync(10, filters, default);

            Assert.NotNull(result);
            Assert.Empty(result!);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task GetProductsAsync_returns_null_when_category_does_not_exist()
    {
        var (context, connection) = CreateSeededContext();
        try
        {
            var service = CreateService(context);

            var result = await service.GetProductsAsync(999, null, default);

            Assert.Null(result);
        }
        finally
        {
            context.Dispose();
            connection.Dispose();
        }
    }
}
