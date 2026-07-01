using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Warehouses.Common;
using WarehouseManagementSystem.Application.Features.Warehouses.CreateWarehouse;
using WarehouseManagementSystem.Application.Features.Warehouses.DeleteWarehouse;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouseById;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses;
using WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing warehouses.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WarehousesController : ControllerBase
    {
        private readonly CreateWarehouseHandler _createHandler;
        private readonly GetWarehousesHandler _getHandler;
        private readonly GetWarehouseByIdHandler _getByIdHandler;
        private readonly UpdateWarehouseHandler _updateHandler;
        private readonly DeleteWarehouseHandler _deleteHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WarehousesController"/> class.
        /// </summary>
        public WarehousesController(
            CreateWarehouseHandler createHandler,
            GetWarehousesHandler getHandler,
            GetWarehouseByIdHandler getByIdHandler,
            UpdateWarehouseHandler updateHandler,
            DeleteWarehouseHandler deleteHandler)
        {
            _createHandler = createHandler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        /// <summary>
        /// Creates a new warehouse. Only the "Admin" role can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command)
        {
            var result = await _createHandler.Handle(command);
            return Ok(ApiResponse<Guid>.Ok(result, "Warehouse created successfully"));
        }

        /// <summary>
        /// Retrieves a paginated list of warehouses. Only users with the "Admin" or "RegionalManager" roles can access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<WarehouseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> GetWarehouses([FromQuery] GetWarehousesQuery query)
        {
            var result = await _getHandler.Handle(query);
            return Ok(ApiResponse<PagedResponse<WarehouseDto>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a warehouse by its unique identifier. Only users with the "Admin", "RegionalManager", or "WarehouseManager" roles can access this endpoint.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager,WarehouseManager")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getByIdHandler.Handle(id);

            if (result == null)
                return NotFound(ApiResponse<WarehouseDto>.Fail("Warehouse not found"));

            return Ok(ApiResponse<WarehouseDto>.Ok(result));
        }

        /// <summary>
        /// Updates an existing warehouse. Only the "Admin" role can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseCommand command)
        {
            var result = await _updateHandler.Handle(id, command);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Warehouse not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Warehouse updated successfully"));
        }

        /// <summary>
        /// Soft deletes a warehouse. Only the "Admin" role can perform this action.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteHandler.Handle(id);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Warehouse not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Warehouse deleted successfully"));
        }
    }
}