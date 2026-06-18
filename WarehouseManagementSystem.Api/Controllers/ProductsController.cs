using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Application.Features.Products.GetProductById;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;
using WarehouseManagementSystem.Application.Features.Products.DeleteProduct;

namespace WarehouseManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductHandler _handler;
        private readonly GetProductsHandler _getHandler;
        private readonly GetProductByIdHandler _getByIdHandler;
        private readonly UpdateProductHandler _updateHandler;
        private readonly DeleteProductHandler _deleteHandler;
        public ProductsController(CreateProductHandler handler, GetProductsHandler getHandler, GetProductByIdHandler getByIdHandler,
            UpdateProductHandler updateHandler, DeleteProductHandler deleteHandler)
        {
            _handler = handler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _handler.Handle(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPagedAsync(int page, int pageSize)
        {
            var products = await _getHandler.Handle(page,pageSize);
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {

            var product = await
                _getByIdHandler.Handle(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateProductCommand command)
        {

            var result = await
                _updateHandler.Handle(id,command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {

            var result = await
                _deleteHandler.Handle(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
