using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IStockMovementRepository
    {
        Task AddAsync(StockMovement movement);
        Task<(List<StockMovement> Items, int TotalCount)> GetAllAsync(
            StockMovementFilter filter);
        Task<StockMovement?> GetByIdAsync(Guid id);
    }
}