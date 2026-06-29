using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Zones.Common;

namespace WarehouseManagementSystem.Application.Features.Zones.GetZoneById
{
    public class GetZoneByIdHandler
    {
        private readonly IZoneRepository _repository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;

        public GetZoneByIdHandler(
            IZoneRepository repository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
        }

        public async Task<ZoneDto?> Handle(Guid warehouseId, Guid zoneId)
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

            var zone = await _repository.GetByIdAsync(zoneId);

            if (zone == null || zone.WarehouseId != warehouseId) return null;

            return ZoneMapper.ToDto(zone);
        }
    }
}