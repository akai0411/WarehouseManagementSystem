using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IWarehouseRepository
    {
        Task AddAsync(Warehouse warehouse);
        Task<(List<Warehouse> Items, int TotalCount)> GetAllAsync(WarehouseFilter filter);
        Task<Warehouse?> GetByIdAsync(Guid id);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(Warehouse warehouse);
        Task<Warehouse?> GetByNameAsync(string name);
        Task<Warehouse?> GetByAddressAsync(string streetName, string number, string city, string country);
    }
}