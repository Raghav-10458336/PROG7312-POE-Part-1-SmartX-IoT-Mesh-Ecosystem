
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests
{
    [TestClass]
    public class HistoricalTelemetryTests
    {
        [TestMethod]
        public void History_ReturnsPacketsInChronologicalOrder()
        {
            var history = new HistoricalTelemetryService();

            var later = new TelemetryPacket<float>
            {
                DeviceIdentifier = "ENV-01",
                Value = 30,
                TimestampUtc = new DateTime(2026, 10, 15, 12, 0, 0,
                    DateTimeKind.Utc)
            };

            var earlier = new TelemetryPacket<float>
            {
                DeviceIdentifier = "ENV-01",
                Value = 20,
                TimestampUtc = new DateTime(2026, 10, 15, 10, 0, 0,
                    DateTimeKind.Utc)
            };

            history.AddFloat(later);
            history.AddFloat(earlier);

            var results = history.GetAll();

            Assert.AreSame(earlier, results[0]);
            Assert.AreSame(later, results[1]);
        }

        [TestMethod]
        public void History_FiltersByDeviceIdentifier()
        {
            var history = new HistoricalTelemetryService();

            history.AddFloat(new TelemetryPacket<float>
            {
                DeviceIdentifier = "ENV-01",
                Value = 25
            });

            history.AddInteger(new TelemetryPacket<int>
            {
                DeviceIdentifier = "PWR-01",
                Value = 100
            });

            var results = history.GetByDevice("env-01");

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void History_FiltersByTimeRange()
        {
            var history = new HistoricalTelemetryService();

            var inside = new TelemetryPacket<float>
            {
                TimestampUtc = new DateTime(2026, 10, 15, 12, 0, 0,
                    DateTimeKind.Utc)
            };

            var outside = new TelemetryPacket<float>
            {
                TimestampUtc = new DateTime(2026, 10, 15, 18, 0, 0,
                    DateTimeKind.Utc)
            };

            history.AddFloat(inside);
            history.AddFloat(outside);

            var results = history.GetByTimeRange(
                new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(inside, results[0]);
        }

        [TestMethod]
        public void History_PreservesPacketsWithSameTimestamp()
        {
            var history = new HistoricalTelemetryService();

            var timestamp = new DateTime(
                2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

            history.AddFloat(new TelemetryPacket<float>
            {
                TimestampUtc = timestamp,
                Value = 25
            });

            history.AddBoolean(new TelemetryPacket<bool>
            {
                TimestampUtc = timestamp,
                Value = true
            });

            Assert.AreEqual(2, history.GetTotalCount());
        }
    }
}
