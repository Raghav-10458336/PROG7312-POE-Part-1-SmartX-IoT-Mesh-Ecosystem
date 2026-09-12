using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly SensorRegistryService _sensorRegistry;

        public SensorsController(SensorRegistryService sensorRegistry)
        {
            _sensorRegistry = sensorRegistry;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<SensorRegistration>> GetAll()
        {
            return Ok(_sensorRegistry.GetAll());
        }

        [HttpGet("{id:guid}")]
        public ActionResult<SensorRegistration> GetById(Guid id)
        {
            SensorRegistration? sensor = _sensorRegistry.GetById(id);

            if (sensor is null)
            {
                return NotFound();
            }

            return Ok(sensor);
        }

        [HttpPost]
        public ActionResult<SensorRegistration> Register(
            [FromBody] SensorRegistration sensor)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            SensorRegistration? existingSensor =
                _sensorRegistry.GetByDeviceIdentifier(sensor.DeviceIdentifier);

            if (existingSensor is not null)
            {
                return Conflict(new
                {
                    message = "A sensor with this device identifier already exists."
                });
            }

            SensorRegistration registeredSensor =
                _sensorRegistry.Register(sensor);

            return CreatedAtAction(
                nameof(GetById),
                new { id = registeredSensor.Id },
                registeredSensor);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            bool removed = _sensorRegistry.Remove(id);

            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
