using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Locations.Common;

namespace WarehouseManagementSystem.Application.Features.Locations.GetLocations
{
    public class GetLocationsHandler
    {
        private readonly ILocationRepository _repository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;

        public GetLocationsHandler(
            ILocationRepository repository,
            IZoneRepository zoneRepository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _zoneRepository = zoneRepository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResponse<LocationDto>> Handle(Guid zoneId, GetLocationsQuery query)
        {
            var zone = await _zoneRepository.GetByIdAsync(zoneId)
                ?? throw new NotFoundException("Zone not found.");

            await ValidateAccessAsync(zone.WarehouseId);

            var filter = new LocationFilter
            {
                Row = query.Row,
                HasInventory = query.HasInventory,
                SortBy = query.SortBy,
                Descending = query.Descending,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            var (locations, totalCount) = await _repository.GetAllAsync(zoneId, filter);

            var items = locations.Select(LocationMapper.ToDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<LocationDto>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        private async Task ValidateAccessAsync(Guid warehouseId)
        {
            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                if (warehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You do not have access to this zone.");
            }

            if (_currentUser.IsRegionalManager)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
                if (warehouse?.Address.Country != _currentUser.Country)
                    throw new UnauthorizedException(
                        "You do not have access to this zone.");
            }
        }
    }
}