namespace Hiflux.Core.Dtos;

public class CategoryFilterDto
{
    public int FilterId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public List<string> Values { get; set; } = [];
}
