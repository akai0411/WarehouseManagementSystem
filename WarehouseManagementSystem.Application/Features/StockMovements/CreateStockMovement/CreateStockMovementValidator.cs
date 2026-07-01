using FluentValidation;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement
{
    public class CreateStockMovementCommandValidator
        : AbstractValidator<CreateStockMovementCommand>
    {
        private static readonly string[] ValidTypes =
            Enum.GetNames(typeof(MovementType));

        public CreateStockMovementCommandValidator()
        {
            RuleFor(x => x.InventoryId)
                .NotEmpty().WithMessage("InventoryId is required.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Movement type is required.")
                .Must(t => ValidTypes.Contains(t))
                .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Reference)
                .MaximumLength(100)
                .WithMessage("Reference cannot exceed 100 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}