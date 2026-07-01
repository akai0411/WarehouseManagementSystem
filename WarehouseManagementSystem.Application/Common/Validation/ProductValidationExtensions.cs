using FluentValidation;
using WarehouseManagementSystem.Application.Common.Validation;

public static class ProductValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(100)
            .WithMessage(ValidationMessages.NameMaxLength);
    }

    public static IRuleBuilderOptions<T, string> ValidSku<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(ValidationMessages.SkuRequired)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.SkuMaxLength);
    }

    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage(ValidationMessages.PriceGreaterThanZero);
    }

    public static IRuleBuilderOptions<T, int> ValidQuantity<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationMessages.QuantityCannotBeNegative);
    }

    public static IRuleBuilderOptions<T, string> ValidDescription<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(500)
            .WithMessage(ValidationMessages.DescriptionMaxLength);
    }
}