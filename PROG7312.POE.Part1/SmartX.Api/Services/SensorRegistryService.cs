
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class SensorRegistryService
    {
        // Maintain fast lookups using both device identifiers and sensor IDs.
        private readonly Dictionary<string, SensorRegistration>
            _sensorsByIdentifier =
                new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<Guid, SensorRegistration>
            _sensorsById = new();

        private readonly object _sensorLock = new();

        public IReadOnlyList<SensorRegistration> GetAll()
        {
            lock (_sensorLock)
            {
                return _sensorsById.Values
                    .ToList()
                    .AsReadOnly();
            }
        }

        public SensorRegistration? GetById(Guid id)
        {
            lock (_sensorLock)
            {
                return _sensorsById.TryGetValue(
                    id,
                    out SensorRegistration? sensor)
                    ? sensor
                    : null;
            }
        }

        public SensorRegistration? GetByDeviceIdentifier(
            string deviceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier))
            {
                return null;
            }

            lock (_sensorLock)
            {
                return _sensorsByIdentifier.TryGetValue(
                    deviceIdentifier,
                    out SensorRegistration? sensor)
                    ? sensor
                    : null;
            }
        }

        public SensorRegistration Register(
            SensorRegistration sensor)
        {
            ArgumentNullException.ThrowIfNull(sensor);

            if (string.IsNullOrWhiteSpace(sensor.DeviceIdentifier))
            {
                throw new ArgumentException(
                    "A valid device identifier is required.",
                    nameof(sensor));
            }

            lock (_sensorLock)
            {
                // Prevent duplicate identifiers and IDs during registration.
                if (_sensorsByIdentifier.ContainsKey(
                    sensor.DeviceIdentifier))
                {
                    throw new InvalidOperationException(
                        "A sensor with this device identifier already exists.");
                }

                if (_sensorsById.ContainsKey(sensor.Id))
                {
                    throw new InvalidOperationException(
                        "A sensor with this ID already exists.");
                }

                _sensorsByIdentifier.Add(
                    sensor.DeviceIdentifier,
                    sensor);

                _sensorsById.Add(
                    sensor.Id,
                    sensor);

                return sensor;
            }
        }

        public bool Remove(Guid id)
        {
            lock (_sensorLock)
            {
                if (!_sensorsById.TryGetValue(
                    id,
                    out SensorRegistration? sensor))
                {
                    return false;
                }

                // Remove the sensor from both lookup dictionaries.
                _sensorsById.Remove(id);

                _sensorsByIdentifier.Remove(
                    sensor.DeviceIdentifier);

                return true;
            }
        }
    }
}
