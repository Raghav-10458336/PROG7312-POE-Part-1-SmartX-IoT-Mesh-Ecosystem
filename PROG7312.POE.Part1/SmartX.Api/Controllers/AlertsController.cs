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

        public AlertsController(TelemetryAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<TelemetryAlert>> GetAll()
        {
            return Ok(_alertService.GetAll());
        }

        [HttpGet("count")]
        public IActionResult GetCount()
        {
            return Ok(new
            {
                totalAlerts = _alertService.GetAlertCount()
            });
        }
    }
}
