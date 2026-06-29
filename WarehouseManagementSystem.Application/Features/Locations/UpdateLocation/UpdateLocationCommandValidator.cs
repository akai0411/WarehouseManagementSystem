using FluentValidation;

namespace WarehouseManagementSystem.Application.Features.Locations.UpdateLocation
{
    public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
    {
        public UpdateLocationCommandValidator()
        {
            RuleFor(x => x.Row)
                .NotEmpty().WithMessage("Row is required.")
                .MaximumLength(10).WithMessage("Row cannot exceed 10 characters.")
                .Matches("^[A-Z]+$").WithMessage("Row must be uppercase letters only.");

            RuleFor(x => x.Shelf)
                .GreaterThan(0).WithMessage("Shelf must be greater than 0.");

            RuleFor(x => x.Bin)
                .GreaterThan(0).WithMessage("Bin must be greater than 0.");
        }
    }
}