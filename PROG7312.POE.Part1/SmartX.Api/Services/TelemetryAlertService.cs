using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryAlertService
    {
        private readonly List<TelemetryAlert> _alerts = new();

        public IReadOnlyList<TelemetryAlert> GetAll()
        {
            return _alerts
                .OrderByDescending(
                    alert => alert.DetectedAtUtc)
                .ToList()
                .AsReadOnly();
        }

        public int GetAlertCount()
        {
            return _alerts.Count;
        }

        public int GetUnacknowledgedCount()
        {
            return _alerts.Count(
                alert => !alert.IsAcknowledged);
        }

        public TelemetryAlert? GetById(
            Guid alertId)
        {
            return _alerts.FirstOrDefault(
                alert =>
                    alert.AlertId == alertId);
        }

        public bool Acknowledge(
            Guid alertId)
        {
            TelemetryAlert? alert =
                GetById(alertId);

            if (alert is null)
            {
                return false;
            }

            if (alert.IsAcknowledged)
            {
                return true;
            }

            alert.IsAcknowledged = true;

            alert.AcknowledgedAtUtc =
                DateTime.UtcNow;

            return true;
        }

        public void EvaluateFloat(
            TelemetryPacket<float> packet)
        {
            if (packet.MetricName == "Soil Moisture"
                && packet.Value < 20)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Critical",
                    $"Soil moisture dropped to " +
                    $"{packet.Value:F2}%.");
            }
            else if (
                packet.MetricName == "Temperature"
                && packet.Value > 38)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Critical",
                    $"Temperature increased to " +
                    $"{packet.Value:F2} °C.");
            }
            else if (
                packet.MetricName == "Humidity"
                && packet.Value < 25)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Warning",
                    $"Humidity dropped to " +
                    $"{packet.Value:F2}%.");
            }
        }

        public void EvaluateInteger(
            TelemetryPacket<int> packet)
        {
            if (packet.MetricName == "Power Draw"
                && packet.Value > 4500)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Critical",
                    $"Power draw increased to " +
                    $"{packet.Value:N0} W.");
            }
            else if (
                packet.MetricName == "Energy Usage"
                && packet.Value > 5500)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Warning",
                    $"Energy usage increased to " +
                    $"{packet.Value:N0} Wh.");
            }
            else if (
                packet.MetricName == "Flow Rate"
                && packet.Value > 350)
            {
                AddAlert(
                    packet.DeviceIdentifier,
                    packet.MetricName,
                    "Warning",
                    $"Flow rate increased to " +
                    $"{packet.Value:N0} L/h.");
            }
        }

        public void EvaluateDisconnect(
            string deviceIdentifier,
            string deploymentLocation,
            DateTime? lastSeenUtc,
            DateTime evaluationTimeUtc,
            TimeSpan disconnectThreshold)
        {
            bool alreadyReported =
                _alerts.Any(alert =>
                    alert.DeviceIdentifier.Equals(
                        deviceIdentifier,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    alert.MetricName.Equals(
                        "Sensor Connectivity",
                        StringComparison.OrdinalIgnoreCase));

            if (alreadyReported)
            {
                return;
            }

            if (!lastSeenUtc.HasValue)
            {
                AddAlert(
                    deviceIdentifier,
                    "Sensor Connectivity",
                    "Critical",
                    $"No telemetry has been received from " +
                    $"{deviceIdentifier} at " +
                    $"{deploymentLocation}.");

                return;
            }

            TimeSpan timeSinceLastTelemetry =
                evaluationTimeUtc -
                lastSeenUtc.Value;

            if (timeSinceLastTelemetry <=
                disconnectThreshold)
            {
                return;
            }

            int staleMinutes =
                Math.Max(
                    1,
                    (int)Math.Floor(
                        timeSinceLastTelemetry.TotalMinutes));

            AddAlert(
                deviceIdentifier,
                "Sensor Connectivity",
                "Critical",
                $"{deviceIdentifier} at " +
                $"{deploymentLocation} has not reported " +
                $"telemetry for approximately " +
                $"{staleMinutes} minutes.");
        }

        private void AddAlert(
            string deviceIdentifier,
            string metricName,
            string severity,
            string message)
        {
            TelemetryAlert alert = new()
            {
                DeviceIdentifier =
                    deviceIdentifier,

                MetricName =
                    metricName,

                Severity =
                    severity,

                Message =
                    message,

                DetectedAtUtc =
                    DateTime.UtcNow,

                IsAcknowledged =
                    false
            };

            _alerts.Add(alert);
        }
    }
}