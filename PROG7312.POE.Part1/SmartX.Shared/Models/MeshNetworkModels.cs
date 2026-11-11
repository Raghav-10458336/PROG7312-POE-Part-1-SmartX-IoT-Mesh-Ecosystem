
using System;
using System.Collections.Generic;

namespace SmartX.Shared.Models
{
    public enum MeshNodeType
    {
        Sensor,
        Gateway
    }

    public class MeshNode
    {
        public string Id { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public MeshNodeType Type { get; set; } = MeshNodeType.Sensor;

        public string DeploymentLocation { get; set; } = string.Empty;

        public bool IsOnline { get; set; } = true;

        // Normalised positions for network visualisation.
        public double X { get; set; }

        public double Y { get; set; }
    }

    public class MeshLink
    {
        public string SourceId { get; set; } = string.Empty;

        public string TargetId { get; set; } = string.Empty;

        // Lower weights represent more efficient simulated links.
        public double Weight { get; set; } = 1.0;

        public bool IsEnabled { get; set; } = true;
    }

    public class MeshNetworkSnapshot
    {
        public string GatewayId { get; set; } = string.Empty;

        public List<MeshNode> Nodes { get; set; } = new();

        public List<MeshLink> Links { get; set; } = new();

        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class MeshConnectivityResult
    {
        public string GatewayId { get; set; } = string.Empty;

        public List<string> ReachableNodeIds { get; set; } = new();

        public List<string> UnreachableNodeIds { get; set; } = new();

        public Dictionary<string, int> HopCounts { get; set; } = new();

        public Dictionary<string, string?> Parents { get; set; } = new();
    }

    public class MeshRouteResult
    {
        public string SourceId { get; set; } = string.Empty;

        public string GatewayId { get; set; } = string.Empty;

        public bool IsReachable { get; set; }

        public List<string> Path { get; set; } = new();

        public int HopCount { get; set; }

        public double TotalWeight { get; set; }
    }

    public class MeshMstResult
    {
        public string GatewayId { get; set; } = string.Empty;

        public List<MeshLink> SelectedLinks { get; set; } = new();

        public List<string> ConnectedNodeIds { get; set; } = new();

        public List<string> DisconnectedNodeIds { get; set; } = new();

        public double TotalWeight { get; set; }

        public bool SpansAllOnlineNodes { get; set; }
    }
}
