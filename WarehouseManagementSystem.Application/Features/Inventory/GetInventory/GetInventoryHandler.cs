using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Inventory.Common;

namespace WarehouseManagementSystem.Application.Features.Inventory.GetInventory
{
    public class GetInventoryHandler
    {
        private readonly IInventoryRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetInventoryHandler(
            IInventoryRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResponse<InventoryDto>> Handle(GetInventoryQuery query)
        {
            var filter = new InventoryFilter
            {
                ProductId = query.ProductId,
                LocationId = query.LocationId,
                IsLowStock = query.IsLowStock,
                SortBy = query.SortBy,
                Descending = query.Descending,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            // Scope by role
            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                // Force filter to their warehouse only
                filter.WarehouseId = _currentUser.WarehouseId;
            }
            else if (_currentUser.IsRegionalManager)
            {
                // RegionalManager can filter by warehouse
                // but only within their country — enforced in repository.
                // A missing Country must never fall through to "no filter",
                // or a misconfigured RegionalManager would see every
                // warehouse in every country.
                if (string.IsNullOrWhiteSpace(_currentUser.Country))
                    throw new UnauthorizedException(
                        "Your account has no country assigned. Contact an administrator.");

                filter.WarehouseId = query.WarehouseId;
                filter.Country = _currentUser.Country;
            }
            else
            {
                // Admin can filter by any warehouse
                filter.WarehouseId = query.WarehouseId;
            }

            var (inventory, totalCount) = await _repository.GetAllAsync(filter);

            var items = inventory.Select(InventoryMapper.ToDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<InventoryDto>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}