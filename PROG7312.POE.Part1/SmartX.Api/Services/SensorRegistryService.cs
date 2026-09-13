using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class SensorRegistryService
    {
        private readonly List<SensorRegistration> _sensors =
            new();

        private readonly object _sensorLock =
            new();

        public IReadOnlyList<SensorRegistration> GetAll()
        {
            lock (_sensorLock)
            {
                return _sensors
                    .ToList()
                    .AsReadOnly();
            }
        }

        public SensorRegistration? GetById(
            Guid id)
        {
            lock (_sensorLock)
            {
                return _sensors.FirstOrDefault(
                    sensor =>
                        sensor.Id == id);
            }
        }

        public SensorRegistration? GetByDeviceIdentifier(
            string deviceIdentifier)
        {
            lock (_sensorLock)
            {
                return _sensors.FirstOrDefault(
                    sensor =>
                        sensor.DeviceIdentifier.Equals(
                            deviceIdentifier,
                            StringComparison.OrdinalIgnoreCase));
            }
        }

        public SensorRegistration Register(
            SensorRegistration sensor)
        {
            lock (_sensorLock)
            {
                _sensors.Add(sensor);

                return sensor;
            }
        }

        public bool Remove(
            Guid id)
        {
            lock (_sensorLock)
            {
                SensorRegistration? sensor =
                    _sensors.FirstOrDefault(
                        item =>
                            item.Id == id);

                if (sensor is null)
                {
                    return false;
                }

                return _sensors.Remove(sensor);
            }
        }
    }
}