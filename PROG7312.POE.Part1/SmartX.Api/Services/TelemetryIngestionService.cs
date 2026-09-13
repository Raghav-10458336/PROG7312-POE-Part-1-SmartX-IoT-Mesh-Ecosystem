using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryIngestionService
    {
        private readonly TelemetryBuffer<TelemetryPacket<float>>
            _floatTelemetry = new();

        private readonly TelemetryBuffer<TelemetryPacket<int>>
            _integerTelemetry = new();

        private readonly TelemetryBuffer<TelemetryPacket<bool>>
            _booleanTelemetry = new();

        private readonly object _floatLock = new();
        private readonly object _integerLock = new();
        private readonly object _booleanLock = new();

        private readonly TelemetryAlertService _alertService;

        public TelemetryIngestionService(
            TelemetryAlertService alertService)
        {
            _alertService = alertService;
        }

        public void IngestFloat(
            TelemetryPacket<float> packet)
        {
            lock (_floatLock)
            {
                _floatTelemetry.Add(packet);
            }

            _alertService.EvaluateFloat(packet);
        }

        public void IngestInteger(
            TelemetryPacket<int> packet)
        {
            lock (_integerLock)
            {
                _integerTelemetry.Add(packet);
            }

            _alertService.EvaluateInteger(packet);
        }

        public void IngestBoolean(
            TelemetryPacket<bool> packet)
        {
            lock (_booleanLock)
            {
                _booleanTelemetry.Add(packet);
            }
        }

        public IReadOnlyList<TelemetryPacket<float>>
            GetFloatTelemetry()
        {
            lock (_floatLock)
            {
                return _floatTelemetry
                    .GetAll()
                    .ToList()
                    .AsReadOnly();
            }
        }

        public IReadOnlyList<TelemetryPacket<int>>
            GetIntegerTelemetry()
        {
            lock (_integerLock)
            {
                return _integerTelemetry
                    .GetAll()
                    .ToList()
                    .AsReadOnly();
            }
        }

        public IReadOnlyList<TelemetryPacket<bool>>
            GetBooleanTelemetry()
        {
            lock (_booleanLock)
            {
                return _booleanTelemetry
                    .GetAll()
                    .ToList()
                    .AsReadOnly();
            }
        }

        public int GetTotalPacketCount()
        {
            int floatCount;
            int integerCount;
            int booleanCount;

            lock (_floatLock)
            {
                floatCount =
                    _floatTelemetry.Count;
            }

            lock (_integerLock)
            {
                integerCount =
                    _integerTelemetry.Count;
            }

            lock (_booleanLock)
            {
                booleanCount =
                    _booleanTelemetry.Count;
            }

            return floatCount
                 + integerCount
                 + booleanCount;
        }
    }
}