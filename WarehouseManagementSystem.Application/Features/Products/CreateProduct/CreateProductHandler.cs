using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Domain.Entities;

public class CreateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(IProductRepository repository,IValidator<CreateProductCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateProductCommand request)
    {
        #region Validation

        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        #endregion 

        var existingProduct = await _repository.GetBySkuAsync(request.SKU);

        if (existingProduct != null)
        {
            throw new ConflictException(
                $"A product with SKU '{request.SKU}' already exists.");
        } 
        var product = ProductMapper.ToEntity(request);

        await _repository.AddAsync(product);

        return product.Id;
    }
}