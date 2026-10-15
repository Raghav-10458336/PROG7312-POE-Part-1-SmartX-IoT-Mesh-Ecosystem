
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
        private readonly TelemetryQueueService _queueService;

        public TelemetryController(
            TelemetryIngestionService telemetryService,
            TelemetryQueueService queueService)
        {
            _telemetryService = telemetryService;
            _queueService = queueService;
        }

        [HttpPost("float")]
        public IActionResult IngestFloat(
            [FromBody] TelemetryPacket<float> packet)
        {
            // Queue incoming float telemetry for background processing.
            _queueService.EnqueueFloat(packet);

            return Accepted(new
            {
                message = "Float telemetry packet queued.",
                packet
            });
        }

        [HttpPost("integer")]
        public IActionResult IngestInteger(
            [FromBody] TelemetryPacket<int> packet)
        {
            // Queue incoming integer telemetry for background processing.
            _queueService.EnqueueInteger(packet);

            return Accepted(new
            {
                message = "Integer telemetry packet queued.",
                packet
            });
        }

        [HttpPost("boolean")]
        public IActionResult IngestBoolean(
            [FromBody] TelemetryPacket<bool> packet)
        {
            // Queue incoming boolean telemetry for background processing.
            _queueService.EnqueueBoolean(packet);

            return Accepted(new
            {
                message = "Boolean telemetry packet queued.",
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
