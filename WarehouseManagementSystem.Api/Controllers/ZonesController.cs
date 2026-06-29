using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Zones.Common;
using WarehouseManagementSystem.Application.Features.Zones.CreateZone;
using WarehouseManagementSystem.Application.Features.Zones.DeleteZone;
using WarehouseManagementSystem.Application.Features.Zones.GetZoneById;
using WarehouseManagementSystem.Application.Features.Zones.GetZones;
using WarehouseManagementSystem.Application.Features.Zones.UpdateZone;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing zones within a warehouse.
    /// </summary>
    [ApiController]
    [Route("api/warehouses/{warehouseId:guid}/zones")]
    [Produces("application/json")]
    public class ZonesController : ControllerBase
    {
        private readonly CreateZoneHandler _createHandler;
        private readonly GetZonesHandler _getHandler;
        private readonly GetZoneByIdHandler _getByIdHandler;
        private readonly UpdateZoneHandler _updateHandler;
        private readonly DeleteZoneHandler _deleteHandler;

        public ZonesController(
            CreateZoneHandler createHandler,
            GetZonesHandler getHandler,
            GetZoneByIdHandler getByIdHandler,
            UpdateZoneHandler updateHandler,
            DeleteZoneHandler deleteHandler)
        {
            _createHandler = createHandler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        /// <summary>
        /// Creates a new zone in the specified warehouse.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> Create(
            Guid warehouseId,
            [FromBody] CreateZoneCommand command)
        {
            var result = await _createHandler.Handle(warehouseId, command);
            return Ok(ApiResponse<Guid>.Ok(result, "Zone created successfully."));
        }

        /// <summary>
        /// Retrieves all zones in the specified warehouse.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager,WarehouseManager,Operator")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ZoneDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetZones(
            Guid warehouseId,
            [FromQuery] GetZonesQuery query)
        {
            var result = await _getHandler.Handle(warehouseId, query);
            return Ok(ApiResponse<PagedResponse<ZoneDto>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a zone by its unique identifier.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager,WarehouseManager,Operator")]
        [HttpGet("{zoneId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ZoneDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid warehouseId, Guid zoneId)
        {
            var result = await _getByIdHandler.Handle(warehouseId, zoneId);

            if (result == null)
                return NotFound(ApiResponse<ZoneDto>.Fail("Zone not found."));

            return Ok(ApiResponse<ZoneDto>.Ok(result));
        }

        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{zoneId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Update(
            Guid warehouseId,
            Guid zoneId,
            [FromBody] UpdateZoneCommand command)
        {
            var result = await _updateHandler.Handle(warehouseId, zoneId, command);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Zone not found."));

            return Ok(ApiResponse<bool>.Ok(true, "Zone updated successfully."));
        }

        /// <summary>
        /// Soft deletes a zone.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{zoneId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 422)]
        public async Task<IActionResult> Delete(Guid warehouseId, Guid zoneId)
        {
            var result = await _deleteHandler.Handle(warehouseId, zoneId);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Zone not found."));

            return Ok(ApiResponse<bool>.Ok(true, "Zone deleted successfully."));
        }
    }
}