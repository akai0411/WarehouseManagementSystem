using FluentValidation;
using FluentValidation.Results;
using Moq;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.CreateWarehouse;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Warehouses
{
    public class CreateWarehouseHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repository = new();
        private readonly Mock<IValidator<CreateWarehouseCommand>> _validator = new();

        private CreateWarehouseHandler CreateHandler() =>
            new(_repository.Object, _validator.Object);

        private static CreateWarehouseCommand ValidCommand() => new()
        {
            Name = "Test Warehouse",
            Description = "Test Description",
            StreetType = "Street",
            StreetName = "Test Street",
            Number = "123",
            PostalCode = "12345",
            City = "Test City",
            Country = "Test Country"
        };

        private void SetupValidationResult(CreateWarehouseCommand command, bool isValid)
        {
            var result = isValid
                ? new ValidationResult()
                : new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") });

            _validator.Setup(v => v.ValidateAsync(command, default)).ReturnsAsync(result);
        }

        // Explicit, rather than relying on Moq's implicit "unconfigured
        // Task<T> methods return a completed task with a default result"
        // behavior — spelling it out here means the test's assumptions are
        // visible and don't depend on framework defaults.
        private void SetupNoExistingWarehouse()
        {
            _repository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((Warehouse?)null);
            _repository.Setup(r => r.GetByAddressAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Warehouse?)null);
            // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
            // to any entity whose Id is still Guid.Empty when persisted.
            _repository.Setup(r => r.AddAsync(It.IsAny<Warehouse>()))
                .Callback<Warehouse>(w => w.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewWarehouseId()
        {
            var command = ValidCommand();
            SetupValidationResult(command, isValid: true);
            SetupNoExistingWarehouse();

            var result = await CreateHandler().Handle(command);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
        {
            var command = ValidCommand();
            SetupValidationResult(command, isValid: true);
            SetupNoExistingWarehouse();

            await CreateHandler().Handle(command);

            _repository.Verify(r => r.AddAsync(It.IsAny<Warehouse>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationException()
        {
            var command = ValidCommand();
            command.Name = ""; // invalid
            SetupValidationResult(command, isValid: false);

            var handler = CreateHandler();

            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_InvalidCommand_DoesNotCallRepositoryAddAsync()
        {
            var command = ValidCommand();
            command.Name = ""; // invalid
            SetupValidationResult(command, isValid: false);

            var handler = CreateHandler();

            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command));
            _repository.Verify(r => r.AddAsync(It.IsAny<Warehouse>()), Times.Never);
        }
    }
}
