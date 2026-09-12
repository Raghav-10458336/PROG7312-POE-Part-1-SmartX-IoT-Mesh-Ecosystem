using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;
using SmartX.Shared.Validation;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeploymentController : ControllerBase
    {
        [HttpPost("validate")]
        public IActionResult ValidateDeployment(
            [FromBody] DeploymentNode rootNode)
        {
            bool isValid = DeploymentValidator.ValidateNode(rootNode);

            return Ok(new
            {
                isValid,
                message = isValid
                    ? "Deployment hierarchy is valid."
                    : "Deployment hierarchy contains an invalid or unconfigured node."
            });
        }
    }
}