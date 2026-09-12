namespace SmartX.Shared.Telemetry
{
    public class TelemetryPacket<T>
    {
        public Guid PacketId { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public T Value { get; set; } = default!;

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        public TelemetryPacket()
        {
        }

        public TelemetryPacket(
            Guid sensorId,
            string deviceIdentifier,
            string metricName,
            string unit,
            T value)
        {
            SensorId = sensorId;
            DeviceIdentifier = deviceIdentifier;
            MetricName = metricName;
            Unit = unit;
            Value = value;
            TimestampUtc = DateTime.UtcNow;
        }
    }
}