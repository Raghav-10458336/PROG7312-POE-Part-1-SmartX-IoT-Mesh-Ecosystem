
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Tests
{
    [TestClass]
    public class MeshNetworkServiceTests
    {
        private const string Gateway = "SMARTX-GATEWAY";

        // Build a small, predictable graph for algorithm tests.
        private static MeshNetworkService CreateNetwork()
        {
            var network = new MeshNetworkService();

            AddNode(network, Gateway, MeshNodeType.Gateway);
            AddNode(network, "A");
            AddNode(network, "B");
            AddNode(network, "C");
            AddNode(network, "D");

            AddLink(network, Gateway, "A", 1);
            AddLink(network, Gateway, "B", 5);
            AddLink(network, "A", "B", 2);
            AddLink(network, "B", "C", 1);
            AddLink(network, "A", "C", 4);
            AddLink(network, "C", "D", 3);

            return network;
        }

        private static void AddNode(
            MeshNetworkService network,
            string id,
            MeshNodeType type = MeshNodeType.Sensor)
        {
            network.AddOrUpdateNode(new MeshNode
            {
                Id = id,
                Label = id,
                Type = type,
                IsOnline = true
            });
        }

        private static void AddLink(
            MeshNetworkService network,
            string source,
            string target,
            double weight)
        {
            network.AddOrUpdateLink(new MeshLink
            {
                SourceId = source,
                TargetId = target,
                Weight = weight,
                IsEnabled = true
            });
        }

        [TestMethod]
        public void Connectivity_AllNodesOnline_AllReachable()
        {
            var network = CreateNetwork();

            var result = network.GetConnectivity();

            Assert.AreEqual(5, result.ReachableNodeIds.Count);
            Assert.AreEqual(0, result.UnreachableNodeIds.Count);
            Assert.IsTrue(result.ReachableNodeIds.Contains("D"));
        }

        [TestMethod]
        public void Connectivity_BfsCalculatesMinimumHopCounts()
        {
            var network = CreateNetwork();

            var result = network.GetConnectivity();

            Assert.AreEqual(0, result.HopCounts[Gateway]);
            Assert.AreEqual(1, result.HopCounts["A"]);
            Assert.AreEqual(1, result.HopCounts["B"]);
            Assert.AreEqual(2, result.HopCounts["C"]);
            Assert.AreEqual(3, result.HopCounts["D"]);
        }

        [TestMethod]
        public void Connectivity_OfflineSensorBecomesUnreachable()
        {
            var network = CreateNetwork();

            Assert.IsTrue(network.SetNodeOnline("C", false));

            var result = network.GetConnectivity();

            Assert.IsTrue(result.UnreachableNodeIds.Contains("C"));
            Assert.IsTrue(result.UnreachableNodeIds.Contains("D"));
            Assert.AreEqual(3, result.ReachableNodeIds.Count);
        }

        [TestMethod]
        public void Connectivity_RestoringSensorRestoresConnectivity()
        {
            var network = CreateNetwork();

            network.SetNodeOnline("C", false);
            network.SetNodeOnline("C", true);

            var result = network.GetConnectivity();

            Assert.AreEqual(5, result.ReachableNodeIds.Count);
            Assert.AreEqual(0, result.UnreachableNodeIds.Count);
        }

        [TestMethod]
        public void Connectivity_DisabledLinkUsesAlternativeRoute()
        {
            var network = CreateNetwork();

            Assert.IsTrue(
                network.SetLinkEnabled(Gateway, "A", false));

            var result = network.GetConnectivity();

            Assert.AreEqual(5, result.ReachableNodeIds.Count);
            Assert.AreEqual(0, result.UnreachableNodeIds.Count);

            // A remains reachable through Gateway -> B -> A.
            Assert.AreEqual(2, result.HopCounts["A"]);
        }

        [TestMethod]
        public void Mst_ConnectedGraphHasMinimumTotalWeight()
        {
            var network = CreateNetwork();

            var result = network.GetMinimumSpanningTree();

            // Optimal edges:
            // Gateway-A = 1, A-B = 2, B-C = 1, C-D = 3.
            Assert.AreEqual(4, result.SelectedLinks.Count);
            Assert.AreEqual(7.0, result.TotalWeight, 0.000001);
            Assert.IsTrue(result.SpansAllOnlineNodes);
            Assert.AreEqual(0, result.DisconnectedNodeIds.Count);
        }

        [TestMethod]
        public void Mst_SelectedLinksFormTreeWithoutCycles()
        {
            var network = CreateNetwork();

            var result = network.GetMinimumSpanningTree();

            // A connected tree with V vertices must have V-1 edges.
            Assert.AreEqual(
                result.ConnectedNodeIds.Count - 1,
                result.SelectedLinks.Count);

            // Verify that the selected edges form one connected tree.
            var parent = result.ConnectedNodeIds.ToDictionary(
                id => id,
                id => id,
                StringComparer.OrdinalIgnoreCase);

            string Find(string id)
            {
                while (!string.Equals(
                    parent[id],
                    id,
                    StringComparison.OrdinalIgnoreCase))
                {
                    id = parent[id];
                }

                return id;
            }

            foreach (var link in result.SelectedLinks)
            {
                string rootA = Find(link.SourceId);
                string rootB = Find(link.TargetId);

                Assert.IsFalse(
                    string.Equals(
                        rootA,
                        rootB,
                        StringComparison.OrdinalIgnoreCase),
                    "The MST must not contain cycles.");

                parent[rootA] = rootB;
            }

            string gatewayRoot = Find(Gateway);

            foreach (string id in result.ConnectedNodeIds)
            {
                Assert.IsTrue(
                    string.Equals(
                        Find(id),
                        gatewayRoot,
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public void Mst_DisconnectedSensorIsExcluded()
        {
            var network = CreateNetwork();

            network.SetLinkEnabled("C", "D", false);

            var result = network.GetMinimumSpanningTree();

            Assert.IsTrue(result.DisconnectedNodeIds.Contains("D"));
            Assert.AreEqual(3, result.SelectedLinks.Count);
            Assert.AreEqual(4.0, result.TotalWeight, 0.000001);
            Assert.IsFalse(result.SpansAllOnlineNodes);
        }

        [TestMethod]
        public void Mst_LinkFailureRecalculatesMinimumWeight()
        {
            var network = CreateNetwork();

            network.SetLinkEnabled("A", "B", false);

            var result = network.GetMinimumSpanningTree();

            // Remaining optimal edges:
            // Gateway-A = 1, A-C = 4, B-C = 1, C-D = 3.
            Assert.AreEqual(4, result.SelectedLinks.Count);
            Assert.AreEqual(9.0, result.TotalWeight, 0.000001);
            Assert.IsTrue(result.SpansAllOnlineNodes);
        }

        [TestMethod]
        public void Mst_RestoringLinkRestoresOriginalWeight()
        {
            var network = CreateNetwork();

            network.SetLinkEnabled("A", "B", false);
            network.SetLinkEnabled("A", "B", true);

            var result = network.GetMinimumSpanningTree();

            Assert.AreEqual(7.0, result.TotalWeight, 0.000001);
        }

        [TestMethod]
        public void Route_MstPathHasCorrectHopsAndWeight()
        {
            var network = CreateNetwork();

            var result = network.GetMstRoute("D");

            CollectionAssert.AreEqual(
                new List<string> { "D", "C", "B", "A", Gateway },
                result.Path);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(4, result.HopCount);
            Assert.AreEqual(7.0, result.TotalWeight, 0.000001);
        }

        [TestMethod]
        public void Route_DisconnectedSensorIsUnreachable()
        {
            var network = CreateNetwork();

            network.SetLinkEnabled("C", "D", false);

            var result = network.GetMstRoute("D");

            Assert.IsFalse(result.IsReachable);
            Assert.AreEqual(0, result.Path.Count);
        }

        [TestMethod]
        public void Route_UnknownSensorIsUnreachable()
        {
            var network = CreateNetwork();

            var result = network.GetMstRoute("UNKNOWN-SENSOR");

            Assert.IsFalse(result.IsReachable);
            Assert.AreEqual(0, result.Path.Count);
        }

        [TestMethod]
        public void Network_RejectsInvalidLinks()
        {
            var network = CreateNetwork();

            Assert.ThrowsExactly<ArgumentException>(() =>
                AddLink(network, "A", "A", 1));

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                AddLink(network, "A", "B", -1));

            Assert.ThrowsExactly<InvalidOperationException>(() =>
                AddLink(network, "A", "UNKNOWN", 1));
        }

        [TestMethod]
        public void Network_LinkLookupIsDirectionIndependent()
        {
            var network = CreateNetwork();

            Assert.IsTrue(
                network.SetLinkEnabled("B", "A", false));

            var snapshot = network.GetSnapshot();

            var link = snapshot.Links.Single(l =>
                (l.SourceId == "A" && l.TargetId == "B") ||
                (l.SourceId == "B" && l.TargetId == "A"));

            Assert.IsFalse(link.IsEnabled);
        }

        [TestMethod]
        public void Network_SnapshotCannotModifyInternalState()
        {
            var network = CreateNetwork();

            var snapshot = network.GetSnapshot();

            snapshot.Nodes.Single(n => n.Id == "A").IsOnline = false;
            snapshot.Links.First().IsEnabled = false;

            var actual = network.GetSnapshot();

            Assert.IsTrue(actual.Nodes.Single(n => n.Id == "A").IsOnline);
            Assert.IsTrue(actual.Links.All(l => l.IsEnabled));
        }

        // Dijkstra shortest-path tests.

        [TestMethod]
        public void Dijkstra_FindsMinimumWeightRoute()
        {
            var network = CreateNetwork();

            var result = network.GetShortestRoute("D");

            CollectionAssert.AreEqual(
                new List<string> { "D", "C", "B", "A", Gateway },
                result.Path);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(4, result.HopCount);
            Assert.AreEqual(7.0, result.TotalWeight, 0.000001);
        }

        [TestMethod]
        public void Dijkstra_PrefersLowerWeightOverFewerHops()
        {
            var network = CreateNetwork();

            // B-Gateway costs 5.
            // B-A-Gateway costs 3.
            var result = network.GetShortestRoute("B");

            CollectionAssert.AreEqual(
                new List<string> { "B", "A", Gateway },
                result.Path);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(2, result.HopCount);
            Assert.AreEqual(3.0, result.TotalWeight, 0.000001);
        }

        [TestMethod]
        public void Dijkstra_DisabledLinkTriggersAlternativeRoute()
        {
            var network = CreateNetwork();

            Assert.IsTrue(
                network.SetLinkEnabled("A", "B", false));

            var result = network.GetShortestRoute("B");

            // The direct gateway link now costs less than B-C-A-Gateway.
            CollectionAssert.AreEqual(
                new List<string> { "B", Gateway },
                result.Path);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(1, result.HopCount);
            Assert.AreEqual(5.0, result.TotalWeight, 0.000001);
        }

        [TestMethod]
        public void Dijkstra_OfflineSensorCannotBeUsed()
        {
            var network = CreateNetwork();

            Assert.IsTrue(network.SetNodeOnline("C", false));

            var result = network.GetShortestRoute("D");

            Assert.IsFalse(result.IsReachable);
            Assert.AreEqual(0, result.Path.Count);
        }

        [TestMethod]
        public void Dijkstra_UnknownSensorIsUnreachable()
        {
            var network = CreateNetwork();

            var result = network.GetShortestRoute("UNKNOWN");

            Assert.IsFalse(result.IsReachable);
            Assert.AreEqual(0, result.Path.Count);
        }

        [TestMethod]
        public void Dijkstra_RestoredLinkRestoresOptimalRoute()
        {
            var network = CreateNetwork();

            Assert.IsTrue(network.SetLinkEnabled("A", "B", false));
            Assert.IsTrue(network.SetLinkEnabled("A", "B", true));

            var result = network.GetShortestRoute("B");

            CollectionAssert.AreEqual(
                new List<string> { "B", "A", Gateway },
                result.Path);

            Assert.IsTrue(result.IsReachable);
            Assert.AreEqual(2, result.HopCount);
            Assert.AreEqual(3.0, result.TotalWeight, 0.000001);
        }
    }
}
