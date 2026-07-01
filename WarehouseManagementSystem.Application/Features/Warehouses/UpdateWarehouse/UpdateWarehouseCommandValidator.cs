using FluentValidation;

namespace WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse
{
    public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
    {
        public UpdateWarehouseCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required.")
                .MaximumLength(200).WithMessage("Warehouse name cannot exceed 200 characters.");

            RuleFor(x => x.StreetType)
                .NotEmpty().WithMessage("Street type is required.")
                .MaximumLength(50).WithMessage("Street type cannot exceed 50 characters.");

            RuleFor(x => x.StreetName)
                .NotEmpty().WithMessage("Street name is required.")
                .MaximumLength(200).WithMessage("Street name cannot exceed 200 characters.");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Street number is required.")
                .MaximumLength(10).WithMessage("Street number cannot exceed 10 characters.");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required.")
                .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");
        }
    }
}