using FluentValidation;
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