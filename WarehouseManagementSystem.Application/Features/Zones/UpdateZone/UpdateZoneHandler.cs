using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Features.Zones.UpdateZone
{
    public class UpdateZoneHandler
    {
        private readonly IZoneRepository _repository;
        private readonly IValidator<UpdateZoneCommand> _validator;

        public UpdateZoneHandler(
            IZoneRepository repository,
            IValidator<UpdateZoneCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<bool> Handle(Guid warehouseId, Guid zoneId, UpdateZoneCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var zone = await _repository.GetByIdAsync(zoneId);

            if (zone == null || zone.WarehouseId != warehouseId) return false;

            var existingZone = await _repository
                .GetByNameAndWarehouseAsync(command.Name, warehouseId);
            if (existingZone != null && existingZone.Id != zoneId)
                throw new ConflictException(
                    $"A zone with name '{command.Name}' already exists in this warehouse.");

            zone.Name = command.Name;
            zone.Description = command.Description;
            zone.Type = Enum.Parse<ZoneType>(command.Type);

            await _repository.UpdateAsync(zone);

            return true;
        }
    }
}