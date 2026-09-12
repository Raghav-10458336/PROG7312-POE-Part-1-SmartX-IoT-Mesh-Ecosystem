namespace SmartX.Shared.Models
{
    public class TelemetryAlert
    {
        public Guid AlertId { get; set; } = Guid.NewGuid();

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Severity { get; set; } = "Warning";

        public string Message { get; set; } = string.Empty;

        public DateTime DetectedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
