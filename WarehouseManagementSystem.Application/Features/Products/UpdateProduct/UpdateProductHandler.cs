using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagementSystem.Application.Features.Products.UpdateProduct
{
    public class UpdateProductHandler
    {
        private readonly IProductRepository _repository;
        private readonly IValidator<UpdateProductCommand> _validator;

        public UpdateProductHandler(IProductRepository repository, IValidator<UpdateProductCommand> validator)
        {  
            _repository = repository;
            _validator = validator;
        }
        
        public async Task<bool> Handle(Guid id, UpdateProductCommand request)
        {
            #region Validation

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            #endregion
            var product = await _repository.GetByIdAsync(id);

            if (product == null) return false;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;

            try
            {
                await _repository.UpdateAsync(product);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "This product was modified by another user. Please refresh and try again.");
            }

            return true;
        }

    }
}
