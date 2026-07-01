using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Inventory.Common;
using WarehouseManagementSystem.Application.Features.Inventory.CreateInventory;
using WarehouseManagementSystem.Application.Features.Inventory.DeleteInventory;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventory;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventoryById;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing inventory assignments.
    /// </summary>
    [ApiController]
    [Route("api/inventory")]
    [Produces("application/json")]
    public class InventoryController : ControllerBase
    {
        private readonly CreateInventoryHandler _createHandler;
        private readonly GetInventoryHandler _getHandler;
        private readonly GetInventoryByIdHandler _getByIdHandler;
        private readonly DeleteInventoryHandler _deleteHandler;

        public InventoryController(
            CreateInventoryHandler createHandler,
            GetInventoryHandler getHandler,
            GetInventoryByIdHandler getByIdHandler,
            DeleteInventoryHandler deleteHandler)
        {
            _createHandler = createHandler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _deleteHandler = deleteHandler;
        }

        /// <summary>
        /// Assigns a product to a location.
        /// </summary>
        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 409)]
        public async Task<IActionResult> Create([FromBody] CreateInventoryCommand command)
        {
            var result = await _createHandler.Handle(command);
            return Ok(ApiResponse<Guid>.Ok(result, "Product assigned to location successfully."));
        }

        /// <summary>
        /// Retrieves inventory with optional filters. Results are scoped by role.
        /// </summary>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<InventoryDto>>), 200)]
        public async Task<IActionResult> GetInventory([FromQuery] GetInventoryQuery query)
        {
            var result = await _getHandler.Handle(query);
            return Ok(ApiResponse<PagedResponse<InventoryDto>>.Ok(result));
        }

        /// <summary>
        /// Retrieves an inventory record by its unique identifier.
        /// </summary>
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getByIdHandler.Handle(id);

            if (result == null)
                return NotFound(ApiResponse<InventoryDto>.Fail("Inventory not found."));

            return Ok(ApiResponse<InventoryDto>.Ok(result));
        }

        /// <summary>
        /// Unassigns a product from a location. Only allowed if quantity is 0.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 422)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteHandler.Handle(id);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Inventory not found."));

            return Ok(ApiResponse<bool>.Ok(true, "Product unassigned from location successfully."));
        }
    }
}