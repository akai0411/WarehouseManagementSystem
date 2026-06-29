using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IInventoryRepository
    {
        Task AddAsync(Inventory inventory);
        Task<(List<Inventory> Items, int TotalCount)> GetAllAsync(InventoryFilter filter);
        Task<Inventory?> GetByIdAsync(Guid id);
        Task<Inventory?> GetByLocationIdAsync(Guid locationId);
        Task DeleteAsync(Inventory inventory);
    }
}