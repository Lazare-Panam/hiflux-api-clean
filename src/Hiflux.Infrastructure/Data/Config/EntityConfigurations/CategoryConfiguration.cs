using System.Text.Json;
using Hiflux.Core.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;        // ValueComparer<>
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hiflux.Infrastructure.Data.Config.EntityConfigurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        private static readonly JsonSerializerOptions CertJson = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   // write label/url
            PropertyNameCaseInsensitive = true                   // read label/url
        };

        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.CategoryId);
            builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(c => c.Name).IsUnique();
            builder.Property(c => c.CatalogUrl).HasMaxLength(2048);
            builder.Property(c => c.CadUrl).HasMaxLength(2048);
            builder.Property(c => c.Description).HasMaxLength(2000);
            builder.Property(c => c.BodyMaterial).HasMaxLength(100);
            builder.Property(c => c.SeatMaterial).HasMaxLength(100);
            builder.Property(c => c.Design).HasMaxLength(200);
            builder.Property(c => c.TemperatureRange).HasMaxLength(100);
            builder.Property(c => c.Approvals).HasMaxLength(200);
            builder.Property(c => c.DatasheetUrl).HasMaxLength(2048);
            // Store List<string> in one text column.  ["Fire-safe","Full bore"] <-> "Fire-safe\nFull bore"
            builder.Property(c => c.KeyFeatures)
                .HasConversion(
                    list => string.Join('\n', list),                 // to DB:   ["a","b"] -> "a\nb"
                    text => text.Replace("\r\n", "\n")               // from DB: "a\r\nb\n\n c " -> ["a","b","c"]
                                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToList())                           // cleans Windows \r\n, blank lines, stray spaces
                                                                     // List is mutable, so EF needs this to notice when the items change:
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (a, b) => a!.SequenceEqual(b!),              // equal = same items, same order
                        v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                        v => v.ToList()));

            // Store List<Certificate> as JSON in one column.  [{CE,/ce.pdf}] <-> "[{\"label\":\"CE\",\"url\":\"/ce.pdf\"}]"
            builder.Property(c => c.Certificates)
                .HasColumnName("CertificatesJson")
                .HasConversion(
                    list => JsonSerializer.Serialize(list, CertJson),    // to DB:   objects -> JSON text
                    json => JsonSerializer.Deserialize<List<Certificate>>(json, CertJson) ?? new List<Certificate>())  // from DB: JSON -> objects
                .Metadata.SetValueComparer(
                    new ValueComparer<List<Certificate>>(
                        (a, b) => a!.SequenceEqual(b!),
                        v => v.Aggregate(0, (h, c) => HashCode.Combine(h, c.GetHashCode())),
                        v => v.ToList()));
        }
    }
}