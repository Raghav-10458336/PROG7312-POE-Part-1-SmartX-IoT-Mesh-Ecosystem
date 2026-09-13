using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private const int MaximumTake = 1000;

        private readonly TelemetryIngestionService _telemetryService;

        public TelemetryController(
            TelemetryIngestionService telemetryService)
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
        public ActionResult<
            IReadOnlyList<TelemetryPacket<float>>>
            GetFloatTelemetry(
                [FromQuery] int? take = null)
        {
            IReadOnlyList<TelemetryPacket<float>> telemetry =
                _telemetryService.GetFloatTelemetry();

            if (!take.HasValue)
            {
                return Ok(telemetry);
            }

            if (!IsValidTake(take.Value))
            {
                return BadRequest(new
                {
                    message =
                        $"Take must be between 1 and {MaximumTake}."
                });
            }

            List<TelemetryPacket<float>> recentTelemetry =
                telemetry
                    .OrderByDescending(
                        packet => packet.TimestampUtc)
                    .Take(take.Value)
                    .ToList();

            return Ok(recentTelemetry);
        }

        [HttpGet("integer")]
        public ActionResult<
            IReadOnlyList<TelemetryPacket<int>>>
            GetIntegerTelemetry(
                [FromQuery] int? take = null)
        {
            IReadOnlyList<TelemetryPacket<int>> telemetry =
                _telemetryService.GetIntegerTelemetry();

            if (!take.HasValue)
            {
                return Ok(telemetry);
            }

            if (!IsValidTake(take.Value))
            {
                return BadRequest(new
                {
                    message =
                        $"Take must be between 1 and {MaximumTake}."
                });
            }

            List<TelemetryPacket<int>> recentTelemetry =
                telemetry
                    .OrderByDescending(
                        packet => packet.TimestampUtc)
                    .Take(take.Value)
                    .ToList();

            return Ok(recentTelemetry);
        }

        [HttpGet("boolean")]
        public ActionResult<
            IReadOnlyList<TelemetryPacket<bool>>>
            GetBooleanTelemetry(
                [FromQuery] int? take = null)
        {
            IReadOnlyList<TelemetryPacket<bool>> telemetry =
                _telemetryService.GetBooleanTelemetry();

            if (!take.HasValue)
            {
                return Ok(telemetry);
            }

            if (!IsValidTake(take.Value))
            {
                return BadRequest(new
                {
                    message =
                        $"Take must be between 1 and {MaximumTake}."
                });
            }

            List<TelemetryPacket<bool>> recentTelemetry =
                telemetry
                    .OrderByDescending(
                        packet => packet.TimestampUtc)
                    .Take(take.Value)
                    .ToList();

            return Ok(recentTelemetry);
        }

        [HttpGet("count")]
        public IActionResult GetTotalCount()
        {
            return Ok(new
            {
                totalPackets =
                    _telemetryService.GetTotalPacketCount()
            });
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            int floatPackets =
                _telemetryService
                    .GetFloatTelemetry()
                    .Count;

            int integerPackets =
                _telemetryService
                    .GetIntegerTelemetry()
                    .Count;

            int booleanPackets =
                _telemetryService
                    .GetBooleanTelemetry()
                    .Count;

            return Ok(new
            {
                totalPackets =
                    floatPackets +
                    integerPackets +
                    booleanPackets,

                floatPackets,

                integerPackets,

                booleanPackets
            });
        }

        private static bool IsValidTake(
            int take)
        {
            return take >= 1 &&
                   take <= MaximumTake;
        }
    }
}