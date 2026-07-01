using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Inventory inventory)
        {
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Inventory> Items, int TotalCount)> GetAllAsync(
            InventoryFilter filter)
        {
            var inventory = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Zone)
                        .ThenInclude(z => z.Warehouse)
                .Where(i => !i.IsDeleted);

            if (filter.WarehouseId.HasValue)
                inventory = inventory.Where(i =>
                    i.Location.Zone.WarehouseId == filter.WarehouseId.Value);

            // RegionalManager scoping by country
            if (!string.IsNullOrWhiteSpace(filter.Country))
                inventory = inventory.Where(i =>
                    i.Location.Zone.Warehouse.Address.Country == filter.Country);

            if (filter.ProductId.HasValue)
                inventory = inventory.Where(i => i.ProductId == filter.ProductId.Value);

            if (filter.LocationId.HasValue)
                inventory = inventory.Where(i => i.LocationId == filter.LocationId.Value);

            if (filter.IsLowStock.HasValue && filter.IsLowStock.Value)
                inventory = inventory.Where(i => i.Quantity <= i.MinimumStockLevel);

            var totalCount = await inventory.CountAsync();

            inventory = filter.SortBy?.ToLower() switch
            {
                "quantity" => filter.Descending
                    ? inventory.OrderByDescending(i => i.Quantity)
                    : inventory.OrderBy(i => i.Quantity),
                "product" => filter.Descending
                    ? inventory.OrderByDescending(i => i.Product.Name)
                    : inventory.OrderBy(i => i.Product.Name),
                "location" => filter.Descending
                    ? inventory.OrderByDescending(i => i.Location.Code)
                    : inventory.OrderBy(i => i.Location.Code),
                _ => inventory.OrderBy(i => i.Product.Name)
            };

            var items = await inventory
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Zone)
                        .ThenInclude(z => z.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Inventory?> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.LocationId == locationId && !i.IsDeleted);
        }

        public async Task DeleteAsync(Inventory inventory)
        {
            inventory.IsDeleted = true;
            inventory.DeletedAt = DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}