using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Zones.DeleteZone;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Zones
{
    public class DeleteZoneHandlerTests
    {
        private readonly Mock<IZoneRepository> _repository = new();
        private readonly DeleteZoneHandler _handler;

        public DeleteZoneHandlerTests()
        {
            _handler = new DeleteZoneHandler(_repository.Object);
        }

        [Fact]
        public async Task Handle_ZoneNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var result = await _handler.Handle(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
            _repository.Verify(r => r.DeleteAsync(It.IsAny<Zone>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ZoneBelongsToDifferentWarehouse_ReturnsFalse()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var result = await _handler.Handle(Guid.NewGuid(), zone.Id); // wrong warehouseId

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ZoneHasActiveLocations_ThrowsBusinessRuleExceptionAndDoesNotDelete()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _repository.Setup(r => r.HasActiveLocationsAsync(zone.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(warehouse.Id, zone.Id));
            _repository.Verify(r => r.DeleteAsync(It.IsAny<Zone>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ZoneHasNoActiveLocations_DeletesAndReturnsTrue()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _repository.Setup(r => r.HasActiveLocationsAsync(zone.Id)).ReturnsAsync(false);

            var result = await _handler.Handle(warehouse.Id, zone.Id);

            Assert.True(result);
            _repository.Verify(r => r.DeleteAsync(zone), Times.Once);
        }
    }
}
