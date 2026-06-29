using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;

namespace WarehouseManagementSystem.Application.Features.Locations.CreateLocation
{
    public class CreateLocationHandler
    {
        private readonly ILocationRepository _repository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IValidator<CreateLocationCommand> _validator;

        public CreateLocationHandler(
            ILocationRepository repository,
            IZoneRepository zoneRepository,
            IValidator<CreateLocationCommand> validator)
        {
            _repository = repository;
            _zoneRepository = zoneRepository;
            _validator = validator;
        }

        public async Task<Guid> Handle(Guid zoneId, CreateLocationCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            _ = await _zoneRepository.GetByIdAsync(zoneId)
                ?? throw new NotFoundException("Zone not found.");

            var code = $"{command.Row}-{command.Shelf:D2}-{command.Bin:D2}";

            var existingLocation = await _repository
                .GetByCodeAndZoneAsync(code, zoneId);
            if (existingLocation != null)
                throw new ConflictException(
                    $"A location with code '{code}' already exists in this zone.");

            var location = LocationMapper.ToEntity(command, zoneId);

            await _repository.AddAsync(location);

            return location.Id;
        }
    }
}