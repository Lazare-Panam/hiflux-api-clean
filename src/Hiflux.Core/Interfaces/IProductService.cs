using Hiflux.Core.Dtos;

namespace Hiflux.Core.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct);
        Task<IReadOnlyList<CategoryFilterDto>>? GetCategoryFiltersAsync(int categoryId, CancellationToken ct);
        Task<IReadOnlyList<CatalogProductDto?>> GetCategoryProductsAsync(int categoryId, CancellationToken ct);
        Task<CatalogProductDto?> GetProductByIdAsync(int productId, CancellationToken ct);
        Task<ConfiguratorStateDto?> GetConfiguratorStateAsync(int productId, CancellationToken ct);
    }
}
