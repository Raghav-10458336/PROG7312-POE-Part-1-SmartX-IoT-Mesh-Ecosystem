using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryIngestionService _telemetryService;

        public TelemetryController(TelemetryIngestionService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        [HttpPost("float")]
        public IActionResult IngestFloat(
            [FromBody] TelemetryPacket<float> packet)
        {
            _telemetryService.IngestFloat(packet);

            return Accepted(new
            {
                message = "Float telemetry packet accepted.",
                packet
            });
        }

        [HttpPost("integer")]
        public IActionResult IngestInteger(
            [FromBody] TelemetryPacket<int> packet)
        {
            _telemetryService.IngestInteger(packet);

            return Accepted(new
            {
                message = "Integer telemetry packet accepted.",
                packet
            });
        }

        [HttpPost("boolean")]
        public IActionResult IngestBoolean(
            [FromBody] TelemetryPacket<bool> packet)
        {
            _telemetryService.IngestBoolean(packet);

            return Accepted(new
            {
                message = "Boolean telemetry packet accepted.",
                packet
            });
        }

        [HttpGet("float")]
        public ActionResult<IReadOnlyList<TelemetryPacket<float>>> GetFloatTelemetry()
        {
            return Ok(_telemetryService.GetFloatTelemetry());
        }

        [HttpGet("integer")]
        public ActionResult<IReadOnlyList<TelemetryPacket<int>>> GetIntegerTelemetry()
        {
            return Ok(_telemetryService.GetIntegerTelemetry());
        }

        [HttpGet("boolean")]
        public ActionResult<IReadOnlyList<TelemetryPacket<bool>>> GetBooleanTelemetry()
        {
            return Ok(_telemetryService.GetBooleanTelemetry());
        }

        [HttpGet("count")]
        public IActionResult GetTotalCount()
        {
            return Ok(new
            {
                totalPackets = _telemetryService.GetTotalPacketCount()
            });
        }
    }
}