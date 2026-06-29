using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;

namespace WarehouseManagementSystem.Application.Features.Zones.CreateZone
{
    public class CreateZoneHandler
    {
        private readonly IZoneRepository _repository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IValidator<CreateZoneCommand> _validator;

        public CreateZoneHandler(
            IZoneRepository repository,
            IWarehouseRepository warehouseRepository,
            IValidator<CreateZoneCommand> validator)
        {
            _repository = repository;
            _warehouseRepository = warehouseRepository;
            _validator = validator;
        }

        public async Task<Guid> Handle(Guid warehouseId, CreateZoneCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            _ = await _warehouseRepository.GetByIdAsync(warehouseId)
                ?? throw new NotFoundException("Warehouse not found.");

            var existingZone = await _repository
                .GetByNameAndWarehouseAsync(command.Name, warehouseId);
            if (existingZone != null)
                throw new ConflictException(
                    $"A zone with name '{command.Name}' already exists in this warehouse.");

            var zone = ZoneMapper.ToEntity(command, warehouseId);

            await _repository.AddAsync(zone);

            return zone.Id;
        }
    }
}