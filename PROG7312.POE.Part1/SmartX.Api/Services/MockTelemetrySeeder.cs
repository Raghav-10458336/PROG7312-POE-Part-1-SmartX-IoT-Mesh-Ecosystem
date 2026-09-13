using SmartX.Shared.Enums;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class MockTelemetrySeeder
    {
        private readonly TelemetryIngestionService _telemetryService;
        private readonly SensorRegistryService _sensorRegistryService;
        private readonly TelemetryAlertService _alertService;

        private readonly Random _random = new();

        private static readonly TimeSpan DisconnectThreshold =
            TimeSpan.FromMinutes(10);

        public MockTelemetrySeeder(
            TelemetryIngestionService telemetryService,
            SensorRegistryService sensorRegistryService,
            TelemetryAlertService alertService)
        {
            _telemetryService = telemetryService;
            _sensorRegistryService = sensorRegistryService;
            _alertService = alertService;
        }

        public void Seed()
        {
            Dictionary<string, SensorRegistration> sensors =
                SeedSensors();

            SeedFloatTelemetry(sensors);
            SeedIntegerTelemetry(sensors);
            SeedBooleanTelemetry(sensors);

            DetectDisconnectedSensors(sensors);
        }

        private Dictionary<string, SensorRegistration> SeedSensors()
        {
            Dictionary<string, SensorRegistration> sensors = new();

            for (int i = 1; i <= 8; i++)
            {
                SensorRegistration environmentalSensor = new()
                {
                    DeviceIdentifier = $"ESP32-ENV-{i:D2}",
                    DeploymentLocation = $"Hydroponic Zone {i}",
                    Category = SensorCategory.Environmental
                };

                SensorRegistration powerSensor = new()
                {
                    DeviceIdentifier = $"ESP32-PWR-{i:D2}",
                    DeploymentLocation = $"Utility Node {i}",
                    Category = SensorCategory.PowerConsumption
                };

                SensorRegistration actuatorSensor = new()
                {
                    DeviceIdentifier = $"ESP32-ACT-{i:D2}",
                    DeploymentLocation = $"Actuator Node {i}",
                    Category = SensorCategory.Actuator
                };

                _sensorRegistryService.Register(
                    environmentalSensor);

                _sensorRegistryService.Register(
                    powerSensor);

                _sensorRegistryService.Register(
                    actuatorSensor);

                sensors[environmentalSensor.DeviceIdentifier] =
                    environmentalSensor;

                sensors[powerSensor.DeviceIdentifier] =
                    powerSensor;

                sensors[actuatorSensor.DeviceIdentifier] =
                    actuatorSensor;
            }

            return sensors;
        }

        private void SeedFloatTelemetry(
            Dictionary<string, SensorRegistration> sensors)
        {
            string[] metricNames =
            {
                "Soil Moisture",
                "Temperature",
                "Humidity"
            };

            string[] units =
            {
                "%",
                "°C",
                "%"
            };

            TelemetryPacket<float>[][] batches =
                new TelemetryPacket<float>[8][];

            for (
                int batchIndex = 0;
                batchIndex < batches.Length;
                batchIndex++)
            {
                int batchSize = 300;

                batches[batchIndex] =
                    new TelemetryPacket<float>[batchSize];

                string deviceIdentifier =
                    $"ESP32-ENV-{batchIndex + 1:D2}";

                Guid sensorId =
                    sensors[deviceIdentifier].Id;

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex =
                        i % metricNames.Length;

                    float value =
                        GenerateFloatValue(
                            metricNames[metricIndex],
                            i);

                    TelemetryPacket<float> packet =
                        new(
                            sensorId,
                            deviceIdentifier,
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);

                    packet.TimestampUtc =
                        GetSimulatedTimestamp(
                            deviceIdentifier);

                    batches[batchIndex][i] =
                        packet;
                }
            }

            TelemetryBatch<TelemetryPacket<float>>
                telemetryBatch =
                    new(batches);

            foreach (
                TelemetryPacket<float> packet
                in telemetryBatch.ToList())
            {
                _telemetryService.IngestFloat(packet);
            }
        }

        private void SeedIntegerTelemetry(
            Dictionary<string, SensorRegistration> sensors)
        {
            string[] metricNames =
            {
                "Power Draw",
                "Energy Usage",
                "Flow Rate"
            };

            string[] units =
            {
                "W",
                "Wh",
                "L/h"
            };

            TelemetryPacket<int>[][] batches =
                new TelemetryPacket<int>[8][];

            for (
                int batchIndex = 0;
                batchIndex < batches.Length;
                batchIndex++)
            {
                int batchSize = 250;

                batches[batchIndex] =
                    new TelemetryPacket<int>[batchSize];

                string deviceIdentifier =
                    $"ESP32-PWR-{batchIndex + 1:D2}";

                Guid sensorId =
                    sensors[deviceIdentifier].Id;

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex =
                        i % metricNames.Length;

                    int value =
                        GenerateIntegerValue(
                            metricNames[metricIndex],
                            i);

                    TelemetryPacket<int> packet =
                        new(
                            sensorId,
                            deviceIdentifier,
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);

                    packet.TimestampUtc =
                        GetSimulatedTimestamp(
                            deviceIdentifier);

                    batches[batchIndex][i] =
                        packet;
                }
            }

            TelemetryBatch<TelemetryPacket<int>>
                telemetryBatch =
                    new(batches);

            foreach (
                TelemetryPacket<int> packet
                in telemetryBatch.ToList())
            {
                _telemetryService.IngestInteger(packet);
            }
        }

        private void SeedBooleanTelemetry(
            Dictionary<string, SensorRegistration> sensors)
        {
            string[] metricNames =
            {
                "Valve State",
                "Pump State",
                "Relay State"
            };

            string[] units =
            {
                "Open",
                "Running",
                "Enabled"
            };

            TelemetryPacket<bool>[][] batches =
                new TelemetryPacket<bool>[8][];

            for (
                int batchIndex = 0;
                batchIndex < batches.Length;
                batchIndex++)
            {
                int batchSize = 200;

                batches[batchIndex] =
                    new TelemetryPacket<bool>[batchSize];

                string deviceIdentifier =
                    $"ESP32-ACT-{batchIndex + 1:D2}";

                Guid sensorId =
                    sensors[deviceIdentifier].Id;

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex =
                        i % metricNames.Length;

                    bool value =
                        _random.Next(0, 2) == 1;

                    TelemetryPacket<bool> packet =
                        new(
                            sensorId,
                            deviceIdentifier,
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);

                    packet.TimestampUtc =
                        GetSimulatedTimestamp(
                            deviceIdentifier);

                    batches[batchIndex][i] =
                        packet;
                }
            }

            TelemetryBatch<TelemetryPacket<bool>>
                telemetryBatch =
                    new(batches);

            foreach (
                TelemetryPacket<bool> packet
                in telemetryBatch.ToList())
            {
                _telemetryService.IngestBoolean(packet);
            }
        }

        private DateTime GetSimulatedTimestamp(
            string deviceIdentifier)
        {
            return deviceIdentifier switch
            {
                "ESP32-ENV-08" =>
                    DateTime.UtcNow.AddMinutes(-35),

                "ESP32-PWR-07" =>
                    DateTime.UtcNow.AddMinutes(-27),

                "ESP32-ACT-06" =>
                    DateTime.UtcNow.AddMinutes(-22),

                _ =>
                    DateTime.UtcNow
            };
        }

        private void DetectDisconnectedSensors(
            Dictionary<string, SensorRegistration> sensors)
        {
            Dictionary<string, DateTime> latestTelemetry =
                new(
                    StringComparer.OrdinalIgnoreCase);

            foreach (
                TelemetryPacket<float> packet
                in _telemetryService.GetFloatTelemetry())
            {
                RecordLatestTimestamp(
                    latestTelemetry,
                    packet.DeviceIdentifier,
                    packet.TimestampUtc);
            }

            foreach (
                TelemetryPacket<int> packet
                in _telemetryService.GetIntegerTelemetry())
            {
                RecordLatestTimestamp(
                    latestTelemetry,
                    packet.DeviceIdentifier,
                    packet.TimestampUtc);
            }

            foreach (
                TelemetryPacket<bool> packet
                in _telemetryService.GetBooleanTelemetry())
            {
                RecordLatestTimestamp(
                    latestTelemetry,
                    packet.DeviceIdentifier,
                    packet.TimestampUtc);
            }

            DateTime evaluationTimeUtc =
                DateTime.UtcNow;

            foreach (
                SensorRegistration sensor
                in sensors.Values)
            {
                if (!latestTelemetry.TryGetValue(
                    sensor.DeviceIdentifier,
                    out DateTime lastSeenUtc))
                {
                    _alertService.EvaluateDisconnect(
                        sensor.DeviceIdentifier,
                        sensor.DeploymentLocation,
                        null,
                        evaluationTimeUtc,
                        DisconnectThreshold);

                    continue;
                }

                _alertService.EvaluateDisconnect(
                    sensor.DeviceIdentifier,
                    sensor.DeploymentLocation,
                    lastSeenUtc,
                    evaluationTimeUtc,
                    DisconnectThreshold);
            }
        }

        private static void RecordLatestTimestamp(
            Dictionary<string, DateTime> latestTelemetry,
            string deviceIdentifier,
            DateTime timestampUtc)
        {
            if (!latestTelemetry.TryGetValue(
                deviceIdentifier,
                out DateTime existingTimestamp)
                ||
                timestampUtc > existingTimestamp)
            {
                latestTelemetry[deviceIdentifier] =
                    timestampUtc;
            }
        }

        private float GenerateFloatValue(
            string metricName,
            int index)
        {
            return metricName switch
            {
                "Soil Moisture" =>
                    index % 90 == 0
                        ? _random.Next(5, 15)
                        : (float)(
                            _random.NextDouble() * 30 + 45),

                "Temperature" =>
                    index % 110 == 0
                        ? _random.Next(40, 50)
                        : (float)(
                            _random.NextDouble() * 12 + 18),

                "Humidity" =>
                    index % 100 == 0
                        ? _random.Next(10, 20)
                        : (float)(
                            _random.NextDouble() * 35 + 45),

                _ => 0
            };
        }

        private int GenerateIntegerValue(
            string metricName,
            int index)
        {
            return metricName switch
            {
                "Power Draw" =>
                    index % 90 == 0
                        ? _random.Next(5000, 8000)
                        : _random.Next(800, 2500),

                "Energy Usage" =>
                    index % 120 == 0
                        ? _random.Next(6000, 9000)
                        : _random.Next(1000, 4000),

                "Flow Rate" =>
                    index % 100 == 0
                        ? _random.Next(400, 600)
                        : _random.Next(100, 300),

                _ => 0
            };
        }
    }
}