using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IZoneRepository
    {
        Task AddAsync(Zone zone);
        Task<(List<Zone> Items, int TotalCount)> GetAllAsync(Guid warehouseId, ZoneFilter filter);
        Task<Zone?> GetByIdAsync(Guid id);
        Task UpdateAsync(Zone zone);
        Task DeleteAsync(Zone zone);
        Task<Zone?> GetByNameAndWarehouseAsync(string name, Guid warehouseId);
        Task<bool> HasActiveLocationsAsync(Guid zoneId);
    }
}