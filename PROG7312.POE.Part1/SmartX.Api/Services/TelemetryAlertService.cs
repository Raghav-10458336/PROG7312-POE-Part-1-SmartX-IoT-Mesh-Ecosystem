
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryAlertService
    {
        private readonly List<TelemetryAlert> _alerts = new();

        // Track unique faults that have not yet recovered.
        private readonly HashSet<string> _activeFaults =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _alertLock = new();

        public IReadOnlyList<TelemetryAlert> GetAll()
        {
            lock (_alertLock)
            {
                return _alerts
                    .OrderByDescending(alert => alert.DetectedAtUtc)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public int GetAlertCount()
        {
            lock (_alertLock)
            {
                return _alerts.Count;
            }
        }

        public int GetUnacknowledgedCount()
        {
            lock (_alertLock)
            {
                return _alerts.Count(alert => !alert.IsAcknowledged);
            }
        }

        public int GetActiveFaultCount()
        {
            lock (_alertLock)
            {
                return _activeFaults.Count;
            }
        }

        public IReadOnlyList<string> GetActiveFaults()
        {
            lock (_alertLock)
            {
                return _activeFaults
                    .OrderBy(fault => fault)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public TelemetryAlert? GetById(Guid alertId)
        {
            lock (_alertLock)
            {
                return _alerts.FirstOrDefault(
                    alert => alert.AlertId == alertId);
            }
        }

        public bool Acknowledge(Guid alertId)
        {
            lock (_alertLock)
            {
                TelemetryAlert? alert = _alerts.FirstOrDefault(
                    item => item.AlertId == alertId);

                if (alert is null)
                {
                    return false;
                }

                if (alert.IsAcknowledged)
                {
                    return true;
                }

                alert.IsAcknowledged = true;
                alert.AcknowledgedAtUtc = DateTime.UtcNow;

                return true;
            }
        }

        public void EvaluateFloat(TelemetryPacket<float> packet)
        {
            string? severity = null;
            string? message = null;

            if (packet.MetricName == "Soil Moisture"
                && packet.Value < 20)
            {
                severity = "Critical";
                message =
                    $"Soil moisture dropped to {packet.Value:F2}%.";
            }
            else if (packet.MetricName == "Temperature"
                && packet.Value > 38)
            {
                severity = "Critical";
                message =
                    $"Temperature increased to {packet.Value:F2} °C.";
            }
            else if (packet.MetricName == "Humidity"
                && packet.Value < 25)
            {
                severity = "Warning";
                message =
                    $"Humidity dropped to {packet.Value:F2}%.";
            }

            UpdateFaultState(
                packet.DeviceIdentifier,
                packet.MetricName,
                severity,
                message);
        }

        public void EvaluateInteger(TelemetryPacket<int> packet)
        {
            string? severity = null;
            string? message = null;

            if (packet.MetricName == "Power Draw"
                && packet.Value > 4500)
            {
                severity = "Critical";
                message =
                    $"Power draw increased to {packet.Value:N0} W.";
            }
            else if (packet.MetricName == "Energy Usage"
                && packet.Value > 5500)
            {
                severity = "Warning";
                message =
                    $"Energy usage increased to {packet.Value:N0} Wh.";
            }
            else if (packet.MetricName == "Flow Rate"
                && packet.Value > 350)
            {
                severity = "Warning";
                message =
                    $"Flow rate increased to {packet.Value:N0} L/h.";
            }

            UpdateFaultState(
                packet.DeviceIdentifier,
                packet.MetricName,
                severity,
                message);
        }

        public void EvaluateDisconnect(
            string deviceIdentifier,
            string deploymentLocation,
            DateTime? lastSeenUtc,
            DateTime evaluationTimeUtc,
            TimeSpan disconnectThreshold)
        {
            string? message = null;

            if (!lastSeenUtc.HasValue)
            {
                message =
                    $"No telemetry has been received from " +
                    $"{deviceIdentifier} at {deploymentLocation}.";
            }
            else
            {
                TimeSpan timeSinceLastTelemetry =
                    evaluationTimeUtc - lastSeenUtc.Value;

                if (timeSinceLastTelemetry > disconnectThreshold)
                {
                    int staleMinutes = Math.Max(
                        1,
                        (int)Math.Floor(
                            timeSinceLastTelemetry.TotalMinutes));

                    message =
                        $"{deviceIdentifier} at {deploymentLocation} " +
                        $"has not reported telemetry for approximately " +
                        $"{staleMinutes} minutes.";
                }
            }

            UpdateFaultState(
                deviceIdentifier,
                "Sensor Connectivity",
                message is null ? null : "Critical",
                message);
        }

        private void UpdateFaultState(
            string deviceIdentifier,
            string metricName,
            string? severity,
            string? message)
        {
            string faultKey =
                $"{deviceIdentifier.Trim()}:{metricName.Trim()}";

            lock (_alertLock)
            {
                if (severity is null || message is null)
                {
                    // A normal reading clears the active fault.
                    _activeFaults.Remove(faultKey);
                    return;
                }

                // HashSet.Add returns false for an existing fault.
                if (!_activeFaults.Add(faultKey))
                {
                    return;
                }

                TelemetryAlert alert = new()
                {
                    DeviceIdentifier = deviceIdentifier,
                    MetricName = metricName,
                    Severity = severity,
                    Message = message,
                    DetectedAtUtc = DateTime.UtcNow,
                    IsAcknowledged = false
                };

                _alerts.Add(alert);
            }
        }
    }
}
