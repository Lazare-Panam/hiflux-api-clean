namespace Hiflux.Core.Dtos;

public class ConfiguratorStateDto
{
    public List<ConfiguratorOptionDto> Options { get; set; } = [];
    public int MatchCount { get; set; }
    public int TotalCount { get; set; }
    public CatalogProductDto? Resolved { get; set; }
    public string? ImageUrl { get; set; }
}
