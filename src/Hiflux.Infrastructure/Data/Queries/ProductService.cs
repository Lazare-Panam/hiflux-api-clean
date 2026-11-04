using System.Linq.Expressions;
using Hiflux.Core.Dtos;
using Hiflux.Core.Entities.Products;
using Hiflux.Core.Interfaces;
using Hiflux.Infrastructure.Data.Configurator;
using Hiflux.Infrastructure.Data.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hiflux.Infrastructure.Data.Queries
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;
        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }
        private Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct) => _context.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);

        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
        {
            var categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
            return categories.Select(CatalogMappings.MapCategory).ToList();
        }
        public async Task<IReadOnlyList<CategoryFilterDto>?> GetCategoryFiltersAsync(int categoryId, CancellationToken ct = default)
        {
            if (!await CategoryExistsAsync(categoryId, ct))
            {
                _logger.LogInformation("Catgory {CategoryId} not found when loading filters", categoryId);
                return null;
            }

            /*SELECT cf.FilterId, f.Name, cf.SortOrder          
                FROM CategoryFilters AS cf
                INNER JOIN Filters AS f ON cf.FilterId = f.FilterId
                WHERE cf.CategoryId = @categoryId
                ORDER BY cf.SortOrder*/
            var filterDefs = await _context.CategoryFilters.AsNoTracking()
                .Where(cf => cf.CategoryId == categoryId)
                .OrderBy(cf => cf.SortOrder)
                .Select(cf => new { cf.FilterId, cf.Filter.Name, cf.SortOrder })
                .ToListAsync(ct);
            // the distinct values that actually occur for this series.  DISTINCT because
            // ProductFilterValues has one row per product -> the same value repeats per product
            var values = await _context.ProductFilterValues.AsNoTracking()
                .Where(cf => cf.CategoryId == categoryId)
                .Select(v => new { v.FilterId, v.Value })
                .Distinct()
                .ToListAsync();
            // index the flat value list by FilterId so each filter's lookup is O(1) (in-memory re-join) avoids cartesian explosion
            var valuesByFilter = values.ToLookup(v => v.FilterId, v => v.Value);

            return filterDefs.Select(fd => new CategoryFilterDto
            {
                FilterId = fd.FilterId,
                Name = fd.Name,
                SortOrder = fd.SortOrder,
                Values = valuesByFilter[fd.FilterId].OrderBy(v => v).ToList()
            }).ToList();
        }

        public async Task<IReadOnlyList<CatalogProductDto>?> GetProductsAsync(int categoryId, IReadOnlyDictionary<string, string>? filters, CancellationToken ct)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);
            if (!categoryExists)
            {
                _logger.LogInformation("Category {CategoryId} not found when loading products", categoryId);
                return null;
            }
            var query = _context.CatalogProducts.AsNoTracking().Where(p => p.CategoryId == categoryId);

            if (filters is not null)
            {
                foreach (var pair in filters)
                {
                    var filterName = pair.Key;
                    var filterValue = pair.Value;
                    query = query.Where(p => p.FilterValues.Any(v => v.Filter.Name == filterName && v.Value == filterValue));
                }
            }

            var products = await query
                .OrderBy(p => p.PartNumber)
                .Select(CatalogMappings.ToDto)
                .ToListAsync(ct);
            return products;

        }
        public async Task<CatalogProductDto?> GetProductByPartNumberAsync(string partNumber, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
            {
                _logger.LogInformation("GetProductByPartNumberAsync called with empty part number");
                return null;
            }

            return await _context.CatalogProducts.AsNoTracking()
                .Where(p => p.PartNumber == partNumber)
                .Select(CatalogMappings.ToDto)
                .FirstOrDefaultAsync(ct);

        }
        public async Task<ConfiguratorStateDto?> GetConfiguratorStateAsync(int categoryId, IReadOnlyDictionary<string, string> selection, CancellationToken ct)
        {
            if (!await CategoryExistsAsync(categoryId, ct))
            {
                _logger.LogInformation("Category {CategoryId} not found when configuring", categoryId);
                return null;
            }
            var filterDefs = await _context.CategoryFilters.AsNoTracking()
                .Where(cf => cf.CategoryId == categoryId)
                .OrderBy(cf => cf.SortOrder)
                .Select(cf => new FilterDef(cf.FilterId, cf.Filter.Name, cf.SortOrder))
                .ToListAsync(ct);

            var rows = await _context.ProductFilterValues.AsNoTracking()
                .Where(v => v.CategoryId == categoryId)
                .Select(v => new SpecRow(v.ProductId, v.Filter.Name, v.Value))
                .ToListAsync(ct);

            var state = ConfiguratorEngine.Compute(filterDefs, rows, selection);

            CatalogProductDto? resolved = null;
            if (state.ResolvedProductId is int pid)
            {
                resolved = await _context.CatalogProducts.AsNoTracking()
                    .Where(p => p.ProductId == pid)
                    .Select(CatalogMappings.ToDto)
                    .FirstOrDefaultAsync(ct);
            }

            var imageUrl = await _context.CatalogProducts.AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.ImageUrl != null)
                .OrderBy(p => p.PartNumber)
                .Select(p => p.ImageUrl)
                .FirstOrDefaultAsync(ct);

            return new ConfiguratorStateDto
            {
                Options = state.Options,
                MatchCount = state.MatchCount,
                TotalCount = state.TotalCount,
                Resolved = resolved,
                ImageUrl = imageUrl,
            };
        }
    }
}
