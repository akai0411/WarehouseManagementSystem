using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Locations.Common;
using WarehouseManagementSystem.Application.Features.Locations.CreateLocation;
using WarehouseManagementSystem.Application.Features.Locations.DeleteLocation;
using WarehouseManagementSystem.Application.Features.Locations.GetLocationById;
using WarehouseManagementSystem.Application.Features.Locations.GetLocations;
using WarehouseManagementSystem.Application.Features.Locations.UpdateLocation;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing locations within a zone.
    /// </summary>
    [ApiController]
    [Route("api/zones/{zoneId:guid}/locations")]
    [Produces("application/json")]
    public class LocationsController : ControllerBase
    {
        private readonly CreateLocationHandler _createHandler;
        private readonly GetLocationsHandler _getHandler;
        private readonly GetLocationByIdHandler _getByIdHandler;
        private readonly UpdateLocationHandler _updateHandler;
        private readonly DeleteLocationHandler _deleteHandler;

        public LocationsController(
            CreateLocationHandler createHandler,
            GetLocationsHandler getHandler,
            GetLocationByIdHandler getByIdHandler,
            UpdateLocationHandler updateHandler,
            DeleteLocationHandler deleteHandler)
        {
            _createHandler = createHandler;
            _getHandler = getHandler;
            _getByIdHandler = getByIdHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        /// <summary>
        /// Creates a new location in the specified zone.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> Create(
            Guid zoneId,
            [FromBody] CreateLocationCommand command)
        {
            var result = await _createHandler.Handle(zoneId, command);
            return Ok(ApiResponse<Guid>.Ok(result, "Location created successfully."));
        }

        /// <summary>
        /// Retrieves all locations in the specified zone.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager,WarehouseManager,Operator")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<LocationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetLocations(
            Guid zoneId,
            [FromQuery] GetLocationsQuery query)
        {
            var result = await _getHandler.Handle(zoneId, query);
            return Ok(ApiResponse<PagedResponse<LocationDto>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a location by its unique identifier.
        /// </summary>
        [Authorize(Roles = "Admin,RegionalManager,WarehouseManager,Operator")]
        [HttpGet("{locationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid zoneId, Guid locationId)
        {
            var result = await _getByIdHandler.Handle(zoneId, locationId);

            if (result == null)
                return NotFound(ApiResponse<LocationDto>.Fail("Location not found."));

            return Ok(ApiResponse<LocationDto>.Ok(result));
        }

        /// <summary>
        /// Updates an existing location.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{locationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Update(
            Guid zoneId,
            Guid locationId,
            [FromBody] UpdateLocationCommand command)
        {
            var result = await _updateHandler.Handle(zoneId, locationId, command);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Location not found."));

            return Ok(ApiResponse<bool>.Ok(true, "Location updated successfully."));
        }

        /// <summary>
        /// Soft deletes a location.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{locationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 422)]
        public async Task<IActionResult> Delete(Guid zoneId, Guid locationId)
        {
            var result = await _deleteHandler.Handle(zoneId, locationId);

            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Location not found."));

            return Ok(ApiResponse<bool>.Ok(true, "Location deleted successfully."));
        }
    }
}