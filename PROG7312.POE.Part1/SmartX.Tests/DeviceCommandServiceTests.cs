
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Tests
{
    [TestClass]
    public class DeviceCommandServiceTests
    {
        private static DeviceCommandService CreateService()
        {
            var registry = new SensorRegistryService();

            registry.Register(new SensorRegistration
            {
                DeviceIdentifier = "TEST-DEVICE-01",
                DeploymentLocation = "Test Lab",
                IsActive = true
            });

            return new DeviceCommandService(registry);
        }

        [TestMethod]
        public void Execute_ValidPumpCommand_UpdatesState()
        {
            var service = CreateService();

            var command = service.Execute(
                "TEST-DEVICE-01", "Pump", true);

            Assert.IsNotNull(command);
            Assert.IsFalse(command.PreviousState);
            Assert.IsTrue(command.NewState);
            Assert.IsTrue(service.GetState(
                "TEST-DEVICE-01", "Pump"));
        }

        [TestMethod]
        public void Execute_UnknownDevice_IsRejected()
        {
            var service = CreateService();

            var command = service.Execute(
                "UNKNOWN", "Pump", true);

            Assert.IsNull(command);
            Assert.AreEqual(0, service.GetHistory().Count);
        }

        [TestMethod]
        public void Execute_InvalidCommandType_IsRejected()
        {
            var service = CreateService();

            var command = service.Execute(
                "TEST-DEVICE-01", "Restart", true);

            Assert.IsNull(command);
            Assert.AreEqual(0, service.GetHistory().Count);
        }

        [TestMethod]
        public void Execute_InactiveDevice_IsRejected()
        {
            var registry = new SensorRegistryService();

            registry.Register(new SensorRegistration
            {
                DeviceIdentifier = "INACTIVE-01",
                DeploymentLocation = "Test Lab",
                IsActive = false
            });

            var service = new DeviceCommandService(registry);

            Assert.IsNull(service.Execute(
                "INACTIVE-01", "Relay", true));
        }

        [TestMethod]
        public void History_UsesLastInFirstOutOrder()
        {
            var service = CreateService();

            service.Execute("TEST-DEVICE-01", "Pump", true);
            service.Execute("TEST-DEVICE-01", "Relay", true);

            var history = service.GetHistory();

            Assert.AreEqual(2, history.Count);
            Assert.AreEqual("Relay", history[0].CommandType);
            Assert.AreEqual("Pump", history[1].CommandType);
        }

        [TestMethod]
        public void Undo_ReversesLatestCommandOnly()
        {
            var service = CreateService();

            service.Execute("TEST-DEVICE-01", "Pump", true);
            service.Execute("TEST-DEVICE-01", "Relay", true);

            var undone = service.UndoLast();

            Assert.IsNotNull(undone);
            Assert.IsTrue(undone.IsUndone);

            Assert.IsFalse(service.GetState(
                "TEST-DEVICE-01", "Relay"));

            Assert.IsTrue(service.GetState(
                "TEST-DEVICE-01", "Pump"));

            Assert.AreEqual(1, service.GetHistory().Count);
        }

        [TestMethod]
        public void Undo_EmptyHistory_ReturnsNull()
        {
            var service = CreateService();

            Assert.IsNull(service.UndoLast());
        }

        [TestMethod]
        public void Undo_RestoresPreviousState()
        {
            var service = CreateService();

            service.Execute("TEST-DEVICE-01", "Pump", true);
            service.Execute("TEST-DEVICE-01", "Pump", false);

            service.UndoLast();

            Assert.IsTrue(service.GetState(
                "TEST-DEVICE-01", "Pump"));
        }
    }
}
