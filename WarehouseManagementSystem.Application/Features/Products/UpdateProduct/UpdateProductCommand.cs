

namespace WarehouseManagementSystem.Application.Features.Products.UpdateProduct
{
    public class UpdateProductCommand
    {
        public string Name { get; set; } 
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
    }
}
