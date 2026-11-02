using Hiflux.Core.Dtos;

namespace Hiflux.Core.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct);
        Task<IReadOnlyList<CategoryFilterDto>?> GetCategoryFiltersAsync(int categoryId, CancellationToken ct);
        Task<IReadOnlyList<CatalogProductDto>?> GetCategoryProductsAsync(int categoryId, IReadOnlyDictionary<string, string>? filters, CancellationToken ct);
        Task<CatalogProductDto?> GetProductByPartNumberAsync(string partNumber, CancellationToken ct);
        Task<ConfiguratorStateDto?> GetConfiguratorStateAsync(int categoryId, IReadOnlyDictionary<string, string> selection, CancellationToken ct);
    }
}
