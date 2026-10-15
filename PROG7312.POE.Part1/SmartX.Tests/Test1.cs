
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests
{
    [TestClass]
    public class TelemetryQueueTests
    {
        [TestMethod]
        public void NormalPackets_AreProcessedInFIFOOrder()
        {
            var queue = new TelemetryQueueService();

            var first = new TelemetryPacket<float>
            {
                MetricName = "Temperature",
                Value = 25
            };

            var second = new TelemetryPacket<float>
            {
                MetricName = "Temperature",
                Value = 28
            };

            queue.EnqueueFloat(first);
            queue.EnqueueFloat(second);

            Assert.IsTrue(queue.TryDequeue(out object? result1));
            Assert.IsTrue(queue.TryDequeue(out object? result2));

            Assert.AreSame(first, result1);
            Assert.AreSame(second, result2);
        }

        [TestMethod]
        public void CriticalPacket_IsProcessedBeforeNormalPacket()
        {
            var queue = new TelemetryQueueService();

            var normal = new TelemetryPacket<float>
            {
                MetricName = "Temperature",
                Value = 25
            };

            var critical = new TelemetryPacket<float>
            {
                MetricName = "Temperature",
                Value = 45
            };

            queue.EnqueueFloat(normal);
            queue.EnqueueFloat(critical);

            Assert.IsTrue(queue.TryDequeue(out object? result1));
            Assert.IsTrue(queue.TryDequeue(out object? result2));

            Assert.AreSame(critical, result1);
            Assert.AreSame(normal, result2);
        }

        [TestMethod]
        public void CriticalPacket_IsProcessedBeforeWarningPacket()
        {
            var queue = new TelemetryQueueService();

            var warning = new TelemetryPacket<float>
            {
                MetricName = "Humidity",
                Value = 15
            };

            var critical = new TelemetryPacket<float>
            {
                MetricName = "Soil Moisture",
                Value = 10
            };

            queue.EnqueueFloat(warning);
            queue.EnqueueFloat(critical);

            Assert.IsTrue(queue.TryDequeue(out object? result1));
            Assert.IsTrue(queue.TryDequeue(out object? result2));

            Assert.AreSame(critical, result1);
            Assert.AreSame(warning, result2);
        }
    }
}
