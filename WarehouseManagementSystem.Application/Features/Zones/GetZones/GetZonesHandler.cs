using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Zones.Common;

namespace WarehouseManagementSystem.Application.Features.Zones.GetZones
{
    public class GetZonesHandler
    {
        private readonly IZoneRepository _repository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;

        public GetZonesHandler(
            IZoneRepository repository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResponse<ZoneDto>> Handle(Guid warehouseId, GetZonesQuery query)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId)
                ?? throw new NotFoundException("Warehouse not found.");

            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                if (warehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You do not have access to this warehouse.");
            }

            if (_currentUser.IsRegionalManager)
            {
                if (warehouse.Address.Country != _currentUser.Country)
                    throw new UnauthorizedException(
                        "You do not have access to this warehouse.");
            }

            var filter = new ZoneFilter
            {
                Name = query.Name,
                Type = query.Type,
                SortBy = query.SortBy,
                Descending = query.Descending,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            var (zones, totalCount) = await _repository.GetAllAsync(warehouseId, filter);

            var items = zones.Select(ZoneMapper.ToDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<ZoneDto>
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