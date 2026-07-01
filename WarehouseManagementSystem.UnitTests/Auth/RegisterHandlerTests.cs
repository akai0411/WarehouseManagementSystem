using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Auth.Register;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Auth
{
    public class RegisterHandlerTests
    {
        private readonly Mock<IUserRepository> _repository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();

        // Uses the real validator (same pattern as the other Create*HandlerTests)
        // so the actual FluentValidation rules (email format, password
        // complexity) are genuinely exercised.
        private readonly RegisterCommandValidator _validator = new();

        private RegisterHandler CreateHandler() =>
            new(_repository.Object, _validator, _passwordHasher.Object);

        private static RegisterCommand ValidCommand() => new()
        {
            Email = "new.user@warehouse.com",
            Password = "Str0ngPass"
        };

        [Fact]
        public async Task Handle_ValidCommand_RegistersUserAndReturnsNewUserId()
        {
            var command = ValidCommand();

            _repository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);
            _passwordHasher.Setup(h => h.HashPassword(It.IsAny<User>(), command.Password))
                .Returns("hashed-password");
            _repository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await CreateHandler().Handle(command);

            Assert.NotEqual(Guid.Empty, result);
            _repository.Verify(r => r.AddAsync(It.Is<User>(
                u => u.Email == command.Email && u.PasswordHash == "hashed-password")), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsConflictException()
        {
            var command = ValidCommand();

            _repository.Setup(r => r.GetByEmailAsync(command.Email))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = command.Email });

            await Assert.ThrowsAsync<ConflictException>(() => CreateHandler().Handle(command));

            _repository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidEmail_ThrowsValidationException()
        {
            var command = new RegisterCommand { Email = "not-an-email", Password = "Str0ngPass" };

            await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command));

            _repository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WeakPassword_ThrowsValidationException()
        {
            // Missing uppercase/number, and under 8 characters.
            var command = new RegisterCommand { Email = "new.user@warehouse.com", Password = "weak" };

            await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command));

            _repository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
