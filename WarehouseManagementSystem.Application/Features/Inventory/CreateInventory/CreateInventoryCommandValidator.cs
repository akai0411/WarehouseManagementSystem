using FluentValidation;

namespace WarehouseManagementSystem.Application.Features.Inventory.CreateInventory
{
    public class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
    {
        public CreateInventoryCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.LocationId)
                .NotEmpty().WithMessage("LocationId is required.");

            RuleFor(x => x.Quantity)
                .Equal(0).WithMessage(
                    "Initial quantity must be 0. Use stock movements to add stock.");

            RuleFor(x => x.MinimumStockLevel)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum stock level cannot be negative.");
        }
    }
}