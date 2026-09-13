using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/power-analysis")]
    public class PowerAnalysisController : ControllerBase
    {
        [HttpGet]
        public ActionResult<PowerAnalysisResult> Analyze(
            double meterA,
            double meterB)
        {
            PowerReading firstReading =
                new(meterA);

            PowerReading secondReading =
                new(meterB);

            PowerReading combinedReading =
                firstReading + secondReading;

            PowerReading differenceReading =
                firstReading > secondReading
                    ? firstReading - secondReading
                    : secondReading - firstReading;

            string highestLoadMeter;

            if (firstReading > secondReading)
            {
                highestLoadMeter = "Meter A";
            }
            else if (secondReading > firstReading)
            {
                highestLoadMeter = "Meter B";
            }
            else
            {
                highestLoadMeter = "Equal load";
            }

            PowerAnalysisResult result = new()
            {
                MeterA = firstReading.Watts,
                MeterB = secondReading.Watts,
                CombinedLoad = combinedReading.Watts,
                LoadDifference = differenceReading.Watts,
                HighestLoadMeter = highestLoadMeter
            };

            return Ok(result);
        }
    }
}
