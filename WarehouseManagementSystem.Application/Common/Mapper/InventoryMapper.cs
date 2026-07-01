using WarehouseManagementSystem.Application.Features.Inventory.Common;
using WarehouseManagementSystem.Application.Features.Inventory.CreateInventory;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class InventoryMapper
    {
        public static InventoryDto ToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product.Name,
                ProductSku = inventory.Product.SKU,
                LocationId = inventory.LocationId,
                LocationCode = inventory.Location.Code,
                WarehouseId = inventory.Location.Zone.WarehouseId,
                WarehouseName = inventory.Location.Zone.Warehouse.Name,
                Quantity = inventory.Quantity,
                MinimumStockLevel = inventory.MinimumStockLevel,
                IsLowStock = inventory.Quantity <= inventory.MinimumStockLevel,
                CreatedAt = inventory.CreatedAt
            };
        }

        public static Inventory ToEntity(CreateInventoryCommand command)
        {
            return new Inventory
            {
                ProductId = command.ProductId,
                LocationId = command.LocationId,
                Quantity = command.Quantity,
                MinimumStockLevel = command.MinimumStockLevel
            };
        }
    }
}