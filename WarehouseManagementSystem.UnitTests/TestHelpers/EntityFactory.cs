using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.Domain.ValueObjects;
// The UnitTests project has an "Inventory" folder/namespace
// (WarehouseManagementSystem.UnitTests.Inventory), which shadows the bare
// "Inventory" type name from Domain.Entities in any file under this
// project. This alias sidesteps that collision.
using InventoryEntity = WarehouseManagementSystem.Domain.Entities.Inventory;

namespace WarehouseManagementSystem.UnitTests.TestHelpers
{
    /// <summary>
    /// Builds fully-wired domain entity graphs for handler tests.
    /// The Inventory/StockMovement handlers read nested navigation
    /// properties (e.g. inventory.Location.Zone.WarehouseId, or
    /// ...Zone.Warehouse.Address.Country) for role-scoping checks, so
    /// tests need a real Warehouse -> Zone -> Location -> Inventory chain
    /// instead of a bare entity with nulled-out navigations.
    /// </summary>
    internal static class EntityFactory
    {
        public static Warehouse CreateWarehouse(Guid? id = null, string country = "Spain")
        {
            return new Warehouse
            {
                Id = id ?? Guid.NewGuid(),
                Name = "Main Warehouse",
                Address = new Address("Street", "Example", "1", "28001", "Madrid", country)
            };
        }

        public static Zone CreateZone(Warehouse warehouse)
        {
            return new Zone
            {
                Id = Guid.NewGuid(),
                Name = "Zone A",
                Type = ZoneType.Dry,
                WarehouseId = warehouse.Id,
                Warehouse = warehouse
            };
        }

        public static Location CreateLocation(Zone zone)
        {
            return new Location
            {
                Id = Guid.NewGuid(),
                Row = "A",
                Shelf = 1,
                Bin = 1,
                Code = "A-01-01",
                ZoneId = zone.Id,
                Zone = zone
            };
        }

        public static Product CreateProduct(string name = "Widget", string sku = "SKU-001")
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                SKU = sku,
                Description = "Test product",
                Price = 9.99m
            };
        }

        /// <summary>
        /// Creates an Inventory record with its full navigation graph
        /// (Product, Location, Zone, Warehouse, Address) populated, so
        /// warehouse/country role-scoping logic in the handlers can run
        /// against it without null reference exceptions.
        /// </summary>
        public static InventoryEntity CreateInventory(
            int quantity = 10,
            int minimumStockLevel = 5,
            Guid? warehouseId = null,
            string country = "Spain")
        {
            var warehouse = CreateWarehouse(warehouseId, country);
            var zone = CreateZone(warehouse);
            var location = CreateLocation(zone);
            var product = CreateProduct();

            var inventory = new InventoryEntity
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                LocationId = location.Id,
                Location = location,
                Quantity = quantity,
                MinimumStockLevel = minimumStockLevel
            };

            location.Inventory = inventory;

            return inventory;
        }
    }
}
