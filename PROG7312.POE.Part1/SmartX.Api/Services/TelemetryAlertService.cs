using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryAlertService
    {
        private readonly List<TelemetryAlert> _alerts = new();

        public IReadOnlyList<TelemetryAlert> GetAll()
        {
            return _alerts.AsReadOnly();
        }

        public int GetAlertCount()
        {
            return _alerts.Count;
        }

        public void EvaluateFloat(TelemetryPacket<float> packet)
        {
            switch (packet.MetricName)
            {
                case "Soil Moisture" when packet.Value < 20:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Critical",
                        $"Low soil moisture detected: {packet.Value:F1}%.");
                    break;

                case "Temperature" when packet.Value > 38:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Critical",
                        $"High temperature detected: {packet.Value:F1}°C.");
                    break;

                case "Humidity" when packet.Value < 25:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Warning",
                        $"Low humidity detected: {packet.Value:F1}%.");
                    break;
            }
        }

        public void EvaluateInteger(TelemetryPacket<int> packet)
        {
            switch (packet.MetricName)
            {
                case "Power Draw" when packet.Value > 4500:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Critical",
                        $"Power spike detected: {packet.Value} W.");
                    break;

                case "Energy Usage" when packet.Value > 5500:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Warning",
                        $"High energy usage detected: {packet.Value} Wh.");
                    break;

                case "Flow Rate" when packet.Value > 350:
                    AddAlert(
                        packet.DeviceIdentifier,
                        packet.MetricName,
                        "Warning",
                        $"Abnormal flow rate detected: {packet.Value} L/h.");
                    break;
            }
        }

        private void AddAlert(
            string deviceIdentifier,
            string metricName,
            string severity,
            string message)
        {
            _alerts.Add(new TelemetryAlert
            {
                DeviceIdentifier = deviceIdentifier,
                MetricName = metricName,
                Severity = severity,
                Message = message
            });
        }
    }
}