using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly HistoricalTelemetryService _historyService;
        public HistoryController(HistoricalTelemetryService historyService)
        {
            _historyService = historyService;
        }

        // Existing routes remain available for backwards compatibility.
        [HttpGet]
        public IActionResult GetAll() => Ok(_historyService.GetAll());

        [HttpGet("device/{deviceIdentifier}")]
        public IActionResult GetByDevice(string deviceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier))
                return BadRequest(new { message = "Device identifier is required." });
            return Ok(_historyService.GetByDevice(deviceIdentifier));
        }

        [HttpGet("range")]
        public IActionResult GetByTimeRange([FromQuery] DateTime startUtc, [FromQuery] DateTime endUtc)
        {
            if (startUtc == default || endUtc == default)
                return BadRequest(new { message = "Both startUtc and endUtc are required." });
            if (startUtc > endUtc)
                return BadRequest(new { message = "Start time cannot be after end time." });
            return Ok(_historyService.GetByTimeRange(startUtc, endUtc));
        }

        // New lightweight endpoint for the historical timeline UI.
        [HttpGet("recent")]
        public IActionResult GetRecent(
            [FromQuery] DateTime startUtc,
            [FromQuery] DateTime endUtc,
            [FromQuery] int limit = 50,
            [FromQuery] string? device = null,
            [FromQuery] string? metric = null)
        {
            if (startUtc == default || endUtc == default)
                return BadRequest(new { message = "Both startUtc and endUtc are required." });
            if (startUtc > endUtc)
                return BadRequest(new { message = "Start time cannot be after end time." });
            if (limit < 1 || limit > 100)
                return BadRequest(new { message = "Limit must be between 1 and 100." });

            return Ok(_historyService.GetRecent(startUtc, endUtc, limit, device, metric));
        }
    }
}
