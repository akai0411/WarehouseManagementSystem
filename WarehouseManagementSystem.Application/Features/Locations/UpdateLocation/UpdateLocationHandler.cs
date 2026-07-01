using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Locations.UpdateLocation
{
    public class UpdateLocationHandler
    {
        private readonly ILocationRepository _repository;
        private readonly IValidator<UpdateLocationCommand> _validator;

        public UpdateLocationHandler(
            ILocationRepository repository,
            IValidator<UpdateLocationCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<bool> Handle(Guid zoneId, Guid locationId, UpdateLocationCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var location = await _repository.GetByIdAsync(locationId);

            if (location == null || location.ZoneId != zoneId) return false;

            var newCode = $"{command.Row}-{command.Shelf:D2}-{command.Bin:D2}";

            var existingLocation = await _repository
                .GetByCodeAndZoneAsync(newCode, zoneId);
            if (existingLocation != null && existingLocation.Id != locationId)
                throw new ConflictException(
                    $"A location with code '{newCode}' already exists in this zone.");

            location.Row = command.Row;
            location.Shelf = command.Shelf;
            location.Bin = command.Bin;
            location.Code = newCode;

            await _repository.UpdateAsync(location);

            return true;
        }
    }
}