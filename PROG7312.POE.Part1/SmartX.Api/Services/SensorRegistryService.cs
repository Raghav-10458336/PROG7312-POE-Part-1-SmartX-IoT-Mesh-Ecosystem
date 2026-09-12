using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class SensorRegistryService
    {
        private readonly List<SensorRegistration> _sensors = new();

        public IReadOnlyList<SensorRegistration> GetAll()
        {
            return _sensors.AsReadOnly();
        }

        public SensorRegistration? GetById(Guid id)
        {
            return _sensors.FirstOrDefault(sensor => sensor.Id == id);
        }

        public SensorRegistration? GetByDeviceIdentifier(string deviceIdentifier)
        {
            return _sensors.FirstOrDefault(sensor =>
                sensor.DeviceIdentifier.Equals(
                    deviceIdentifier,
                    StringComparison.OrdinalIgnoreCase));
        }

        public SensorRegistration Register(SensorRegistration sensor)
        {
            _sensors.Add(sensor);
            return sensor;
        }

        public bool Remove(Guid id)
        {
            SensorRegistration? sensor = GetById(id);

            if (sensor is null)
            {
                return false;
            }

            return _sensors.Remove(sensor);
        }
    }
}