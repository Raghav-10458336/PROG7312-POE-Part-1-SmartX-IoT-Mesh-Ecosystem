
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class HistoricalTelemetryService
    {
        private readonly SortedDictionary<DateTime, List<object>>
            _history = new();

        private readonly object _historyLock = new();

        public void AddFloat(TelemetryPacket<float> packet)
        {
            AddPacket(packet.TimestampUtc, packet);
        }

        public void AddInteger(TelemetryPacket<int> packet)
        {
            AddPacket(packet.TimestampUtc, packet);
        }

        public void AddBoolean(TelemetryPacket<bool> packet)
        {
            AddPacket(packet.TimestampUtc, packet);
        }

        private void AddPacket(DateTime timestamp, object packet)
        {
            lock (_historyLock)
            {
                // Preserve packets that share the same timestamp.
                if (!_history.TryGetValue(timestamp, out var packets))
                {
                    packets = new List<object>();
                    _history[timestamp] = packets;
                }

                packets.Add(packet);
            }
        }

        public IReadOnlyList<object> GetAll()
        {
            lock (_historyLock)
            {
                return _history.Values
                    .SelectMany(packets => packets)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public IReadOnlyList<object> GetByDevice(
            string deviceIdentifier)
        {
            lock (_historyLock)
            {
                return _history.Values
                    .SelectMany(packets => packets)
                    .Where(packet => GetDeviceIdentifier(packet)
                        .Equals(deviceIdentifier,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public IReadOnlyList<object> GetByTimeRange(
            DateTime startUtc,
            DateTime endUtc)
        {
            lock (_historyLock)
            {
                return _history
                    .Where(entry =>
                        entry.Key >= startUtc &&
                        entry.Key <= endUtc)
                    .SelectMany(entry => entry.Value)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public int GetTotalCount()
        {
            lock (_historyLock)
            {
                return _history.Values.Sum(packets => packets.Count);
            }
        }

        private static string GetDeviceIdentifier(object packet)
        {
            return packet switch
            {
                TelemetryPacket<float> floatPacket =>
                    floatPacket.DeviceIdentifier,

                TelemetryPacket<int> integerPacket =>
                    integerPacket.DeviceIdentifier,

                TelemetryPacket<bool> booleanPacket =>
                    booleanPacket.DeviceIdentifier,

                _ => string.Empty
            };
        }
    }
}
