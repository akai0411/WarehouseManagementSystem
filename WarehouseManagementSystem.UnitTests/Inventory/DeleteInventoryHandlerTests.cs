using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Inventory.DeleteInventory;
using WarehouseManagementSystem.UnitTests.TestHelpers;
using InventoryEntity = WarehouseManagementSystem.Domain.Entities.Inventory;

namespace WarehouseManagementSystem.UnitTests.Inventory
{
    public class DeleteInventoryHandlerTests
    {
        private readonly Mock<IInventoryRepository> _repository = new();
        private readonly DeleteInventoryHandler _handler;

        public DeleteInventoryHandlerTests()
        {
            _handler = new DeleteInventoryHandler(_repository.Object);
        }

        [Fact]
        public async Task Handle_InventoryNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((InventoryEntity?)null);

            var result = await _handler.Handle(Guid.NewGuid());

            Assert.False(result);
            _repository.Verify(r => r.DeleteAsync(It.IsAny<InventoryEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InventoryHasStock_ThrowsBusinessRuleExceptionAndDoesNotDelete()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 5);
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(inventory.Id));
            _repository.Verify(r => r.DeleteAsync(It.IsAny<InventoryEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InventoryHasZeroStock_DeletesAndReturnsTrue()
        {
            var inventory = EntityFactory.CreateInventory(quantity: 0);
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var result = await _handler.Handle(inventory.Id);

            Assert.True(result);
            _repository.Verify(r => r.DeleteAsync(inventory), Times.Once);
        }
    }
}
