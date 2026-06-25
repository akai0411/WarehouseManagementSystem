using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;

namespace WarehouseManagementSystem.Application.Features.Warehouses.CreateWarehouse
{
    public class CreateWarehouseHandler
    {
        private readonly IWarehouseRepository _repository;
        private readonly IValidator<CreateWarehouseCommand> _validator;

        public CreateWarehouseHandler(
            IWarehouseRepository repository,
            IValidator<CreateWarehouseCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateWarehouseCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existingName = await _repository.GetByNameAsync(command.Name);
            if (existingName != null)
                throw new ConflictException($"A warehouse with name '{command.Name}' already exists.");

            var existingAddress = await _repository.GetByAddressAsync(
                command.StreetName,
                command.Number,
                command.City,
                command.Country);
            if (existingAddress != null)
                throw new ConflictException("A warehouse at this address already exists.");

            var warehouse = WarehouseMapper.ToEntity(command);

            await _repository.AddAsync(warehouse);

            return warehouse.Id;
        }
    }
}