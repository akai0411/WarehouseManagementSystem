using FluentValidation;
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

            var warehouse = WarehouseMapper.ToEntity(command);

            await _repository.AddAsync(warehouse);

            return warehouse.Id;
        }
    }
}