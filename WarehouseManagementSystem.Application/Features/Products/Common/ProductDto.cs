namespace WarehouseManagementSystem.Application.Features.Products.Common
{
    /// <summary>
    /// Represents a product in the warehouse system.
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Unique identifier of the product.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Stock Keeping Unit (unique product code).
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Optional product description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Available quantity in stock.
        /// </summary>
        public int QuantityInStock { get; set; }
    }
}