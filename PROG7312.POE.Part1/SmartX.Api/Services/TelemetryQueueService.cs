
using SmartX.Shared.Telemetry;

namespace SmartX.Api.Services
{
    public class TelemetryQueueService
    {
        private readonly Queue<object> _normalQueue = new();

        private readonly PriorityQueue<object, int>
            _priorityQueue = new();

        private readonly object _queueLock = new();

        public int NormalQueueCount
        {
            get
            {
                lock (_queueLock)
                {
                    return _normalQueue.Count;
                }
            }
        }

        public int PriorityQueueCount
        {
            get
            {
                lock (_queueLock)
                {
                    return _priorityQueue.Count;
                }
            }
        }

        public void EnqueueFloat(TelemetryPacket<float> packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            int priority = GetFloatPriority(packet);
            Enqueue(packet, priority);
        }

        public void EnqueueInteger(TelemetryPacket<int> packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            int priority = GetIntegerPriority(packet);
            Enqueue(packet, priority);
        }

        public void EnqueueBoolean(TelemetryPacket<bool> packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            Enqueue(packet, 2);
        }

        public bool TryDequeue(out object? packet)
        {
            lock (_queueLock)
            {
                // Process critical and warning events before normal readings.
                if (_priorityQueue.TryDequeue(
                    out object? priorityPacket,
                    out _))
                {
                    packet = priorityPacket;
                    return true;
                }

                if (_normalQueue.Count > 0)
                {
                    packet = _normalQueue.Dequeue();
                    return true;
                }

                packet = null;
                return false;
            }
        }

        private void Enqueue(object packet, int priority)
        {
            lock (_queueLock)
            {
                if (priority < 2)
                {
                    _priorityQueue.Enqueue(packet, priority);
                }
                else
                {
                    _normalQueue.Enqueue(packet);
                }
            }
        }

        private static int GetFloatPriority(
            TelemetryPacket<float> packet)
        {
            if (packet.MetricName == "Soil Moisture"
                && packet.Value < 20)
            {
                return 0;
            }

            if (packet.MetricName == "Temperature"
                && packet.Value > 38)
            {
                return 0;
            }

            if (packet.MetricName == "Humidity"
                && packet.Value < 25)
            {
                return 1;
            }

            return 2;
        }

        private static int GetIntegerPriority(
            TelemetryPacket<int> packet)
        {
            if (packet.MetricName == "Power Draw"
                && packet.Value > 4500)
            {
                return 0;
            }

            if (packet.MetricName == "Energy Usage"
                && packet.Value > 5500)
            {
                return 1;
            }

            if (packet.MetricName == "Flow Rate"
                && packet.Value > 350)
            {
                return 1;
            }

            return 2;
        }
    }
}
