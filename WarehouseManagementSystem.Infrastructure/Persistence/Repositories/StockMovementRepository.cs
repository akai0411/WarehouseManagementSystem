using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly AppDbContext _context;

        public StockMovementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockMovement movement)
        {
            await _context.StockMovements.AddAsync(movement);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<StockMovement> Items, int TotalCount)> GetAllAsync(
            StockMovementFilter filter)
        {
            var movements = _context.StockMovements
                .Include(m => m.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(m => m.Inventory)
                    .ThenInclude(i => i.Location)
                        .ThenInclude(l => l.Zone)
                            .ThenInclude(z => z.Warehouse)
                .AsQueryable();

            if (filter.InventoryId.HasValue)
                movements = movements.Where(m =>
                    m.InventoryId == filter.InventoryId.Value);

            if (filter.ProductId.HasValue)
                movements = movements.Where(m =>
                    m.Inventory.ProductId == filter.ProductId.Value);

            if (filter.WarehouseId.HasValue)
                movements = movements.Where(m =>
                    m.Inventory.Location.Zone.WarehouseId == filter.WarehouseId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Country))
                movements = movements.Where(m =>
                    m.Inventory.Location.Zone.Warehouse.Address.Country == filter.Country);

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                Enum.TryParse<MovementType>(filter.Type, out var movementType))
                movements = movements.Where(m => m.Type == movementType);

            if (filter.From.HasValue)
                movements = movements.Where(m => m.CreatedAt >= filter.From.Value);

            if (filter.To.HasValue)
                movements = movements.Where(m => m.CreatedAt <= filter.To.Value);

            var totalCount = await movements.CountAsync();

            movements = filter.SortBy?.ToLower() switch
            {
                "type" => filter.Descending
                    ? movements.OrderByDescending(m => m.Type)
                    : movements.OrderBy(m => m.Type),
                "quantity" => filter.Descending
                    ? movements.OrderByDescending(m => m.Quantity)
                    : movements.OrderBy(m => m.Quantity),
                _ => filter.Descending
                    ? movements.OrderByDescending(m => m.CreatedAt)
                    : movements.OrderBy(m => m.CreatedAt)
            };

            var items = await movements
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<StockMovement?> GetByIdAsync(Guid id)
        {
            return await _context.StockMovements
                .Include(m => m.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(m => m.Inventory)
                    .ThenInclude(i => i.Location)
                        .ThenInclude(l => l.Zone)
                            .ThenInclude(z => z.Warehouse)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}