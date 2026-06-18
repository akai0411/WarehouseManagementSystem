using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure the Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.QuantityInStock).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.DeletedAt).IsRequired(false);
            });
        }

    }
}
