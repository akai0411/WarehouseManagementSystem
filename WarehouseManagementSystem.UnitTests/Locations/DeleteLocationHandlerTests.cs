using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Locations.DeleteLocation;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Locations
{
    public class DeleteLocationHandlerTests
    {
        private readonly Mock<ILocationRepository> _repository = new();
        private readonly DeleteLocationHandler _handler;

        public DeleteLocationHandlerTests()
        {
            _handler = new DeleteLocationHandler(_repository.Object);
        }

        [Fact]
        public async Task Handle_LocationNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Location?)null);

            var result = await _handler.Handle(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
            _repository.Verify(r => r.DeleteAsync(It.IsAny<Location>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LocationBelongsToDifferentZone_ReturnsFalse()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);

            var result = await _handler.Handle(Guid.NewGuid(), location.Id); // wrong zoneId

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_LocationHasInventory_ThrowsBusinessRuleExceptionAndDoesNotDelete()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _repository.Setup(r => r.HasInventoryAsync(location.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(zone.Id, location.Id));
            _repository.Verify(r => r.DeleteAsync(It.IsAny<Location>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LocationHasNoInventory_DeletesAndReturnsTrue()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _repository.Setup(r => r.HasInventoryAsync(location.Id)).ReturnsAsync(false);

            var result = await _handler.Handle(zone.Id, location.Id);

            Assert.True(result);
            _repository.Verify(r => r.DeleteAsync(location), Times.Once);
        }
    }
}
