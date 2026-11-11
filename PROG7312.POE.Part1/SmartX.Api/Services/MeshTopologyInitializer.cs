
using SmartX.Shared.Models;

namespace SmartX.Api.Services
{
    public class MeshTopologyInitializer
    {
        private readonly SensorRegistryService _sensorRegistry;
        private readonly MeshNetworkService _meshNetwork;

        private const string GatewayId = "SMARTX-GATEWAY";

        public MeshTopologyInitializer(
            SensorRegistryService sensorRegistry,
            MeshNetworkService meshNetwork)
        {
            _sensorRegistry = sensorRegistry;
            _meshNetwork = meshNetwork;
        }

        // Build the simulated mesh from registered Smart-X sensors.
        public void Initialize()
        {
            var sensors = _sensorRegistry.GetAll()
                .OrderBy(s => s.DeviceIdentifier,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            _meshNetwork.AddOrUpdateNode(new MeshNode
            {
                Id = GatewayId,
                Label = "Smart-X Gateway",
                Type = MeshNodeType.Gateway,
                DeploymentLocation = "Central Network Gateway",
                IsOnline = true,
                X = 50,
                Y = 50
            });

            if (sensors.Count == 0)
                return;

            // Position sensors around the gateway.
            for (int i = 0; i < sensors.Count; i++)
            {
                var sensor = sensors[i];

                double angle = 2 * Math.PI * i / sensors.Count;

                // Alternate between inner and outer rings.
                double radius = i % 2 == 0 ? 27 : 40;

                double x = 50 + radius * Math.Cos(angle);
                double y = 50 + radius * Math.Sin(angle);

                _meshNetwork.AddOrUpdateNode(new MeshNode
                {
                    Id = sensor.DeviceIdentifier,
                    Label = sensor.DeviceIdentifier,
                    Type = MeshNodeType.Sensor,
                    DeploymentLocation = sensor.DeploymentLocation,
                    IsOnline = sensor.IsActive,
                    X = Math.Round(x, 2),
                    Y = Math.Round(y, 2)
                });
            }

            // Connect neighbouring sensors into a ring.
            if (sensors.Count > 1)
            {
                for (int i = 0; i < sensors.Count; i++)
                {
                    int next = (i + 1) % sensors.Count;

                    AddLink(
                        sensors[i].DeviceIdentifier,
                        sensors[next].DeviceIdentifier,
                        2.0);
                }
            }

            // Add alternative routes across the mesh.
            if (sensors.Count > 3)
            {
                for (int i = 0; i < sensors.Count; i++)
                {
                    int next = (i + 2) % sensors.Count;

                    AddLink(
                        sensors[i].DeviceIdentifier,
                        sensors[next].DeviceIdentifier,
                        3.5);
                }
            }

            // Connect selected sensors directly to the gateway.
            int gatewaySpacing = Math.Max(1, sensors.Count / 4);

            for (int i = 0; i < sensors.Count; i += gatewaySpacing)
            {
                AddLink(
                    GatewayId,
                    sensors[i].DeviceIdentifier,
                    1.5 + (i % 3) * 0.5);
            }
        }

        private void AddLink(
            string sourceId,
            string targetId,
            double weight)
        {
            if (string.Equals(
                sourceId,
                targetId,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _meshNetwork.AddOrUpdateLink(new MeshLink
            {
                SourceId = sourceId,
                TargetId = targetId,
                Weight = weight,
                IsEnabled = true
            });
        }
    }
}
