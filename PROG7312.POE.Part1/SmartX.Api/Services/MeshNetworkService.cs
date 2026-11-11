
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class MeshNetworkService
    {
        private readonly object _sync = new();
        private readonly Dictionary<string, MeshNode> _nodes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MeshLink> _links = new(StringComparer.OrdinalIgnoreCase);

        private string _gatewayId = string.Empty;

        // Register a gateway or sensor in the graph.
        public void AddOrUpdateNode(MeshNode node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (string.IsNullOrWhiteSpace(node.Id))
                throw new ArgumentException("Node identifier is required.");

            lock (_sync)
            {
                if (node.Type == MeshNodeType.Gateway)
                {
                    if (_gatewayId.Length > 0 &&
                        !string.Equals(_gatewayId, node.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("Only one gateway is supported.");
                    }

                    _gatewayId = node.Id;
                }
                else if (string.Equals(_gatewayId, node.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The gateway cannot be replaced by a sensor.");
                }

                _nodes[node.Id] = CopyNode(node);
            }
        }

        // Add an undirected, weighted wireless connection.
        public void AddOrUpdateLink(MeshLink link)
        {
            ArgumentNullException.ThrowIfNull(link);

            if (string.IsNullOrWhiteSpace(link.SourceId) ||
                string.IsNullOrWhiteSpace(link.TargetId))
                throw new ArgumentException("Both link endpoints are required.");

            if (string.Equals(link.SourceId, link.TargetId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Self-connections are not allowed.");

            if (!double.IsFinite(link.Weight) || link.Weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(link), "Weight must be positive and finite.");

            lock (_sync)
            {
                if (!_nodes.ContainsKey(link.SourceId) || !_nodes.ContainsKey(link.TargetId))
                    throw new InvalidOperationException("Both nodes must exist before connecting them.");

                _links[LinkKey(link.SourceId, link.TargetId)] = CopyLink(link);
            }
        }

        // Simulate device failure or recovery.
        public bool SetNodeOnline(string nodeId, bool isOnline)
        {
            lock (_sync)
            {
                if (!_nodes.TryGetValue(nodeId, out var node))
                    return false;

                node.IsOnline = isOnline;
                return true;
            }
        }

        // Simulate a wireless link failing or recovering.
        public bool SetLinkEnabled(string sourceId, string targetId, bool enabled)
        {
            lock (_sync)
            {
                if (!_links.TryGetValue(LinkKey(sourceId, targetId), out var link))
                    return false;

                link.IsEnabled = enabled;
                return true;
            }
        }

        public MeshNetworkSnapshot GetSnapshot()
        {
            lock (_sync)
            {
                return new MeshNetworkSnapshot
                {
                    GatewayId = _gatewayId,
                    Nodes = _nodes.Values.Select(CopyNode).ToList(),
                    Links = _links.Values.Select(CopyLink).ToList(),
                    GeneratedAtUtc = DateTime.UtcNow
                };
            }
        }

        // BFS discovers every online node reachable from the gateway.
        public MeshConnectivityResult GetConnectivity()
        {
            var snapshot = GetSnapshot();
            var adjacency = BuildAdjacency(snapshot);

            var result = new MeshConnectivityResult
            {
                GatewayId = snapshot.GatewayId
            };

            var online = snapshot.Nodes
                .Where(n => n.IsOnline)
                .Select(n => n.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();

            if (online.Contains(snapshot.GatewayId))
            {
                visited.Add(snapshot.GatewayId);
                queue.Enqueue(snapshot.GatewayId);

                result.Parents[snapshot.GatewayId] = null;
                result.HopCounts[snapshot.GatewayId] = 0;
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var (neighbor, _) in adjacency[current])
                {
                    if (!visited.Add(neighbor))
                        continue;

                    result.Parents[neighbor] = current;
                    result.HopCounts[neighbor] = result.HopCounts[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }

            result.ReachableNodeIds = visited
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            result.UnreachableNodeIds = snapshot.Nodes
                .Where(n => n.Type == MeshNodeType.Sensor && !visited.Contains(n.Id))
                .Select(n => n.Id)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return result;
        }

        // Prim's algorithm constructs the gateway-connected MST.
        public MeshMstResult GetMinimumSpanningTree()
        {
            var snapshot = GetSnapshot();
            var adjacency = BuildAdjacency(snapshot);

            var result = new MeshMstResult
            {
                GatewayId = snapshot.GatewayId
            };

            var online = snapshot.Nodes
                .Where(n => n.IsOnline)
                .Select(n => n.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (online.Contains(snapshot.GatewayId))
            {
                visited.Add(snapshot.GatewayId);

                var candidates = new PriorityQueue<(string From, string To, double Weight), double>();

                EnqueueLinks(snapshot.GatewayId);

                while (candidates.TryDequeue(out var candidate, out _))
                {
                    if (visited.Contains(candidate.To))
                        continue;

                    visited.Add(candidate.To);

                    result.SelectedLinks.Add(new MeshLink
                    {
                        SourceId = candidate.From,
                        TargetId = candidate.To,
                        Weight = candidate.Weight,
                        IsEnabled = true
                    });

                    result.TotalWeight += candidate.Weight;
                    EnqueueLinks(candidate.To);
                }

                void EnqueueLinks(string nodeId)
                {
                    foreach (var (neighbor, weight) in adjacency[nodeId])
                    {
                        if (!visited.Contains(neighbor))
                            candidates.Enqueue((nodeId, neighbor, weight), weight);
                    }
                }
            }

            result.ConnectedNodeIds = visited
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            result.DisconnectedNodeIds = snapshot.Nodes
                .Where(n => n.Type == MeshNodeType.Sensor && !visited.Contains(n.Id))
                .Select(n => n.Id)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            result.SpansAllOnlineNodes = online.All(visited.Contains);

            return result;
        }

        // Reconstruct a sensor-to-gateway path along the selected MST.
        public MeshRouteResult GetMstRoute(string sensorId)
        {
            var mst = GetMinimumSpanningTree();

            var result = new MeshRouteResult
            {
                SourceId = sensorId,
                GatewayId = mst.GatewayId
            };

            if (string.IsNullOrWhiteSpace(sensorId) ||
                !mst.ConnectedNodeIds.Contains(sensorId, StringComparer.OrdinalIgnoreCase))
                return result;

            var adjacency = new Dictionary<string, List<(string Neighbor, double Weight)>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var id in mst.ConnectedNodeIds)
                adjacency[id] = new List<(string, double)>();

            foreach (var link in mst.SelectedLinks)
            {
                adjacency[link.SourceId].Add((link.TargetId, link.Weight));
                adjacency[link.TargetId].Add((link.SourceId, link.Weight));
            }

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sensorId };
            var queue = new Queue<string>();
            var parents = new Dictionary<string, (string Parent, double Weight)>(
                StringComparer.OrdinalIgnoreCase);

            queue.Enqueue(sensorId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (string.Equals(current, mst.GatewayId, StringComparison.OrdinalIgnoreCase))
                    break;

                foreach (var (neighbor, weight) in adjacency[current])
                {
                    if (!visited.Add(neighbor))
                        continue;

                    parents[neighbor] = (current, weight);
                    queue.Enqueue(neighbor);
                }
            }

            if (!visited.Contains(mst.GatewayId))
                return result;

            var reversePath = new List<string> { mst.GatewayId };
            var cursor = mst.GatewayId;

            while (!string.Equals(cursor, sensorId, StringComparison.OrdinalIgnoreCase))
            {
                var previous = parents[cursor];
                result.TotalWeight += previous.Weight;
                cursor = previous.Parent;
                reversePath.Add(cursor);
            }

            reversePath.Reverse();

            result.Path = reversePath;
            result.HopCount = Math.Max(0, reversePath.Count - 1);
            result.IsReachable = true;

            return result;
        }

        // Build adjacency lists from enabled links between online nodes.
        private static Dictionary<string, List<(string Neighbor, double Weight)>> BuildAdjacency(
            MeshNetworkSnapshot snapshot)
        {
            var adjacency = snapshot.Nodes
                .Where(n => n.IsOnline)
                .ToDictionary(
                    n => n.Id,
                    _ => new List<(string Neighbor, double Weight)>(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var link in snapshot.Links)
            {
                if (!link.IsEnabled ||
                    !adjacency.ContainsKey(link.SourceId) ||
                    !adjacency.ContainsKey(link.TargetId))
                    continue;

                adjacency[link.SourceId].Add((link.TargetId, link.Weight));
                adjacency[link.TargetId].Add((link.SourceId, link.Weight));
            }

            return adjacency;
        }

        // An undirected edge has the same identity in either direction.
        private static string LinkKey(string a, string b)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(a, b) <= 0
                ? $"{a.ToUpperInvariant().Length}:{a.ToUpperInvariant()}{b.ToUpperInvariant().Length}:{b.ToUpperInvariant()}"
                : $"{b.ToUpperInvariant().Length}:{b.ToUpperInvariant()}{a.ToUpperInvariant().Length}:{a.ToUpperInvariant()}";
        }

        private static MeshNode CopyNode(MeshNode node) => new()
        {
            Id = node.Id,
            Label = node.Label,
            Type = node.Type,
            DeploymentLocation = node.DeploymentLocation,
            IsOnline = node.IsOnline,
            X = node.X,
            Y = node.Y
        };

        private static MeshLink CopyLink(MeshLink link) => new()
        {
            SourceId = link.SourceId,
            TargetId = link.TargetId,
            Weight = link.Weight,
            IsEnabled = link.IsEnabled
        };
    }
}
