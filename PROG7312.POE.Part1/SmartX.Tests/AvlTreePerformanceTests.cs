
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.DataStructures;
using SmartX.Api.Services;
using SmartX.Shared.Telemetry;
using System;
using System.Diagnostics;
using System.Linq;

namespace SmartX.Tests
{
    [TestClass]
    public class AvlTreePerformanceTests
    {
        private static readonly DateTime BaseTime =
            new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        // Verify large ascending datasets remain searchable.
        [TestMethod]
        public void LargeDataset_AscendingInsertion_PreservesSortedOrder()
        {
            var tree = new AvlTree<int, int>();

            for (int i = 0; i < 10000; i++)
                tree.Insert(i, i);

            Assert.AreEqual(10000, tree.Count);

            CollectionAssert.AreEqual(
                Enumerable.Range(0, 10000).ToArray(),
                tree.GetAll().ToArray());
        }

        // Retrieve a narrow range from extensive history.
        [TestMethod]
        public void LargeDataset_RangeQuery_ReturnsExactReadings()
        {
            var tree = CreateLargeTree();

            var results = tree.GetRange(
                BaseTime.AddSeconds(4500),
                BaseTime.AddSeconds(4510));

            Assert.AreEqual(11, results.Count);

            CollectionAssert.AreEqual(
                Enumerable.Range(4500, 11).ToArray(),
                results.ToArray());
        }

        // Ensure newest-first traversal respects the limit.
        [TestMethod]
        public void LargeDataset_RecentQuery_ReturnsNewestReadings()
        {
            var tree = CreateLargeTree();

            var results = tree.GetRecent(
                BaseTime,
                BaseTime.AddSeconds(9999),
                5);

            CollectionAssert.AreEqual(
                new[] { 9999, 9998, 9997, 9996, 9995 },
                results.ToArray());
        }

        // Verify range queries are independent of insertion order.
        [TestMethod]
        public void LargeDataset_ReverseInsertion_ReturnsCorrectRange()
        {
            var tree = new AvlTree<int, int>();

            for (int i = 9999; i >= 0; i--)
                tree.Insert(i, i);

            var results = tree.GetRange(100, 105);

            CollectionAssert.AreEqual(
                new[] { 100, 101, 102, 103, 104, 105 },
                results.ToArray());

            Assert.AreEqual(10000, tree.Count);
        }

        // Verify duplicate timestamps survive large insertions.
        [TestMethod]
        public void LargeDataset_DuplicateTimestamps_PreserveAllValues()
        {
            var tree = CreateLargeTree();

            var timestamp = BaseTime.AddSeconds(5000);

            tree.Insert(timestamp, 20000);
            tree.Insert(timestamp, 30000);

            var results = tree.GetRange(timestamp, timestamp);

            CollectionAssert.AreEqual(
                new[] { 5000, 20000, 30000 },
                results.ToArray());

            Assert.AreEqual(10002, tree.Count);
        }

        // Verify the real historical service uses the AVL range query.
        [TestMethod]
        public void HistoricalService_LargeDataset_ReturnsCorrectTimeRange()
        {
            var history = new HistoricalTelemetryService();

            for (int i = 0; i < 10000; i++)
            {
                history.AddFloat(new TelemetryPacket<float>
                {
                    DeviceIdentifier = "ESP32-TEST-01",
                    MetricName = "Temperature",
                    Unit = "C",
                    Value = i,
                    TimestampUtc = BaseTime.AddSeconds(i)
                });
            }

            var results = history.GetByTimeRange(
                BaseTime.AddSeconds(2000),
                BaseTime.AddSeconds(2009));

            Assert.AreEqual(10000, history.GetTotalCount());
            Assert.AreEqual(10, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                var packet =
                    results[i] as TelemetryPacket<float>;

                Assert.IsNotNull(packet);
                Assert.AreEqual(2000 + i, packet.Value);
            }
        }

        // Record diagnostic timings without fragile speed thresholds.
        [TestMethod]
        public void LargeDataset_PerformanceMeasurements_AreRecorded()
        {
            var tree = new AvlTree<DateTime, int>();
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 10000; i++)
                tree.Insert(BaseTime.AddSeconds(i), i);

            stopwatch.Stop();
            long insertionTicks = stopwatch.ElapsedTicks;

            stopwatch.Restart();

            var results = tree.GetRange(
                BaseTime.AddSeconds(5000),
                BaseTime.AddSeconds(5099));

            stopwatch.Stop();
            long rangeQueryTicks = stopwatch.ElapsedTicks;

            Assert.AreEqual(10000, tree.Count);
            Assert.AreEqual(100, results.Count);

            Trace.WriteLine(
                $"AVL insertion (10,000 readings): " +
                $"{insertionTicks} stopwatch ticks");

            Trace.WriteLine(
                $"AVL range query (100 readings): " +
                $"{rangeQueryTicks} stopwatch ticks");

            Trace.WriteLine(
                $"Stopwatch frequency: {Stopwatch.Frequency} ticks/sec");
        }

        private static AvlTree<DateTime, int> CreateLargeTree()
        {
            var tree = new AvlTree<DateTime, int>();

            for (int i = 0; i < 10000; i++)
                tree.Insert(BaseTime.AddSeconds(i), i);

            return tree;
        }
    }
}
