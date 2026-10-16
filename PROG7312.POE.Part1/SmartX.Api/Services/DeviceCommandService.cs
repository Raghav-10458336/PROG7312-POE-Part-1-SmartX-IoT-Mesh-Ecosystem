
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class DeviceCommandService
    {
        private readonly SensorRegistryService _sensorRegistry;

        // LIFO history supports undoing the most recent command.
        private readonly Stack<DeviceCommand> _history = new();

        // Simulated actuator states, separate from sensor registration.
        private readonly Dictionary<string, bool> _deviceStates =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _commandLock = new();

        public DeviceCommandService(SensorRegistryService sensorRegistry)
        {
            _sensorRegistry = sensorRegistry;
        }

        public DeviceCommand? Execute(
            string deviceIdentifier,
            string commandType,
            bool newState)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier) ||
                string.IsNullOrWhiteSpace(commandType))
            {
                return null;
            }

            deviceIdentifier = deviceIdentifier.Trim();
            commandType = commandType.Trim();

            if (commandType != "Pump" && commandType != "Relay")
            {
                return null;
            }

            lock (_commandLock)
            {
                SensorRegistration? sensor =
                    _sensorRegistry.GetByDeviceIdentifier(deviceIdentifier);

                if (sensor is null || !sensor.IsActive)
                {
                    return null;
                }

                string stateKey = $"{deviceIdentifier}:{commandType}";

                bool previousState =
                    _deviceStates.GetValueOrDefault(stateKey);

                DeviceCommand command = new()
                {
                    DeviceIdentifier = deviceIdentifier,
                    CommandType = commandType,
                    PreviousState = previousState,
                    NewState = newState
                };

                _deviceStates[stateKey] = newState;
                _history.Push(command);

                return command;
            }
        }

        public IReadOnlyList<DeviceCommand> GetHistory()
        {
            lock (_commandLock)
            {
                return _history.ToList().AsReadOnly();
            }
        }

        public DeviceCommand? UndoLast()
        {
            lock (_commandLock)
            {
                if (_history.Count == 0)
                {
                    return null;
                }

                DeviceCommand command = _history.Pop();

                string stateKey =
                    $"{command.DeviceIdentifier}:{command.CommandType}";

                _deviceStates[stateKey] = command.PreviousState;
                command.IsUndone = true;

                return command;
            }
        }

        public bool GetState(string deviceIdentifier, string commandType)
        {
            lock (_commandLock)
            {
                string stateKey = $"{deviceIdentifier}:{commandType}";
                return _deviceStates.GetValueOrDefault(stateKey);
            }
        }
    }
}
