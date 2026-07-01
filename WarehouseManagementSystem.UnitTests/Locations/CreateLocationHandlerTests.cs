using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Locations.CreateLocation;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Locations
{
    public class CreateLocationHandlerTests
    {
        private readonly Mock<ILocationRepository> _locationRepository = new();
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly CreateLocationCommandValidator _validator = new();

        private CreateLocationHandler CreateHandler() =>
            new(_locationRepository.Object, _zoneRepository.Object, _validator);

        [Fact]
        public async Task Handle_ValidCommand_CreatesLocationAndReturnsNewLocationId()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetByCodeAndZoneAsync("A-01-01", zone.Id))
                .ReturnsAsync((Location?)null);
            // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
            // to any entity whose Id is still Guid.Empty when persisted.
            _locationRepository.Setup(r => r.AddAsync(It.IsAny<Location>()))
                .Callback<Location>(l => l.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            var command = new CreateLocationCommand { Row = "A", Shelf = 1, Bin = 1 };

            var resultId = await CreateHandler().Handle(zone.Id, command);

            Assert.NotEqual(Guid.Empty, resultId);
            _locationRepository.Verify(r => r.AddAsync(It.Is<Location>(
                l => l.Code == "A-01-01" && l.ZoneId == zone.Id)), Times.Once);
        }

        [Fact]
        public async Task Handle_ZoneNotFound_ThrowsNotFoundException()
        {
            _zoneRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var command = new CreateLocationCommand { Row = "A", Shelf = 1, Bin = 1 };

            await Assert.ThrowsAsync<NotFoundException>(
                () => CreateHandler().Handle(Guid.NewGuid(), command));
        }

        [Fact]
        public async Task Handle_DuplicateCodeInZone_ThrowsConflictException()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var existingLocation = EntityFactory.CreateLocation(zone);
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetByCodeAndZoneAsync("A-01-01", zone.Id))
                .ReturnsAsync(existingLocation);

            var command = new CreateLocationCommand { Row = "A", Shelf = 1, Bin = 1 };

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(zone.Id, command));
            _locationRepository.Verify(r => r.AddAsync(It.IsAny<Location>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LowercaseRow_ThrowsValidationException()
        {
            // Row must be uppercase letters only per CreateLocationCommandValidator.
            var command = new CreateLocationCommand { Row = "a", Shelf = 1, Bin = 1 };

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler().Handle(Guid.NewGuid(), command));
            _zoneRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
