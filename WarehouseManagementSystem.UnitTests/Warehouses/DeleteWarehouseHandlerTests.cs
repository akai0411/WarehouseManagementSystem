using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.DeleteWarehouse;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Warehouses
{
    public class DeleteWarehouseHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repository = new();
        private readonly DeleteWarehouseHandler _handler;

        public DeleteWarehouseHandlerTests()
        {
            _handler = new DeleteWarehouseHandler(_repository.Object);
        }

        [Fact]
        public async Task Handle_WarehouseNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Warehouse?)null);

            var result = await _handler.Handle(Guid.NewGuid());

            Assert.False(result);
            _repository.Verify(r => r.DeleteAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WarehouseHasActiveZones_ThrowsBusinessRuleExceptionAndDoesNotDelete()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _repository.Setup(r => r.HasActiveZonesAsync(warehouse.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(warehouse.Id));

            _repository.Verify(r => r.DeleteAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WarehouseHasNoActiveZones_DeletesAndReturnsTrue()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _repository.Setup(r => r.HasActiveZonesAsync(warehouse.Id)).ReturnsAsync(false);

            var result = await _handler.Handle(warehouse.Id);

            Assert.True(result);
            _repository.Verify(r => r.DeleteAsync(warehouse), Times.Once);
        }
    }
}
