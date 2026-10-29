namespace Hiflux.Core.Entities.Products
{
    public class Filter
    {
        public int FilterId { get; set; }
        public string Name { get; set; }
        public List<CategoryFilter> CategoryFilters { get; set; } = [];
    }
}
