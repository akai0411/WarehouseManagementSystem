using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Locations.UpdateLocation;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Locations
{
    public class UpdateLocationHandlerTests
    {
        private readonly Mock<ILocationRepository> _repository = new();
        private readonly UpdateLocationCommandValidator _validator = new();

        private UpdateLocationHandler CreateHandler() => new(_repository.Object, _validator);

        private static UpdateLocationCommand ValidCommand() => new()
        {
            Row = "B",
            Shelf = 2,
            Bin = 3
        };

        [Fact]
        public async Task Handle_LocationNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Location?)null);

            var result = await CreateHandler().Handle(Guid.NewGuid(), Guid.NewGuid(), ValidCommand());

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_LocationBelongsToDifferentZone_ReturnsFalse()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);

            var result = await CreateHandler().Handle(Guid.NewGuid(), location.Id, ValidCommand());

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_DuplicateCodeOnDifferentLocation_ThrowsConflictException()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            var otherLocationWithSameCode = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _repository.Setup(r => r.GetByCodeAndZoneAsync("B-02-03", zone.Id))
                .ReturnsAsync(otherLocationWithSameCode);

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(zone.Id, location.Id, ValidCommand()));
            _repository.Verify(r => r.UpdateAsync(It.IsAny<Location>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CodeUnchangedOnSameLocation_DoesNotThrowConflict()
        {
            // GetByCodeAndZoneAsync will legitimately find the location
            // being updated itself when the code is unchanged — must not
            // be treated as a conflict with itself.
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _repository.Setup(r => r.GetByCodeAndZoneAsync("B-02-03", zone.Id))
                .ReturnsAsync(location); // same location

            var result = await CreateHandler().Handle(zone.Id, location.Id, ValidCommand());

            Assert.True(result);
            _repository.Verify(r => r.UpdateAsync(location), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesLocationFieldsAndReturnsTrue()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _repository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _repository.Setup(r => r.GetByCodeAndZoneAsync("B-02-03", zone.Id))
                .ReturnsAsync((Location?)null);

            var result = await CreateHandler().Handle(zone.Id, location.Id, ValidCommand());

            Assert.True(result);
            Assert.Equal("B", location.Row);
            Assert.Equal(2, location.Shelf);
            Assert.Equal(3, location.Bin);
            Assert.Equal("B-02-03", location.Code);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationExceptionBeforeTouchingRepository()
        {
            var command = new UpdateLocationCommand { Row = "lowercase", Shelf = 1, Bin = 1 };

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler().Handle(Guid.NewGuid(), Guid.NewGuid(), command));
            _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
