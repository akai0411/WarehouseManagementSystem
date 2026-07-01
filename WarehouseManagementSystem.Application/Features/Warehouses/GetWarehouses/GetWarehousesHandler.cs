using WarehouseManagementSystem.Application.Common.Filters;
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
            var filter = new WarehouseFilter
            {
                Name = query.Name,
                City = query.City,
                Country = _currentUser.IsRegionalManager ? _currentUser.Country : query.Country,
                SortBy = query.SortBy,
                Descending = query.Descending,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            var (warehouses, totalCount) = await _repository.GetAllAsync(filter);

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