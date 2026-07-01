using WarehouseManagementSystem.Application.Features.StockMovements.Common;
using WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class StockMovementMapper
    {
        public static StockMovementDto ToDto(StockMovement movement)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                InventoryId = movement.InventoryId,
                ProductName = movement.Inventory.Product.Name,
                ProductSku = movement.Inventory.Product.SKU,
                LocationCode = movement.Inventory.Location.Code,
                WarehouseName = movement.Inventory.Location.Zone.Warehouse.Name,
                Type = movement.Type.ToString(),
                Quantity = movement.Quantity,
                Reference = movement.Reference,
                Notes = movement.Notes,
                CreatedBy = movement.CreatedBy,
                CreatedAt = movement.CreatedAt
            };
        }

        public static StockMovement ToEntity(
            CreateStockMovementCommand command, Guid createdBy)
        {
            return new StockMovement
            {
                InventoryId = command.InventoryId,
                Type = Enum.Parse<MovementType>(command.Type),
                Quantity = command.Quantity,
                Reference = command.Reference,
                Notes = command.Notes,
                CreatedBy = createdBy
            };
        }
    }
}