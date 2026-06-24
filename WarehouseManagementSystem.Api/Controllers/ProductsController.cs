using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Products.Common;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Application.Features.Products.DeleteProduct;
using WarehouseManagementSystem.Application.Features.Products.GetProductById;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing products in the warehouse system.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductHandler _handler;
        private readonly GetProductsHandler _getHandler;
        private readonly GetProductByIdHandler _getByIdHandler;
        private readonly UpdateProductHandler _updateHandler;
        private readonly DeleteProductHandler _deleteHandler;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        public ProductsController(
            CreateProductHandler handler,
            GetProductsHandler getHandler,
            GetProductByIdHandler getByIdHandler,
            UpdateProductHandler updateHandler,
            DeleteProductHandler deleteHandler)
        {
            _handler = handler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        /// <summary>
        /// Creates a new product in the system.
        /// </summary>
        /// <param name="command">Product creation data</param>
        /// <returns>The created product</returns>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _handler.Handle(command);

            return Ok(ApiResponse<Guid>.Ok(result, "Product created successfully"));
        }

        /// <summary>
        /// Retrieves a paginated list of products with optional filtering.
        /// </summary>
        /// <param name="query">Filtering and pagination parameters</param>
        /// <returns>Paginated list of products</returns>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProductDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
        {
            var products = await _getHandler.Handle(query);

            return Ok(ApiResponse<PagedResponse<ProductDto>>.Ok(products));
        }

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>The requested product</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _getByIdHandler.Handle(id);

            if (product == null)
                return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));

            return Ok(ApiResponse<ProductDto>.Ok(product));
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <param name="command">Updated product data</param>
        /// <returns>Update result</returns>
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Update(Guid id, UpdateProductCommand command)
        {
            var result = await _updateHandler.Handle(id, command);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Product not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Product updated successfully"));
        }

        /// <summary>
        /// Deletes a product from the system.
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>Deletion result</returns>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteHandler.Handle(id);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Product not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Product deleted successfully"));
        }
    }
}