namespace Hiflux.Core.Dtos;

public class CatalogProductDto
{
    public int ProductId { get; set; }
    public string PartNumber { get; set; }
    public string ProductStatus { get; set; }
    public string? FamilyDescription { get; set; }
    public string? DetailDescription { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public List<ProductSpecDto> Specs { get; set; } = [];
}
