using FluentValidation;
namespace WarehouseManagementSystem.Application.Features.Products.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator() 
        {
            RuleFor(p => p.Name).ValidName();
            RuleFor(p => p.SKU).ValidSku();
            RuleFor(p => p.Price).ValidPrice();
            RuleFor(p => p.Description).ValidDescription();  
        }
    }
}
