using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Warehouses.Common;

namespace WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses
{
    public class GetWarehousesHandler
    {
        private readonly IWarehouseRepository _repository;

        public GetWarehousesHandler(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<WarehouseDto>> Handle(GetWarehousesQuery query)
        {
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