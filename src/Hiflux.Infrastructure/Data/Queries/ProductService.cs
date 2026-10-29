using Hiflux.Core.Dtos;
using Hiflux.Core.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hiflux.Infrastructure.Data.Queries
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;
        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }
        private Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct) => _context.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);
        private CategoryDto MapCategory(Category c) => new()
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            CatalogUrl = c.CatalogUrl,
            CadUrl = c.CadUrl,
            Description = c.Description,
            BodyMaterial = c.BodyMaterial,
            SeatMaterial = c.SeatMaterial,
            Design = c.Design,
            TemperatureRange = c.TemperatureRange,
            Approvals = c.Approvals,
            DatasheetUrl = c.DatasheetUrl,
            KeyFeatures = c.KeyFeatures,
            Certificates = c.Certificates.Select(x => new CertificateDto { Label = x.Label, Url = x.Url }).ToList(),
        };
        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
        {
            var categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
            return categories.Select(MapCategory).ToList();
        }
        public async Task<IReadOnlyList<CategoryFilterDto>?> GetCategoryFiltersAsync(int categoryId, CancellationToken ct = default)
        {
            if(!await CategoryExistsAsync(categoryId,ct))
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
                .Where(cf=>cf.CategoryId == categoryId)
                .OrderBy(cf=>cf.SortOrder)
                .Select(cf=> new {cf.FilterId, cf.Filter.Name, cf.SortOrder})
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
    }
}
