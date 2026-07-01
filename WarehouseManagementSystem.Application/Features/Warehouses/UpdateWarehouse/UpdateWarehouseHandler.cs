using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.ValueObjects;

namespace WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse
{
    public class UpdateWarehouseHandler
    {
        private readonly IWarehouseRepository _repository;
        private readonly IValidator<UpdateWarehouseCommand> _validator;

        public UpdateWarehouseHandler(
            IWarehouseRepository repository,
            IValidator<UpdateWarehouseCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<bool> Handle(Guid id, UpdateWarehouseCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var warehouse = await _repository.GetByIdAsync(id);

            if (warehouse == null) return false;

            // Check name uniqueness excluding current warehouse
            var existingName = await _repository.GetByNameAsync(command.Name);
            if (existingName != null && existingName.Id != id)
                throw new ConflictException($"A warehouse with name '{command.Name}' already exists.");

            // Check address uniqueness excluding current warehouse
            var existingAddress = await _repository.GetByAddressAsync(
                command.StreetName,
                command.Number,
                command.City,
                command.Country);
            if (existingAddress != null && existingAddress.Id != id)
                throw new ConflictException("A warehouse at this address already exists.");

            warehouse.Name = command.Name;
            warehouse.Description = command.Description;
            warehouse.Address = new Address(
                command.StreetType,
                command.StreetName,
                command.Number,
                command.PostalCode,
                command.City,
                command.Country);

            await _repository.UpdateAsync(warehouse);

            return true;
        }
    }
}