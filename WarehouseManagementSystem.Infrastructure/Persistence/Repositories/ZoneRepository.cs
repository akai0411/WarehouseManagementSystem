using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class ZoneRepository : IZoneRepository
    {
        private readonly AppDbContext _context;

        public ZoneRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Zone zone)
        {
            await _context.Zones.AddAsync(zone);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Zone> Items, int TotalCount)> GetAllAsync(
            Guid warehouseId, ZoneFilter filter)
        {
            var zones = _context.Zones
                .Where(z => z.WarehouseId == warehouseId && !z.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                zones = zones.Where(z => z.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                Enum.TryParse<ZoneType>(filter.Type, out var zoneType))
                zones = zones.Where(z => z.Type == zoneType);

            var totalCount = await zones.CountAsync();

            zones = filter.SortBy?.ToLower() switch
            {
                "name" => filter.Descending
                    ? zones.OrderByDescending(z => z.Name)
                    : zones.OrderBy(z => z.Name),
                "type" => filter.Descending
                    ? zones.OrderByDescending(z => z.Type)
                    : zones.OrderBy(z => z.Type),
                _ => zones.OrderBy(z => z.Name)
            };

            var items = await zones
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Zone?> GetByIdAsync(Guid id)
        {
            return await _context.Zones
                .FirstOrDefaultAsync(z => z.Id == id && !z.IsDeleted);
        }

        public async Task UpdateAsync(Zone zone)
        {
            _context.Zones.Update(zone);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Zone zone)
        {
            zone.IsDeleted = true;
            zone.DeletedAt = DateTime.UtcNow;
            _context.Zones.Update(zone);
            await _context.SaveChangesAsync();
        }

        public async Task<Zone?> GetByNameAndWarehouseAsync(string name, Guid warehouseId)
        {
            return await _context.Zones
                .FirstOrDefaultAsync(z =>
                    z.Name == name &&
                    z.WarehouseId == warehouseId &&
                    !z.IsDeleted);
        }

        public async Task<bool> HasActiveLocationsAsync(Guid zoneId)
        {
            return await _context.Locations
                .AnyAsync(l => l.ZoneId == zoneId && !l.IsDeleted);
        }
    }
}