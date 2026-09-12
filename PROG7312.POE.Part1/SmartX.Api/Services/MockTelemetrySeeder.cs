using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class MockTelemetrySeeder
    {
        private readonly TelemetryIngestionService _telemetryService;
        private readonly Random _random = new();

        public MockTelemetrySeeder(TelemetryIngestionService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        public void Seed()
        {
            SeedFloatTelemetry();
            SeedIntegerTelemetry();
            SeedBooleanTelemetry();
        }

        private void SeedFloatTelemetry()
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

            for (int batchIndex = 0; batchIndex < batches.Length; batchIndex++)
            {
                int batchSize = 300;
                batches[batchIndex] =
                    new TelemetryPacket<float>[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex = i % metricNames.Length;

                    float value = GenerateFloatValue(
                        metricNames[metricIndex],
                        i);

                    batches[batchIndex][i] =
                        new TelemetryPacket<float>(
                            Guid.NewGuid(),
                            $"ESP32-ENV-{batchIndex + 1:D2}",
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);
                }
            }

            TelemetryBatch<TelemetryPacket<float>> telemetryBatch =
                new(batches);

            foreach (TelemetryPacket<float> packet in telemetryBatch.ToList())
            {
                _telemetryService.IngestFloat(packet);
            }
        }

        private void SeedIntegerTelemetry()
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

            for (int batchIndex = 0; batchIndex < batches.Length; batchIndex++)
            {
                int batchSize = 250;
                batches[batchIndex] =
                    new TelemetryPacket<int>[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex = i % metricNames.Length;

                    int value = GenerateIntegerValue(
                        metricNames[metricIndex],
                        i);

                    batches[batchIndex][i] =
                        new TelemetryPacket<int>(
                            Guid.NewGuid(),
                            $"ESP32-PWR-{batchIndex + 1:D2}",
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);
                }
            }

            TelemetryBatch<TelemetryPacket<int>> telemetryBatch =
                new(batches);

            foreach (TelemetryPacket<int> packet in telemetryBatch.ToList())
            {
                _telemetryService.IngestInteger(packet);
            }
        }

        private void SeedBooleanTelemetry()
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

            for (int batchIndex = 0; batchIndex < batches.Length; batchIndex++)
            {
                int batchSize = 200;
                batches[batchIndex] =
                    new TelemetryPacket<bool>[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    int metricIndex = i % metricNames.Length;

                    bool value = _random.Next(0, 2) == 1;

                    batches[batchIndex][i] =
                        new TelemetryPacket<bool>(
                            Guid.NewGuid(),
                            $"ESP32-ACT-{batchIndex + 1:D2}",
                            metricNames[metricIndex],
                            units[metricIndex],
                            value);
                }
            }

            TelemetryBatch<TelemetryPacket<bool>> telemetryBatch =
                new(batches);

            foreach (TelemetryPacket<bool> packet in telemetryBatch.ToList())
            {
                _telemetryService.IngestBoolean(packet);
            }
        }

        private float GenerateFloatValue(string metricName, int index)
        {
            return metricName switch
            {
                "Soil Moisture" =>
                    index % 90 == 0
                        ? _random.Next(5, 15)
                        : (float)(_random.NextDouble() * 30 + 45),

                "Temperature" =>
                    index % 110 == 0
                        ? _random.Next(40, 50)
                        : (float)(_random.NextDouble() * 12 + 18),

                "Humidity" =>
                    index % 100 == 0
                        ? _random.Next(10, 20)
                        : (float)(_random.NextDouble() * 35 + 45),

                _ => 0
            };
        }

        private int GenerateIntegerValue(string metricName, int index)
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