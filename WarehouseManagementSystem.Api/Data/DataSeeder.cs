using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.Domain.ValueObjects;
using WarehouseManagementSystem.Infrastructure.Persistence;

namespace WarehouseManagementSystem.Api.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var passwordHasher = scope.ServiceProvider
                .GetRequiredService<IPasswordHasher>();

            await SeedWarehousesAsync(context);
            await SeedZonesAsync(context);
            await SeedLocationsAsync(context);
            await SeedProductsAsync(context);
            await SeedUsersAsync(context, passwordHasher);
        }

        private static async Task SeedWarehousesAsync(AppDbContext context)
        {
            if (await context.Warehouses.AnyAsync()) return;

            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    Id = SeedIds.MadridWarehouseId,
                    Name = "Madrid Central",
                    Description = "Main warehouse for Madrid operations",
                    Address = new Address(
                        "Calle",
                        "Gran Via",
                        "1",
                        "28013",
                        "Madrid",
                        "Spain")
                },
                new Warehouse
                {
                    Id = SeedIds.BarcelonaWarehouseId,
                    Name = "Barcelona Nord",
                    Description = "Main warehouse for Barcelona operations",
                    Address = new Address(
                        "Avenida",
                        "Diagonal",
                        "100",
                        "08018",
                        "Barcelona",
                        "Spain")
                }
            };

            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();
        }

        private static async Task SeedZonesAsync(AppDbContext context)
        {
            if (await context.Zones.AnyAsync()) return;

            var zones = new List<Zone>
            {
                // Madrid zones
                new Zone
                {
                    Id = SeedIds.MadridZoneElectronicsId,
                    Name = "Zone A - Electronics",
                    Description = "Dry storage for electronics",
                    Type = ZoneType.Dry,
                    WarehouseId = SeedIds.MadridWarehouseId
                },
                new Zone
                {
                    Id = SeedIds.MadridZoneRefrigeratedId,
                    Name = "Zone B - Refrigerated",
                    Description = "Refrigerated goods storage",
                    Type = ZoneType.Refrigerated,
                    WarehouseId = SeedIds.MadridWarehouseId
                },
                new Zone
                {
                    Id = SeedIds.MadridZoneHighValueId,
                    Name = "Zone C - High Value",
                    Description = "High value products storage",
                    Type = ZoneType.HighValue,
                    WarehouseId = SeedIds.MadridWarehouseId
                },
                // Barcelona zones
                new Zone
                {
                    Id = SeedIds.BarcelonaZoneElectronicsId,
                    Name = "Zone A - Electronics",
                    Description = "Dry storage for electronics",
                    Type = ZoneType.Dry,
                    WarehouseId = SeedIds.BarcelonaWarehouseId
                },
                new Zone
                {
                    Id = SeedIds.BarcelonaZoneRefrigeratedId,
                    Name = "Zone B - Refrigerated",
                    Description = "Refrigerated goods storage",
                    Type = ZoneType.Refrigerated,
                    WarehouseId = SeedIds.BarcelonaWarehouseId
                },
                new Zone
                {
                    Id = SeedIds.BarcelonaZoneHighValueId,
                    Name = "Zone C - High Value",
                    Description = "High value products storage",
                    Type = ZoneType.HighValue,
                    WarehouseId = SeedIds.BarcelonaWarehouseId
                }
            };

            await context.Zones.AddRangeAsync(zones);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLocationsAsync(AppDbContext context)
        {
            if (await context.Locations.AnyAsync()) return;

            var locations = new List<Location>();

            var zoneIds = new[]
            {
                SeedIds.MadridZoneElectronicsId,
                SeedIds.MadridZoneRefrigeratedId,
                SeedIds.MadridZoneHighValueId,
                SeedIds.BarcelonaZoneElectronicsId,
                SeedIds.BarcelonaZoneRefrigeratedId,
                SeedIds.BarcelonaZoneHighValueId
            };

            foreach (var zoneId in zoneIds)
            {
                var rows = new[] { "A", "B" };
                foreach (var row in rows)
                {
                    for (int shelf = 1; shelf <= 2; shelf++)
                    {
                        for (int bin = 1; bin <= 3; bin++)
                        {
                            locations.Add(new Location
                            {
                                Row = row,
                                Shelf = shelf,
                                Bin = bin,
                                Code = $"{row}-{shelf:D2}-{bin:D2}",
                                ZoneId = zoneId
                            });
                        }
                    }
                }
            }

            await context.Locations.AddRangeAsync(locations);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            var products = new List<Product>
            {
                new Product
                {
                    Id = SeedIds.LaptopProductId,
                    Name = "Laptop Pro 15",
                    SKU = "LP-001",
                    Description = "High performance laptop",
                    Price = 1200
                },
                new Product
                {
                    Name = "Wireless Mouse",
                    SKU = "MS-001",
                    Description = "Ergonomic wireless mouse",
                    Price = 25
                },
                new Product
                {
                    Name = "Mechanical Keyboard",
                    SKU = "KB-001",
                    Description = "Mechanical gaming keyboard",
                    Price = 85
                },
                new Product
                {
                    Name = "USB-C Hub",
                    SKU = "HB-001",
                    Description = "7 in 1 USB-C hub",
                    Price = 45
                },
                new Product
                {
                    Name = "Monitor 27\"",
                    SKU = "MN-001",
                    Description = "4K 27 inch monitor",
                    Price = 350
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(
            AppDbContext context,
            IPasswordHasher passwordHasher)
        {
            if (await context.Users.AnyAsync()) return;

            var tempUser = new User();

            var users = new List<User>
            {
                new User
                {
                    Email = "admin@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "Admin"
                },
                new User
                {
                    Email = "regional.europe@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "RegionalManager",
                    Country = "Spain"
                },
                new User
                {
                    Email = "manager.madrid@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "WarehouseManager",
                    WarehouseId = SeedIds.MadridWarehouseId
                },
                new User
                {
                    Email = "operator.madrid@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "Operator",
                    WarehouseId = SeedIds.MadridWarehouseId
                },
                new User
                {
                    Email = "manager.barcelona@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "WarehouseManager",
                    WarehouseId = SeedIds.BarcelonaWarehouseId
                },
                new User
                {
                    Email = "operator.barcelona@warehouse.com",
                    PasswordHash = passwordHasher.HashPassword(new User(), "Admin123!"),
                    Role = "Operator",
                    WarehouseId = SeedIds.BarcelonaWarehouseId
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}