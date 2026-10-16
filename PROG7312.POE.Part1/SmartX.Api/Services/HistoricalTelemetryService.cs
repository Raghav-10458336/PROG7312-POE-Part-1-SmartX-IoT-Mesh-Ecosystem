using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class HistoricalTelemetryService
    {
        private readonly SortedDictionary<DateTime, List<object>> _history = new();
        private readonly object _historyLock = new();

        public void AddFloat(TelemetryPacket<float> packet) => AddPacket(packet.TimestampUtc, packet);
        public void AddInteger(TelemetryPacket<int> packet) => AddPacket(packet.TimestampUtc, packet);
        public void AddBoolean(TelemetryPacket<bool> packet) => AddPacket(packet.TimestampUtc, packet);

        private void AddPacket(DateTime timestamp, object packet)
        {
            lock (_historyLock)
            {
                if (!_history.TryGetValue(timestamp, out var packets))
                {
                    packets = new List<object>();
                    _history[timestamp] = packets;
                }
                packets.Add(packet);
            }
        }

        // Retain the existing methods for callers and tests.
        public IReadOnlyList<object> GetAll()
        {
            lock (_historyLock)
                return _history.Values.SelectMany(x => x).ToList().AsReadOnly();
        }

        public IReadOnlyList<object> GetByDevice(string deviceIdentifier)
        {
            lock (_historyLock)
                return _history.Values.SelectMany(x => x)
                    .Where(x => GetDeviceIdentifier(x).Equals(deviceIdentifier, StringComparison.OrdinalIgnoreCase))
                    .ToList().AsReadOnly();
        }

        public IReadOnlyList<object> GetByTimeRange(DateTime startUtc, DateTime endUtc)
        {
            lock (_historyLock)
                return _history.Where(x => x.Key >= startUtc && x.Key <= endUtc)
                    .SelectMany(x => x.Value).ToList().AsReadOnly();
        }

        public int GetTotalCount()
        {
            lock (_historyLock)
                return _history.Values.Sum(x => x.Count);
        }

        // Filter before limiting; iterate the sorted timeline newest-first.
        public IReadOnlyList<object> GetRecent(
            DateTime startUtc, DateTime endUtc, int limit,
            string? deviceIdentifier = null, string? metric = null)
        {
            limit = Math.Clamp(limit, 1, 100);
            var result = new List<object>(limit);

            lock (_historyLock)
            {
                foreach (var entry in _history.Reverse())
                {
                    if (entry.Key > endUtc) continue;
                    if (entry.Key < startUtc) break;

                    for (int i = entry.Value.Count - 1; i >= 0; i--)
                    {
                        var packet = entry.Value[i];
                        if (!string.IsNullOrWhiteSpace(deviceIdentifier) &&
                            !GetDeviceIdentifier(packet).Contains(deviceIdentifier, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (!string.IsNullOrWhiteSpace(metric) &&
                            !GetMetricName(packet).Contains(metric, StringComparison.OrdinalIgnoreCase))
                            continue;

                        result.Add(packet);
                        if (result.Count == limit) return result.AsReadOnly();
                    }
                }
            }
            return result.AsReadOnly();
        }

        private static string GetDeviceIdentifier(object packet) => packet switch
        {
            TelemetryPacket<float> p => p.DeviceIdentifier,
            TelemetryPacket<int> p => p.DeviceIdentifier,
            TelemetryPacket<bool> p => p.DeviceIdentifier,
            _ => string.Empty
        };

        private static string GetMetricName(object packet) => packet switch
        {
            TelemetryPacket<float> p => p.MetricName,
            TelemetryPacket<int> p => p.MetricName,
            TelemetryPacket<bool> p => p.MetricName,
            _ => string.Empty
        };
    }
}
