
using FluentValidation;

namespace WarehouseManagementSystem.Application.Features.Products.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(p => p.Name).ValidName();
            RuleFor(p => p.Price).ValidPrice();
            RuleFor(p => p.Description).ValidDescription();
        }
    }
}
