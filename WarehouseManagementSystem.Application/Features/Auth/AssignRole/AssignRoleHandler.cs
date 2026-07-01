using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Auth.AssignRole
{
    public class AssignRoleHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<AssignRoleCommand> _validator;

        public AssignRoleHandler(
            IUserRepository userRepository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser,
            IValidator<AssignRoleCommand> validator)
        {
            _userRepository = userRepository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
            _validator = validator;
        }

        public async Task Handle(AssignRoleCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // WarehouseManager can only assign Operator role
            // and only to their own warehouse
            if (_currentUser.IsWarehouseManager)
            {
                if (command.Role != "Operator")
                    throw new UnauthorizedException(
                        "WarehouseManagers can only assign the Operator role.");

                if (command.WarehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "WarehouseManagers can only assign operators to their own warehouse.");
            }

            var user = await _userRepository.GetByIdAsync(command.UserId)
                ?? throw new NotFoundException("User not found.");

            // Validate warehouse exists if provided
            if (command.WarehouseId.HasValue)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(command.WarehouseId.Value)
                    ?? throw new NotFoundException("Warehouse not found.");
            }

            user.Role = command.Role;
            user.WarehouseId = command.WarehouseId;
            user.Country = command.Country;

            await _userRepository.UpdateAsync(user);
        }
    }
}