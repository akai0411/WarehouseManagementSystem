using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Warehouses
{
    public class UpdateWarehouseHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repository = new();
        private readonly UpdateWarehouseCommandValidator _validator = new();

        private UpdateWarehouseHandler CreateHandler() => new(_repository.Object, _validator);

        private static UpdateWarehouseCommand ValidCommand() => new()
        {
            Name = "Renamed Warehouse",
            Description = "Updated description",
            StreetType = "Avenue",
            StreetName = "New Street",
            Number = "42",
            PostalCode = "08001",
            City = "Barcelona",
            Country = "Spain"
        };

        private void SetupNoConflicts(Guid warehouseId, UpdateWarehouseCommand command)
        {
            _repository.Setup(r => r.GetByNameAsync(command.Name)).ReturnsAsync((Warehouse?)null);
            _repository.Setup(r => r.GetByAddressAsync(
                    command.StreetName, command.Number, command.City, command.Country))
                .ReturnsAsync((Warehouse?)null);
        }

        [Fact]
        public async Task Handle_WarehouseNotFound_ReturnsFalse()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Warehouse?)null);

            var result = await CreateHandler().Handle(Guid.NewGuid(), ValidCommand());

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_DuplicateNameOnDifferentWarehouse_ThrowsConflictException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var otherWarehouseWithSameName = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAsync(command.Name))
                .ReturnsAsync(otherWarehouseWithSameName);

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(warehouse.Id, command));
            _repository.Verify(r => r.UpdateAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NameUnchangedOnSameWarehouse_DoesNotThrowConflict()
        {
            // GetByNameAsync will legitimately find the warehouse being
            // updated itself (name unchanged) — that must not be treated as
            // a conflict with itself.
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAsync(command.Name)).ReturnsAsync(warehouse);
            _repository.Setup(r => r.GetByAddressAsync(
                    command.StreetName, command.Number, command.City, command.Country))
                .ReturnsAsync((Warehouse?)null);

            var result = await CreateHandler().Handle(warehouse.Id, command);

            Assert.True(result);
            _repository.Verify(r => r.UpdateAsync(warehouse), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateAddressOnDifferentWarehouse_ThrowsConflictException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var otherWarehouseAtSameAddress = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var command = ValidCommand();
            _repository.Setup(r => r.GetByNameAsync(command.Name)).ReturnsAsync((Warehouse?)null);
            _repository.Setup(r => r.GetByAddressAsync(
                    command.StreetName, command.Number, command.City, command.Country))
                .ReturnsAsync(otherWarehouseAtSameAddress);

            await Assert.ThrowsAsync<ConflictException>(
                () => CreateHandler().Handle(warehouse.Id, command));
            _repository.Verify(r => r.UpdateAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesWarehouseFieldsAndReturnsTrue()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var command = ValidCommand();
            SetupNoConflicts(warehouse.Id, command);

            var result = await CreateHandler().Handle(warehouse.Id, command);

            Assert.True(result);
            Assert.Equal("Renamed Warehouse", warehouse.Name);
            Assert.Equal("Barcelona", warehouse.Address.City);
            _repository.Verify(r => r.UpdateAsync(warehouse), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationExceptionBeforeTouchingRepository()
        {
            var command = ValidCommand();
            command.Name = ""; // triggers the real NotEmpty rule

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler().Handle(Guid.NewGuid(), command));
            _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
