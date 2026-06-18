using FluentValidation;
namespace WarehouseManagementSystem.Application.Features.Products.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0);
        }
    }
}
