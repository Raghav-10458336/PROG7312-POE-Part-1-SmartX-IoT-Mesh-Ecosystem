using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly TelemetryAlertService _alertService;

        public AlertsController(
            TelemetryAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TelemetryAlert>> GetAll()
        {
            return Ok(_alertService.GetAll());
        }

        [HttpGet("count")]
        public IActionResult GetCount()
        {
            return Ok(new
            {
                totalAlerts =
                    _alertService.GetAlertCount(),

                unacknowledgedAlerts =
                    _alertService.GetUnacknowledgedCount()
            });
        }

        [HttpPost("{alertId:guid}/acknowledge")]
        public IActionResult Acknowledge(
            Guid alertId)
        {
            bool acknowledged =
                _alertService.Acknowledge(alertId);

            if (!acknowledged)
            {
                return NotFound(new
                {
                    message =
                        "The requested telemetry alert was not found."
                });
            }

            TelemetryAlert? alert =
                _alertService.GetById(alertId);

            return Ok(alert);
        }
    }
}