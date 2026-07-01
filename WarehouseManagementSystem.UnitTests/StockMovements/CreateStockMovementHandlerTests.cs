using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;
// "Inventory" here would resolve to the sibling
// WarehouseManagementSystem.UnitTests.Inventory namespace, not the
// Domain.Entities.Inventory type, so alias it explicitly.
using InventoryEntity = WarehouseManagementSystem.Domain.Entities.Inventory;

namespace WarehouseManagementSystem.UnitTests.StockMovements
{
    public class CreateStockMovementHandlerTests
    {
        private readonly Mock<IStockMovementRepository> _movementRepository = new();
        private readonly Mock<IInventoryRepository> _inventoryRepository = new();
        private readonly CreateStockMovementCommandValidator _validator = new();

        private CreateStockMovementHandler CreateHandler(Mock<ICurrentUserService> currentUser)
        {
            return new CreateStockMovementHandler(
                _movementRepository.Object,
                _inventoryRepository.Object,
                currentUser.Object,
                _validator);
        }

        [Fact]
        public async Task Handle_Inbound_IncreasesQuantityAndRecordsRequestedQuantity()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10, minimumStockLevel: 0);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            StockMovement? captured = null;
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Callback<StockMovement>(m => captured = m)
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Inbound),
                Quantity = 5
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var response = await handler.Handle(command);

            Assert.Equal(15, inventory.Quantity);
            Assert.Equal(15, response.NewQuantity);
            Assert.Equal(5, captured!.Quantity);
            Assert.Equal(MovementType.Inbound, captured.Type);
            _inventoryRepository.Verify(r => r.UpdateAsync(inventory), Times.Once);
        }

        [Fact]
        public async Task Handle_Outbound_DecreasesQuantityAndRecordsRequestedQuantity()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10, minimumStockLevel: 0);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            StockMovement? captured = null;
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Callback<StockMovement>(m => captured = m)
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Outbound),
                Quantity = 4
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var response = await handler.Handle(command);

            Assert.Equal(6, inventory.Quantity);
            Assert.Equal(6, response.NewQuantity);
            Assert.Equal(4, captured!.Quantity);
        }

        [Fact]
        public async Task Handle_OutboundInsufficientStock_ThrowsBusinessRuleExceptionAndDoesNotPersist()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 3);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Outbound),
                Quantity = 5
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(command));

            Assert.Equal(3, inventory.Quantity); // untouched
            _inventoryRepository.Verify(r => r.UpdateAsync(It.IsAny<InventoryEntity>()), Times.Never);
            _movementRepository.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AdjustmentIncreasesCount_RecordsPositiveDeltaNotRawCount()
        {
            // System says 10, physical count says 15 -> the audit trail
            // should show "+5", not "15" (the fixed behavior).
            var inventory = EntityFactory.CreateInventory(quantity: 10, minimumStockLevel: 0);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            StockMovement? captured = null;
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Callback<StockMovement>(m => captured = m)
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Adjustment),
                Quantity = 15
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(inventory.Location.Zone.WarehouseId));
            var response = await handler.Handle(command);

            Assert.Equal(15, inventory.Quantity);
            Assert.Equal(15, response.NewQuantity);
            Assert.Equal(5, captured!.Quantity); // delta, not the raw 15
        }

        [Fact]
        public async Task Handle_AdjustmentDecreasesCount_RecordsNegativeDelta()
        {
            // System says 20, physical count says 12 -> delta is -8.
            var inventory = EntityFactory.CreateInventory(quantity: 20, minimumStockLevel: 0);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            StockMovement? captured = null;
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Callback<StockMovement>(m => captured = m)
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Adjustment),
                Quantity = 12
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(inventory.Location.Zone.WarehouseId));
            var response = await handler.Handle(command);

            Assert.Equal(12, inventory.Quantity);
            Assert.Equal(-8, captured!.Quantity);
        }

        [Fact]
        public async Task Handle_OperatorAttemptsAdjustment_ThrowsUnauthorizedExceptionAndDoesNotPersist()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Adjustment),
                Quantity = 5
            };

            var handler = CreateHandler(
                CurrentUserMockFactory.Operator(inventory.Location.Zone.WarehouseId));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command));
            _inventoryRepository.Verify(r => r.UpdateAsync(It.IsAny<InventoryEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WarehouseManagerAdjustment_IsAllowed()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Adjustment),
                Quantity = 8
            };

            var handler = CreateHandler(
                CurrentUserMockFactory.WarehouseManager(inventory.Location.Zone.WarehouseId));

            var response = await handler.Handle(command);

            Assert.Equal(8, inventory.Quantity);
        }

        [Fact]
        public async Task Handle_InventoryNotFound_ThrowsNotFoundException()
        {
            _inventoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((InventoryEntity?)null);

            var command = new CreateStockMovementCommand
            {
                InventoryId = Guid.NewGuid(),
                Type = nameof(MovementType.Inbound),
                Quantity = 1
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Inbound),
                Quantity = 1
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_ResultingQuantityAtOrBelowMinimum_GeneratesLowStockWarning()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10, minimumStockLevel: 5);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Outbound),
                Quantity = 6 // 10 - 6 = 4, at or below minimum of 5
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var response = await handler.Handle(command);

            Assert.Single(response.Warnings);
        }

        [Fact]
        public async Task Handle_ResultingQuantityAboveMinimum_NoWarning()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 10, minimumStockLevel: 5);
            _inventoryRepository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);
            _movementRepository.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStockMovementCommand
            {
                InventoryId = inventory.Id,
                Type = nameof(MovementType.Inbound),
                Quantity = 1
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var response = await handler.Handle(command);

            Assert.Empty(response.Warnings);
        }

        [Fact]
        public async Task Handle_ZeroQuantity_ThrowsValidationExceptionBeforeTouchingInventory()
        {
            var command = new CreateStockMovementCommand
            {
                InventoryId = Guid.NewGuid(),
                Type = nameof(MovementType.Inbound),
                Quantity = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command));
            _inventoryRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
