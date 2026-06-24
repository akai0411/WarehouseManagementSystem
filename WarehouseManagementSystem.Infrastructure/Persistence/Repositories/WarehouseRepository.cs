using Microsoft.EntityFrameworkCore;
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

        public async Task<(List<Warehouse> Items, int TotalCount)> GetAllAsync(GetWarehousesQuery query)
        {
            var warehouses = _context.Warehouses
                .Where(w => !w.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query.Name))
                warehouses = warehouses.Where(w => w.Name.Contains(query.Name));

            if (!string.IsNullOrWhiteSpace(query.City))
                warehouses = warehouses.Where(w => w.Address.City.Contains(query.City));

            if (!string.IsNullOrWhiteSpace(query.Country))
                warehouses = warehouses.Where(w => w.Address.Country.Contains(query.Country));

            var totalCount = await warehouses.CountAsync();

            warehouses = query.SortBy?.ToLower() switch
            {
                "name" => query.Descending
                    ? warehouses.OrderByDescending(w => w.Name)
                    : warehouses.OrderBy(w => w.Name),

                "city" => query.Descending
                    ? warehouses.OrderByDescending(w => w.Address.City)
                    : warehouses.OrderBy(w => w.Address.City),

                "country" => query.Descending
                    ? warehouses.OrderByDescending(w => w.Address.Country)
                    : warehouses.OrderBy(w => w.Address.Country),

                _ => warehouses.OrderBy(w => w.Name)
            };

            var items = await warehouses
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Warehouse?> GetByIdAsync(Guid id)
        {
            return await _context.Warehouses
                .Include(w => w.Zones)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
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
    }
}