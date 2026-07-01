using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.StockMovements.Common;
using WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovementById;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovements;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing stock movements.
    /// </summary>
    [ApiController]
    [Route("api/stockmovements")]
    [Produces("application/json")]
    public class StockMovementsController : ControllerBase
    {
        private readonly CreateStockMovementHandler _createHandler;
        private readonly GetStockMovementsHandler _getHandler;
        private readonly GetStockMovementByIdHandler _getByIdHandler;

        public StockMovementsController(
            CreateStockMovementHandler createHandler,
            GetStockMovementsHandler getHandler,
            GetStockMovementByIdHandler getByIdHandler)
        {
            _createHandler = createHandler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
        }

        /// <summary>
        /// Registers a stock movement. Inbound and Outbound for
        /// WarehouseManager and Operator. Adjustment for Admin and WarehouseManager only.
        /// </summary>
        [Authorize(Roles = "Admin,WarehouseManager,Operator")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateStockMovementResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 422)]
        public async Task<IActionResult> Create(
            [FromBody] CreateStockMovementCommand command)
        {
            var result = await _createHandler.Handle(command);
            return Ok(ApiResponse<CreateStockMovementResponse>.Ok(
                result, "Stock movement registered successfully."));
        }

        /// <summary>
        /// Retrieves stock movements. Results are scoped by role.
        /// </summary>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<StockMovementDto>>), 200)]
        public async Task<IActionResult> GetStockMovements(
            [FromQuery] GetStockMovementsQuery query)
        {
            var result = await _getHandler.Handle(query);
            return Ok(ApiResponse<PagedResponse<StockMovementDto>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a stock movement by its unique identifier.
        /// </summary>
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getByIdHandler.Handle(id);

            if (result == null)
                return NotFound(ApiResponse<StockMovementDto>.Fail(
                    "Stock movement not found."));

            return Ok(ApiResponse<StockMovementDto>.Ok(result));
        }
    }
}