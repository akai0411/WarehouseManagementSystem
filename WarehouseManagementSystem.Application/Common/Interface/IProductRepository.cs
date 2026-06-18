using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IProductRepository
    {
        Task AddAsync(Product product );
        Task<List<Product>> GetAllPagedAsync(int page, int pageSize);
        Task<Product?> GetByIdAsync( Guid id );
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
    }
}
