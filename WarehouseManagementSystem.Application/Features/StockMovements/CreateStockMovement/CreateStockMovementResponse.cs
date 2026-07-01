namespace WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement
{
    public class CreateStockMovementResponse
    {
        public Guid MovementId { get; set; }
        public int NewQuantity { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}