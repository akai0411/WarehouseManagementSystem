using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovementById;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.StockMovements
{
    public class GetStockMovementByIdHandlerTests
    {
        private readonly Mock<IStockMovementRepository> _repository = new();

        private GetStockMovementByIdHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_repository.Object, currentUser.Object);

        private static StockMovement CreateMovement(Guid warehouseId, string country = "Spain")
        {
            var inventory = EntityFactory.CreateInventory(warehouseId: warehouseId, country: country);

            return new StockMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventory.Id,
                Inventory = inventory,
                Type = MovementType.Inbound,
                Quantity = 5,
                CreatedBy = Guid.NewGuid()
            };
        }

        [Fact]
        public async Task Handle_MovementNotFound_ReturnsNull()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((StockMovement?)null);

            var result = await CreateHandler(CurrentUserMockFactory.Admin()).Handle(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_WarehouseManagerOwnWarehouse_ReturnsDto()
        {
            var warehouseId = Guid.NewGuid();
            var movement = CreateMovement(warehouseId);
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(warehouseId));
            var result = await handler.Handle(movement.Id);

            Assert.NotNull(result);
            Assert.Equal(movement.Id, result!.Id);
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var movement = CreateMovement(Guid.NewGuid());
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(movement.Id));
        }

        [Fact]
        public async Task Handle_OperatorDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var movement = CreateMovement(Guid.NewGuid());
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.Operator(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(movement.Id));
        }

        [Fact]
        public async Task Handle_RegionalManagerSameCountry_ReturnsDto()
        {
            var movement = CreateMovement(Guid.NewGuid(), country: "Spain");
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("Spain"));
            var result = await handler.Handle(movement.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_RegionalManagerDifferentCountry_ThrowsUnauthorizedException()
        {
            var movement = CreateMovement(Guid.NewGuid(), country: "Spain");
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("France"));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(movement.Id));
        }

        [Fact]
        public async Task Handle_Admin_ReturnsDtoRegardlessOfWarehouse()
        {
            var movement = CreateMovement(Guid.NewGuid());
            _repository.Setup(r => r.GetByIdAsync(movement.Id)).ReturnsAsync(movement);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(movement.Id);

            Assert.NotNull(result);
        }
    }
}
