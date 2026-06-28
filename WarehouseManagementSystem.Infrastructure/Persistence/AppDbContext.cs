using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Domain.Common;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products => Set<Product>();

        public DbSet<User> Users => Set<User>();

        //Warehouse Entities
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Zone> Zones => Set<Zone>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure the Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SKU)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(e => e.DeletedAt)
                    .IsRequired(false);

                entity.Property(e => e.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

            });
            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Country)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
            // Configure the Warehouse entity
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.OwnsOne(e => e.Address, a =>
                {
                    a.Property(x => x.StreetType)
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnName("Address_StreetType");

                    a.Property(x => x.StreetName)
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnName("Address_StreetName");

                    a.Property(x => x.Number)
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnName("Address_Number");

                    a.Property(x => x.PostalCode)
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnName("Address_PostalCode");

                    a.Property(x => x.City)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnName("Address_City");

                    a.Property(x => x.Country)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnName("Address_Country");
                });

                entity.HasMany(e => e.Zones)
                    .WithOne(z => z.Warehouse)
                    .HasForeignKey(z => z.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Configure the Zone entity
            modelBuilder.Entity<Zone>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>(); // store as string not int in DB

                entity.HasMany(e => e.Locations)
                    .WithOne(l => l.Zone)
                    .HasForeignKey(l => l.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Configure the Location entity
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Row)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(e => new { e.ZoneId, e.Code })
                    .IsUnique(); // no duplicate codes within the same zone

                entity.HasOne(e => e.Inventory)
                    .WithOne(i => i.Location)
                    .HasForeignKey<Inventory>(i => i.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            //  Configure the Inventory entity
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.LocationId)
                    .IsUnique(); // one product per location enforced at DB level

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.MinimumStockLevel)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.Id == Guid.Empty)
                            entry.Entity.Id = Guid.NewGuid();
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                }

                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                        auditableEntity.IsDeleted = false;

                    if (entry.State == EntityState.Modified)
                        auditableEntity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
