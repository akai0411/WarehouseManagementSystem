using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;

        public LocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Location location)
        {
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Location> Items, int TotalCount)> GetAllAsync(
            Guid zoneId, LocationFilter filter)
        {
            var locations = _context.Locations
                .Include(l => l.Inventory)
                .Where(l => l.ZoneId == zoneId && !l.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Row))
                locations = locations.Where(l => l.Row == filter.Row);

            if (filter.HasInventory.HasValue)
                locations = filter.HasInventory.Value
                    ? locations.Where(l => l.Inventory != null)
                    : locations.Where(l => l.Inventory == null);

            var totalCount = await locations.CountAsync();

            locations = filter.SortBy?.ToLower() switch
            {
                "row" => filter.Descending
                    ? locations.OrderByDescending(l => l.Row)
                    : locations.OrderBy(l => l.Row),
                "code" => filter.Descending
                    ? locations.OrderByDescending(l => l.Code)
                    : locations.OrderBy(l => l.Code),
                _ => locations.OrderBy(l => l.Code)
            };

            var items = await locations
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Location?> GetByIdAsync(Guid id)
        {
            return await _context.Locations
                .Include(l => l.Inventory)
                .Include(l => l.Zone)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Location location)
        {
            location.IsDeleted = true;
            location.DeletedAt = DateTime.UtcNow;
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task<Location?> GetByCodeAndZoneAsync(string code, Guid zoneId)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l =>
                    l.Code == code &&
                    l.ZoneId == zoneId &&
                    !l.IsDeleted);
        }

        public async Task<bool> HasInventoryAsync(Guid locationId)
        {
            return await _context.Inventories
                .AnyAsync(i => i.LocationId == locationId && !i.IsDeleted);
        }
    }
}