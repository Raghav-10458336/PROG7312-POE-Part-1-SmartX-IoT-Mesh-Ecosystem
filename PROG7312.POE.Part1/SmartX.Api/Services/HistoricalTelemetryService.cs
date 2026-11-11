
using SmartX.Api.DataStructures;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class HistoricalTelemetryService
    {
        // Maintain timestamp-ordered telemetry using a self-balancing AVL tree.
        private readonly AvlTree<DateTime, object> _history = new();
        private readonly object _historyLock = new();

        public void AddFloat(TelemetryPacket<float> packet) =>
            AddPacket(packet.TimestampUtc, packet);

        public void AddInteger(TelemetryPacket<int> packet) =>
            AddPacket(packet.TimestampUtc, packet);

        public void AddBoolean(TelemetryPacket<bool> packet) =>
            AddPacket(packet.TimestampUtc, packet);

        private void AddPacket(DateTime timestamp, object packet)
        {
            lock (_historyLock)
            {
                _history.Insert(timestamp, packet);
            }
        }

        // Return all historical readings in ascending timestamp order.
        public IReadOnlyList<object> GetAll()
        {
            lock (_historyLock)
            {
                return _history.GetAll();
            }
        }

        // Preserve exact, case-insensitive device filtering.
        public IReadOnlyList<object> GetByDevice(string deviceIdentifier)
        {
            lock (_historyLock)
            {
                return _history.GetAll()
                    .Where(packet =>
                        GetDeviceIdentifier(packet).Equals(
                            deviceIdentifier,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .AsReadOnly();
            }
        }

        // Query only relevant AVL subtrees for the requested time range.
        public IReadOnlyList<object> GetByTimeRange(
            DateTime startUtc,
            DateTime endUtc)
        {
            lock (_historyLock)
            {
                return _history.GetRange(startUtc, endUtc);
            }
        }

        public int GetTotalCount()
        {
            lock (_historyLock)
            {
                return _history.Count;
            }
        }

        // Filter before limiting and return newest matching readings first.
        public IReadOnlyList<object> GetRecent(
            DateTime startUtc,
            DateTime endUtc,
            int limit,
            string? deviceIdentifier = null,
            string? metric = null)
        {
            limit = Math.Clamp(limit, 1, 100);

            lock (_historyLock)
            {
                return _history.GetRecent(
                    startUtc,
                    endUtc,
                    limit,
                    packet =>
                    {
                        if (!string.IsNullOrWhiteSpace(deviceIdentifier) &&
                            !GetDeviceIdentifier(packet).Contains(
                                deviceIdentifier,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        if (!string.IsNullOrWhiteSpace(metric) &&
                            !GetMetricName(packet).Contains(
                                metric,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        return true;
                    });
            }
        }

        private static string GetDeviceIdentifier(object packet) =>
            packet switch
            {
                TelemetryPacket<float> p => p.DeviceIdentifier,
                TelemetryPacket<int> p => p.DeviceIdentifier,
                TelemetryPacket<bool> p => p.DeviceIdentifier,
                _ => string.Empty
            };

        private static string GetMetricName(object packet) =>
            packet switch
            {
                TelemetryPacket<float> p => p.MetricName,
                TelemetryPacket<int> p => p.MetricName,
                TelemetryPacket<bool> p => p.MetricName,
                _ => string.Empty
            };
    }
}
