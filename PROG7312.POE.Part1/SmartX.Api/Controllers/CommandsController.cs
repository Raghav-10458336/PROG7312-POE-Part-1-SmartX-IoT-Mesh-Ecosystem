
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly DeviceCommandService _commandService;

        public CommandsController(DeviceCommandService commandService)
        {
            _commandService = commandService;
        }

        [HttpPost]
        public IActionResult Execute([FromBody] CommandRequest request)
        {
            var command = _commandService.Execute(
                request.DeviceIdentifier,
                request.CommandType,
                request.NewState);

            if (command is null)
            {
                return BadRequest(new
                {
                    message = "Invalid command or inactive/unknown device."
                });
            }

            return Ok(command);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_commandService.GetHistory());
        }

        [HttpPost("undo")]
        public IActionResult UndoLast()
        {
            var command = _commandService.UndoLast();

            if (command is null)
            {
                return BadRequest(new
                {
                    message = "No commands available to undo."
                });
            }

            return Ok(command);
        }

        [HttpGet("state/{deviceIdentifier}/{commandType}")]
        public IActionResult GetState(
            string deviceIdentifier,
            string commandType)
        {
            return Ok(new
            {
                deviceIdentifier,
                commandType,
                isOn = _commandService.GetState(
                    deviceIdentifier, commandType)
            });
        }
    }

    public class CommandRequest
    {
        public string DeviceIdentifier { get; set; } = string.Empty;

        public string CommandType { get; set; } = string.Empty;

        public bool NewState { get; set; }
    }
}
