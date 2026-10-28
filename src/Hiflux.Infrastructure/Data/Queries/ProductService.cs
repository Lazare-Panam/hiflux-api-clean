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
    }
}
