using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Zones.UpdateZone;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Zones
{
    public class UpdateZoneHandlerTests
    {
        private readonly Mock<IZoneRepository> _repository = new();
        private readonly UpdateZoneCommandValidator _validator = new();

        private UpdateZoneHandler CreateHandler() => new(_repository.Object, _validator);

        private static UpdateZoneCommand ValidCommand() => new()
        {
            Name = "Renamed Zone",
            Type = nameof(ZoneType.Refrigerated)
        };

        [Fact]
        public async Task Handle_ZoneNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var result = await CreateHandler().Handle(Guid.NewGuid(), Guid.NewGuid(), ValidCommand());

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ZoneBelongsToDifferentWarehouse_ReturnsFalse()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var result = await CreateHandler().Handle(Guid.NewGuid(), zone.Id, ValidCommand());

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_DuplicateNameOnDifferentZone_ThrowsConflictException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            var otherZoneWithSameName = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAndWarehouseAsync(command.Name, warehouse.Id))
                .ReturnsAsync(otherZoneWithSameName);

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(warehouse.Id, zone.Id, command));
            _repository.Verify(r => r.UpdateAsync(It.IsAny<Zone>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NameUnchangedOnSameZone_DoesNotThrowConflict()
        {
            // GetByNameAndWarehouseAsync will legitimately find the zone
            // being updated itself (name unchanged) — that must not be
            // treated as a conflict with itself.
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAndWarehouseAsync(command.Name, warehouse.Id))
                .ReturnsAsync(zone); // same zone

            var result = await CreateHandler().Handle(warehouse.Id, zone.Id, command);

            Assert.True(result);
            _repository.Verify(r => r.UpdateAsync(zone), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesZoneFieldsAndReturnsTrue()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _repository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAndWarehouseAsync(command.Name, warehouse.Id))
                .ReturnsAsync((Zone?)null);

            var result = await CreateHandler().Handle(warehouse.Id, zone.Id, command);

            Assert.True(result);
            Assert.Equal("Renamed Zone", zone.Name);
            Assert.Equal(ZoneType.Refrigerated, zone.Type);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationExceptionBeforeTouchingRepository()
        {
            var command = new UpdateZoneCommand { Name = "", Type = nameof(ZoneType.Dry) };

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler().Handle(Guid.NewGuid(), Guid.NewGuid(), command));
            _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
