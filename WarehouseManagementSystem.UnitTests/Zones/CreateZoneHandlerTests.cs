using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Zones.CreateZone;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Zones
{
    public class CreateZoneHandlerTests
    {
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();
        private readonly CreateZoneCommandValidator _validator = new();

        private CreateZoneHandler CreateHandler() =>
            new(_zoneRepository.Object, _warehouseRepository.Object, _validator);

        [Fact]
        public async Task Handle_ValidCommand_CreatesZoneAndReturnsNewZoneId()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetByNameAndWarehouseAsync("Receiving", warehouse.Id))
                .ReturnsAsync((Zone?)null);
            // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
            // to any entity whose Id is still Guid.Empty when persisted.
            _zoneRepository.Setup(r => r.AddAsync(It.IsAny<Zone>()))
                .Callback<Zone>(z => z.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            var command = new CreateZoneCommand
            {
                Name = "Receiving",
                Type = nameof(ZoneType.Dry)
            };

            var resultId = await CreateHandler().Handle(warehouse.Id, command);

            Assert.NotEqual(Guid.Empty, resultId);
            _zoneRepository.Verify(r => r.AddAsync(It.Is<Zone>(
                z => z.Name == "Receiving" && z.WarehouseId == warehouse.Id)), Times.Once);
        }

        [Fact]
        public async Task Handle_WarehouseNotFound_ThrowsNotFoundException()
        {
            _warehouseRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Warehouse?)null);

            var command = new CreateZoneCommand { Name = "Receiving", Type = nameof(ZoneType.Dry) };

            await Assert.ThrowsAsync<NotFoundException>(
                () => CreateHandler().Handle(Guid.NewGuid(), command));
        }

        [Fact]
        public async Task Handle_DuplicateNameInWarehouse_ThrowsConflictException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var existingZone = EntityFactory.CreateZone(warehouse);
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetByNameAndWarehouseAsync("Zone A", warehouse.Id))
                .ReturnsAsync(existingZone);

            var command = new CreateZoneCommand { Name = "Zone A", Type = nameof(ZoneType.Dry) };

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(warehouse.Id, command));
            _zoneRepository.Verify(r => r.AddAsync(It.IsAny<Zone>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidZoneType_ThrowsValidationException()
        {
            var command = new CreateZoneCommand { Name = "Receiving", Type = "NotARealType" };

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler().Handle(Guid.NewGuid(), command));
            _warehouseRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
