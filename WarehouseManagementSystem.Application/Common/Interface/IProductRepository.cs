using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IProductRepository
    {
        Task AddAsync(Product product );
        Task<(List<Product> Items, int TotalCount)> GetAllAsync(GetProductsQuery query);
        Task<Product?> GetByIdAsync( Guid id );
        /// <summary>
        /// Persists changes to an existing product.
        /// </summary>
        /// <param name="product">The product with its new field values applied.</param>
        /// <param name="originalRowVersion">
        /// The RowVersion the caller originally read the product with (e.g. from a
        /// prior GET). Used as the EF Core concurrency token's original value, so
        /// the update fails with DbUpdateConcurrencyException if someone else
        /// changed the row since the caller last read it — not just since this
        /// method's own GetByIdAsync call a moment ago.
        /// </param>
        Task UpdateAsync(Product product, byte[] originalRowVersion);
        Task DeleteAsync(Product product);
        Task<Product?> GetBySkuAsync(string sku); 
    }
}
