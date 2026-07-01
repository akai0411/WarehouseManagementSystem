using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement
{
    public class CreateStockMovementHandler
    {
        private readonly IStockMovementRepository _repository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateStockMovementCommand> _validator;

        public CreateStockMovementHandler(
            IStockMovementRepository repository,
            IInventoryRepository inventoryRepository,
            ICurrentUserService currentUser,
            IValidator<CreateStockMovementCommand> validator)
        {
            _repository = repository;
            _inventoryRepository = inventoryRepository;
            _currentUser = currentUser;
            _validator = validator;
        }

        public async Task<CreateStockMovementResponse> Handle(
            CreateStockMovementCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var inventory = await _inventoryRepository.GetByIdAsync(command.InventoryId)
                ?? throw new NotFoundException("Inventory not found.");

            var warehouseId = inventory.Location.Zone.WarehouseId;

            // Operators and WarehouseManagers can only move stock in their warehouse
            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                if (warehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You can only register movements in your own warehouse.");
            }

            // Only WarehouseManager and Admin can do adjustments
            var movementType = Enum.Parse<MovementType>(command.Type);
            if (movementType == MovementType.Adjustment && _currentUser.IsOperator)
                throw new UnauthorizedException(
                    "Operators are not allowed to register adjustments.");

            // Apply movement to inventory.
            // quantityToRecord is what gets written to the movement's audit
            // trail: for Inbound/Outbound it mirrors the requested quantity,
            // for Adjustment it's the signed delta actually applied.
            int quantityToRecord = command.Quantity;

            switch (movementType)
            {
                case MovementType.Inbound:
                    inventory.Quantity += command.Quantity;
                    break;

                case MovementType.Outbound:
                    if (inventory.Quantity - command.Quantity < 0)
                        throw new BusinessRuleException(
                            $"Insufficient stock. Current quantity: {inventory.Quantity}, " +
                            $"requested: {command.Quantity}.");
                    inventory.Quantity -= command.Quantity;
                    break;

                case MovementType.Adjustment:
                    // command.Quantity is the actual physical count.
                    // Record the difference from the previous count so the
                    // movement history reflects what actually changed.
                    quantityToRecord = command.Quantity - inventory.Quantity;
                    inventory.Quantity = command.Quantity;
                    break;
            }

            // Save updated inventory quantity
            await _inventoryRepository.UpdateAsync(inventory);

            // Create the movement record
            var movement = StockMovementMapper.ToEntity(
                command, _currentUser.UserId, quantityToRecord);
            await _repository.AddAsync(movement);

            // Build response with warnings
            var warnings = new List<string>();
            if (inventory.Quantity <= inventory.MinimumStockLevel)
                warnings.Add(
                    $"Product '{inventory.Product.Name}' at location " +
                    $"'{inventory.Location.Code}' is at or below minimum stock level " +
                    $"({inventory.Quantity}/{inventory.MinimumStockLevel}).");

            return new CreateStockMovementResponse
            {
                MovementId = movement.Id,
                NewQuantity = inventory.Quantity,
                Warnings = warnings
            };
        }
    }
}