

namespace WarehouseManagementSystem.Application.Features.Products.UpdateProduct
{
    public class UpdateProductCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
