using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Features.Auth.Register
{
    public class RegisterHandler
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<RegisterCommand> _validator;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterHandler(
            IUserRepository repository,
            IValidator<RegisterCommand> validator,
            IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _validator = validator;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(RegisterCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingUser = await _repository.GetByEmailAsync(command.Email);

            if (existingUser != null)
            {
                throw new ConflictException("A user with this email already exists.");
            }

            var user = UserMapper.ToEntity(command);

            user.PasswordHash = _passwordHasher.HashPassword(user, command.Password);

            await _repository.AddAsync(user);

            return user.Id;
        }
    }
}