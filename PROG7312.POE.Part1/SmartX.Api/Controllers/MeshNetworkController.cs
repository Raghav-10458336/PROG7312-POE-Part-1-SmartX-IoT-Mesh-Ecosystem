
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/mesh")]
    public class MeshNetworkController : ControllerBase
    {
        private readonly MeshNetworkService _meshNetwork;

        public MeshNetworkController(MeshNetworkService meshNetwork)
        {
            _meshNetwork = meshNetwork;
        }

        // Retrieve the complete simulated mesh network.
        [HttpGet("topology")]
        public ActionResult<MeshNetworkSnapshot> GetTopology()
        {
            return Ok(_meshNetwork.GetSnapshot());
        }

        // Run BFS to identify reachable and disconnected sensors.
        [HttpGet("connectivity")]
        public ActionResult<MeshConnectivityResult> GetConnectivity()
        {
            return Ok(_meshNetwork.GetConnectivity());
        }

        // Calculate the minimum spanning tree using Prim's algorithm.
        [HttpGet("mst")]
        public ActionResult<MeshMstResult> GetMinimumSpanningTree()
        {
            return Ok(_meshNetwork.GetMinimumSpanningTree());
        }

        // Retrieve the route from a sensor to the gateway along the MST.
        [HttpGet("routes/{sensorId}")]
        public ActionResult<MeshRouteResult> GetRoute(string sensorId)
        {
            if (string.IsNullOrWhiteSpace(sensorId))
            {
                return BadRequest(new
                {
                    message = "A valid sensor identifier is required."
                });
            }

            var snapshot = _meshNetwork.GetSnapshot();

            var sensorExists = snapshot.Nodes.Any(node =>
                node.Type == MeshNodeType.Sensor &&
                string.Equals(
                    node.Id,
                    sensorId,
                    StringComparison.OrdinalIgnoreCase));

            if (!sensorExists)
            {
                return NotFound(new
                {
                    message = $"Sensor '{sensorId}' was not found."
                });
            }

            // Existing but disconnected sensors return IsReachable = false.
            return Ok(_meshNetwork.GetMstRoute(sensorId));
        }

        // Retrieve the lowest-weight route using Dijkstra's algorithm.
        [HttpGet("routes/{sensorId}/shortest")]
        public ActionResult<MeshRouteResult> GetShortestRoute(
            string sensorId)
        {
            if (string.IsNullOrWhiteSpace(sensorId))
            {
                return BadRequest(new
                {
                    message = "A valid sensor identifier is required."
                });
            }

            var snapshot = _meshNetwork.GetSnapshot();

            var sensorExists = snapshot.Nodes.Any(node =>
                node.Type == MeshNodeType.Sensor &&
                string.Equals(
                    node.Id,
                    sensorId,
                    StringComparison.OrdinalIgnoreCase));

            if (!sensorExists)
            {
                return NotFound(new
                {
                    message = $"Sensor '{sensorId}' was not found."
                });
            }

            // Existing but disconnected sensors return IsReachable = false.
            return Ok(_meshNetwork.GetShortestRoute(sensorId));
        }

        // Simulate a sensor or gateway going online or offline.
        [HttpPut("nodes/{nodeId}/status")]
        public IActionResult UpdateNodeStatus(
            string nodeId,
            [FromBody] MeshNodeStatusRequest? request)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return BadRequest(new
                {
                    message = "A valid node identifier is required."
                });
            }

            if (request?.IsOnline is null)
            {
                return BadRequest(new
                {
                    message = "The isOnline property must be provided."
                });
            }

            var updated = _meshNetwork.SetNodeOnline(
                nodeId,
                request.IsOnline.Value);

            if (!updated)
            {
                return NotFound(new
                {
                    message = $"Node '{nodeId}' was not found."
                });
            }

            return Ok(new
            {
                nodeId,
                isOnline = request.IsOnline.Value,
                message = "Node status updated successfully."
            });
        }

        // Simulate a wireless connection failing or recovering.
        [HttpPut("links/status")]
        public IActionResult UpdateLinkStatus(
            [FromBody] MeshLinkStatusRequest? request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.SourceId) ||
                string.IsNullOrWhiteSpace(request.TargetId) ||
                request.IsEnabled is null)
            {
                return BadRequest(new
                {
                    message =
                        "sourceId, targetId and isEnabled are required."
                });
            }

            var updated = _meshNetwork.SetLinkEnabled(
                request.SourceId,
                request.TargetId,
                request.IsEnabled.Value);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "The specified wireless link was not found."
                });
            }

            return Ok(new
            {
                request.SourceId,
                request.TargetId,
                isEnabled = request.IsEnabled.Value,
                message = "Wireless link status updated successfully."
            });
        }
    }

    public class MeshNodeStatusRequest
    {
        public bool? IsOnline { get; set; }
    }

    public class MeshLinkStatusRequest
    {
        public string SourceId { get; set; } = string.Empty;

        public string TargetId { get; set; } = string.Empty;

        public bool? IsEnabled { get; set; }
    }
}
