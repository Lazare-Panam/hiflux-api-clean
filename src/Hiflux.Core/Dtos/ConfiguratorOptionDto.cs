namespace Hiflux.Core.Dtos;

public class ConfiguratorOptionDto
{
    public int FilterId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public List<ConfiguratorValueDto> Values { get; set; } = [];
}
