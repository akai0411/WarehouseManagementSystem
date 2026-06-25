using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Warehouses.Common;

namespace WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses
{
    public class GetWarehousesHandler
    {
        private readonly IWarehouseRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetWarehousesHandler(
            IWarehouseRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResponse<WarehouseDto>> Handle(GetWarehousesQuery query)
        {
            // RegionalManager only sees warehouses in their country
            if (_currentUser.IsRegionalManager)
                query.Country = _currentUser.Country;

            var (warehouses, totalCount) = await _repository.GetAllAsync(query);

            var items = warehouses
                .Select(WarehouseMapper.ToDto)
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<WarehouseDto>
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