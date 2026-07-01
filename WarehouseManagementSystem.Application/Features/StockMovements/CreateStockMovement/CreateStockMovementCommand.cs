namespace WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement
{
    public class CreateStockMovementCommand
    {
        public Guid InventoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}