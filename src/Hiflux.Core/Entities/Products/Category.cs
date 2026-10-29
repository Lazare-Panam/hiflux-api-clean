namespace Hiflux.Core.Entities.Products
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? CatalogUrl { get; set; }
        public string? CadUrl { get; set; }

        // Series-level marketing / datasheet content (all nullable, display-only).
        public string? Description { get; set; }
        public string? BodyMaterial { get; set; }
        public string? SeatMaterial { get; set; }
        public string? Design { get; set; }
        public string? TemperatureRange { get; set; }
        public string? Approvals { get; set; }
        public string? DatasheetUrl { get; set; }
        public List<string> KeyFeatures { get; set; } = [];
        public List<Certificate> Certificates { get; set; } = [];

        public List<CategoryFilter> CategoryFilters { get; set; } = [];
        public List<CatalogProduct> Products { get; set; } = [];
    }
}
