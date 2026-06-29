namespace WarehouseManagementSystem.Application.Features.Inventory.CreateInventory
{
    public class CreateInventoryCommand
    {
        public Guid ProductId { get; set; }
        public Guid LocationId { get; set; }
        public int Quantity { get; set; }
        public int MinimumStockLevel { get; set; }
    }
}