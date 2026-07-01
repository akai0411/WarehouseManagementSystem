using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Locations.Common;

namespace WarehouseManagementSystem.Application.Features.Locations.GetLocationById
{
    public class GetLocationByIdHandler
    {
        private readonly ILocationRepository _repository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;

        public GetLocationByIdHandler(
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

        public async Task<LocationDto?> Handle(Guid zoneId, Guid locationId)
        {
            var zone = await _zoneRepository.GetByIdAsync(zoneId)
                ?? throw new NotFoundException("Zone not found.");

            await ValidateAccessAsync(zone.WarehouseId);

            var location = await _repository.GetByIdAsync(locationId);

            if (location == null || location.ZoneId != zoneId) return null;

            return LocationMapper.ToDto(location);
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