using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;

namespace WarehouseManagementSystem.Application.Features.Inventory.CreateInventory
{
    public class CreateInventoryHandler
    {
        private readonly IInventoryRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateInventoryCommand> _validator;

        public CreateInventoryHandler(
            IInventoryRepository repository,
            IProductRepository productRepository,
            ILocationRepository locationRepository,
            ICurrentUserService currentUser,
            IValidator<CreateInventoryCommand> validator)
        {
            _repository = repository;
            _productRepository = productRepository;
            _locationRepository = locationRepository;
            _currentUser = currentUser;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateInventoryCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // Validate product exists
            _ = await _productRepository.GetByIdAsync(command.ProductId)
                ?? throw new NotFoundException("Product not found.");

            // Validate location exists
            var location = await _locationRepository.GetByIdAsync(command.LocationId)
                ?? throw new NotFoundException("Location not found.");

            // WarehouseManager can only assign inventory in their warehouse
            if (_currentUser.IsWarehouseManager)
            {
                if (location.Zone.WarehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You can only assign inventory to locations in your warehouse.");
            }

            // Check location is not already occupied
            var existingInventory = await _repository
                .GetByLocationIdAsync(command.LocationId);
            if (existingInventory != null)
                throw new ConflictException(
                    "This location already has a product assigned.");

            var inventory = InventoryMapper.ToEntity(command);

            await _repository.AddAsync(inventory);

            return inventory.Id;
        }
    }
}