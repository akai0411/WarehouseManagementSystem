using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Warehouse> Items, int TotalCount)> GetAllAsync(WarehouseFilter filter)
        {
            var warehouses = _context.Warehouses
                .Where(w => !w.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                warehouses = warehouses.Where(w => w.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.City))
                warehouses = warehouses.Where(w => w.Address.City.Contains(filter.City));

            if (!string.IsNullOrWhiteSpace(filter.Country))
                warehouses = warehouses.Where(w => w.Address.Country.Contains(filter.Country));

            var totalCount = await warehouses.CountAsync();

            warehouses = filter.SortBy?.ToLower() switch
            {
                "name" => filter.Descending
                    ? warehouses.OrderByDescending(w => w.Name)
                    : warehouses.OrderBy(w => w.Name),
                "city" => filter.Descending
                    ? warehouses.OrderByDescending(w => w.Address.City)
                    : warehouses.OrderBy(w => w.Address.City),
                "country" => filter.Descending
                    ? warehouses.OrderByDescending(w => w.Address.Country)
                    : warehouses.OrderBy(w => w.Address.Country),
                _ => warehouses.OrderBy(w => w.Name)
            };

            var items = await warehouses
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Warehouse?> GetByIdAsync(Guid id)
        {
            return await _context.Warehouses
                .Include(w => w.Zones)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        }
        public async Task<Warehouse?> GetByNameAsync(string name)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Name == name && !w.IsDeleted);
        }

        public async Task<Warehouse?> GetByAddressAsync(string streetName, string number, string city, string country)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                    w.Address.StreetName == streetName &&
                    w.Address.Number == number &&
                    w.Address.City == city &&
                    w.Address.Country == country &&
                    !w.IsDeleted);
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Warehouse warehouse)
        {
            warehouse.IsDeleted = true;
            warehouse.DeletedAt = DateTime.UtcNow;

            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveZonesAsync(Guid warehouseId)
        {
            return await _context.Zones
                .AnyAsync(z => z.WarehouseId == warehouseId && !z.IsDeleted);
        }
    }
}