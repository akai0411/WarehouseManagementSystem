using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IProductRepository
    {
        Task AddAsync(Product product );
        Task<(List<Product> Items, int TotalCount)> GetAllAsync(GetProductsQuery query);
        Task<Product?> GetByIdAsync( Guid id );
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task<Product?> GetBySkuAsync(string sku); 
    }
}
