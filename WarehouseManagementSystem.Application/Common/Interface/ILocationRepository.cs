using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface ILocationRepository
    {
        Task AddAsync(Location location);
        Task<(List<Location> Items, int TotalCount)> GetAllAsync(Guid zoneId, LocationFilter filter);
        Task<Location?> GetByIdAsync(Guid id);
        Task UpdateAsync(Location location);
        Task DeleteAsync(Location location);
        Task<Location?> GetByCodeAndZoneAsync(string code, Guid zoneId);
        Task<bool> HasInventoryAsync(Guid locationId);
    }
}