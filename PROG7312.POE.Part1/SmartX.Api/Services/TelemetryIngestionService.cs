using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryIngestionService
    {
        private readonly TelemetryBuffer<TelemetryPacket<float>> _floatTelemetry = new();
        private readonly TelemetryBuffer<TelemetryPacket<int>> _integerTelemetry = new();
        private readonly TelemetryBuffer<TelemetryPacket<bool>> _booleanTelemetry = new();

        private readonly TelemetryAlertService _alertService;

        public TelemetryIngestionService(TelemetryAlertService alertService)
        {
            _alertService = alertService;
        }

        public void IngestFloat(TelemetryPacket<float> packet)
        {
            _floatTelemetry.Add(packet);
            _alertService.EvaluateFloat(packet);
        }

        public void IngestInteger(TelemetryPacket<int> packet)
        {
            _integerTelemetry.Add(packet);
            _alertService.EvaluateInteger(packet);
        }

        public void IngestBoolean(TelemetryPacket<bool> packet)
        {
            _booleanTelemetry.Add(packet);
        }

        public IReadOnlyList<TelemetryPacket<float>> GetFloatTelemetry()
        {
            return _floatTelemetry.GetAll();
        }

        public IReadOnlyList<TelemetryPacket<int>> GetIntegerTelemetry()
        {
            return _integerTelemetry.GetAll();
        }

        public IReadOnlyList<TelemetryPacket<bool>> GetBooleanTelemetry()
        {
            return _booleanTelemetry.GetAll();
        }

        public int GetTotalPacketCount()
        {
            return _floatTelemetry.Count
                 + _integerTelemetry.Count
                 + _booleanTelemetry.Count;
        }
    }
}