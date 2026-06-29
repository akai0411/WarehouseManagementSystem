using FluentValidation;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Features.Zones.CreateZone
{
    public class CreateZoneCommandValidator : AbstractValidator<CreateZoneCommand>
    {
        private static readonly string[] ValidTypes = Enum.GetNames(typeof(ZoneType));

        public CreateZoneCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Zone name is required.")
                .MaximumLength(200).WithMessage("Zone name cannot exceed 200 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Zone type is required.")
                .Must(t => ValidTypes.Contains(t))
                .WithMessage($"Zone type must be one of: {string.Join(", ", ValidTypes)}.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        }
    }
}