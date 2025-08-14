using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using RfidAppApi.Data;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// RFID management controller for handling RFID tag operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RfidController : ControllerBase
    {
        private readonly IRfidService _rfidService;
        private readonly IClientService _clientService;

        public RfidController(IRfidService rfidService, IClientService clientService)
        {
            _rfidService = rfidService;
            _clientService = clientService;
        }

        /// <summary>
        /// Get all RFID tags for the authenticated client
        /// </summary>
        /// <returns>List of all RFID tags for the client</returns>
        /// <response code="200">RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetAllRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetRfidsByClientAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get RFID tag by code for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <returns>RFID tag information</returns>
        /// <response code="200">RFID tag found</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{rfidCode}")]
        [ProducesResponseType(typeof(RfidDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> GetRfidByCode(string rfidCode)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfid = await _rfidService.GetRfidByCodeAsync(rfidCode, clientCode);
                if (rfid == null)
                {
                    return NotFound(new { message = $"RFID tag with code {rfidCode} not found." });
                }

                return Ok(rfid);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new RFID tag for the authenticated client
        /// </summary>
        /// <param name="createRfidDto">RFID tag creation details</param>
        /// <returns>Created RFID tag information</returns>
        /// <response code="201">RFID tag created successfully</response>
        /// <response code="400">Invalid input or client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(RfidDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> CreateRfid([FromBody] CreateRfidDto createRfidDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                // Set the client code from the token
                createRfidDto.ClientCode = clientCode;

                var createdRfid = await _rfidService.CreateRfidAsync(createRfidDto);
                return CreatedAtAction(nameof(GetRfidByCode), new { rfidCode = createdRfid.RFIDCode }, createdRfid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing RFID tag for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <param name="updateRfidDto">Updated RFID tag information</param>
        /// <returns>Updated RFID tag information</returns>
        /// <response code="200">RFID tag updated successfully</response>
        /// <response code="400">Invalid input or client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{rfidCode}")]
        [ProducesResponseType(typeof(RfidDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> UpdateRfid(string rfidCode, [FromBody] UpdateRfidDto updateRfidDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var updatedRfid = await _rfidService.UpdateRfidAsync(rfidCode, clientCode, updateRfidDto);
                return Ok(updatedRfid);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an RFID tag for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <returns>No content</returns>
        /// <response code="204">RFID tag deleted successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{rfidCode}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteRfid(string rfidCode)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                await _rfidService.DeleteRfidAsync(rfidCode, clientCode);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available RFID tags (not assigned to any product) for the authenticated client
        /// </summary>
        /// <returns>List of available RFID tags</returns>
        /// <response code="200">Available RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetAvailableRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetAvailableRfidsAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving available RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active RFID tags for the authenticated client
        /// </summary>
        /// <returns>List of active RFID tags</returns>
        /// <response code="200">Active RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetActiveRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetActiveRfidsAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get RFID count for the authenticated client
        /// </summary>
        /// <returns>Total count of RFID tags</returns>
        /// <response code="200">RFID count retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("count")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<int>> GetRfidCount()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var count = await _rfidService.GetRfidCountByClientAsync(clientCode);
                return Ok(new { clientCode, count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving RFID count.", error = ex.Message });
            }
        }

        private string GetClientCodeFromToken()
        {
            var clientCodeClaim = User.Claims.FirstOrDefault(c => c.Type == "ClientCode");
            return clientCodeClaim?.Value ?? string.Empty;
        }
    }
} 