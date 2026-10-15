
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryProcessingWorker : BackgroundService
    {
        private readonly TelemetryQueueService _queueService;
        private readonly TelemetryIngestionService _ingestionService;
        private readonly ILogger<TelemetryProcessingWorker> _logger;

        public TelemetryProcessingWorker(
            TelemetryQueueService queueService,
            TelemetryIngestionService ingestionService,
            ILogger<TelemetryProcessingWorker> logger)
        {
            _queueService = queueService;
            _ingestionService = ingestionService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Smart-X telemetry processing worker started.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Process a bounded batch before checking for new work.
                    int processed = 0;

                    while (processed < 100 &&
                           _queueService.TryDequeue(out object? packet))
                    {
                        try
                        {
                            ProcessPacket(packet);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to process a telemetry packet.");
                        }

                        processed++;
                    }

                    await Task.Delay(50, stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown when the application stops.
            }

            _logger.LogInformation(
                "Smart-X telemetry processing worker stopped.");
        }

        private void ProcessPacket(object? packet)
        {
            switch (packet)
            {
                case TelemetryPacket<float> floatPacket:
                    _ingestionService.IngestFloat(floatPacket);
                    break;

                case TelemetryPacket<int> integerPacket:
                    _ingestionService.IngestInteger(integerPacket);
                    break;

                case TelemetryPacket<bool> booleanPacket:
                    _ingestionService.IngestBoolean(booleanPacket);
                    break;

                default:
                    _logger.LogWarning(
                        "Unsupported telemetry packet type.");
                    break;
            }
        }
    }
}
