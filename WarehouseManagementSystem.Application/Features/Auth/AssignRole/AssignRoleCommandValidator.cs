using FluentValidation;

namespace WarehouseManagementSystem.Application.Features.Auth.AssignRole
{
    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        private static readonly string[] ValidRoles =
            { "Admin", "RegionalManager", "WarehouseManager", "Operator" };

        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(r => ValidRoles.Contains(r))
                .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");

            // WarehouseManager and Operator need a WarehouseId
            RuleFor(x => x.WarehouseId)
                .NotNull()
                .When(x => x.Role == "WarehouseManager" || x.Role == "Operator")
                .WithMessage("WarehouseId is required for WarehouseManager and Operator roles.");

            // RegionalManager needs a Country
            RuleFor(x => x.Country)
                .NotEmpty()
                .When(x => x.Role == "RegionalManager")
                .WithMessage("Country is required for RegionalManager role.");
        }
    }
}