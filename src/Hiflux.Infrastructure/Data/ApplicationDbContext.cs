using Hiflux.Core.Entities.Products;
using Hiflux.Infrastructure.Data.Config.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Hiflux.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Filter> Filters => Set<Filter>();
        public DbSet<CategoryFilter> CategoryFilters => Set<CategoryFilter>();
        public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
        public DbSet<ProductFilterValue> ProductFilterValues => Set<ProductFilterValue>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new FilterConfiguration());
            builder.ApplyConfiguration(new CategoryFilterConfiguration());
            builder.ApplyConfiguration(new CatalogProductConfiguration());
            builder.ApplyConfiguration(new ProductFilterValueConfiguration());
        }
    }
}
