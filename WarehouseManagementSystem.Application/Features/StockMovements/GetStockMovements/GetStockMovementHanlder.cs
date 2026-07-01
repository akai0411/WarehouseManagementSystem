using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.StockMovements.Common;

namespace WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovements
{
    public class GetStockMovementsHandler
    {
        private readonly IStockMovementRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetStockMovementsHandler(
            IStockMovementRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResponse<StockMovementDto>> Handle(
            GetStockMovementsQuery query)
        {
            var filter = new StockMovementFilter
            {
                InventoryId = query.InventoryId,
                ProductId = query.ProductId,
                Type = query.Type,
                From = query.From,
                To = query.To,
                SortBy = query.SortBy,
                Descending = query.Descending,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            // Scope by role
            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                filter.WarehouseId = _currentUser.WarehouseId;
            }
            else if (_currentUser.IsRegionalManager)
            {
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
                filter.WarehouseId = query.WarehouseId;
            }

            var (movements, totalCount) = await _repository.GetAllAsync(filter);

            var items = movements.Select(StockMovementMapper.ToDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<StockMovementDto>
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