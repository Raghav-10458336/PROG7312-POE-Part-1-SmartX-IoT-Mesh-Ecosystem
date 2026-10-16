
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests;

[TestClass]
public class DeviceCommandExtendedTests
{
    private static DeviceCommandService Create()
    {
        var registry = new SensorRegistryService();

        registry.Register(new SensorRegistration
        {
            DeviceIdentifier = "A",
            DeploymentLocation = "Lab"
        });

        registry.Register(new SensorRegistration
        {
            DeviceIdentifier = "B",
            DeploymentLocation = "Lab"
        });

        return new DeviceCommandService(registry);
    }

    [DataTestMethod]
    [DataRow("Pump")]
    [DataRow("Relay")]
    [DataRow("Valve")]
    public void SupportedCommands_UpdateState(string type)
    {
        var service = Create();

        Assert.IsNotNull(service.Execute("A", type, true));
        Assert.IsTrue(service.GetState("A", type));
    }

    [DataTestMethod]
    [DataRow("Pump")]
    [DataRow("Relay")]
    [DataRow("Valve")]
    public void SupportedCommands_CanBeTurnedOff(string type)
    {
        var service = Create();

        service.Execute("A", type, true);
        service.Execute("A", type, false);

        Assert.IsFalse(service.GetState("A", type));
    }

    [TestMethod]
    public void Commands_AreCaseInsensitive()
    {
        var service = Create();

        Assert.IsNotNull(service.Execute("a", "pUmP", true));
        Assert.IsTrue(service.GetState("A", "PUMP"));
    }

    [TestMethod]
    public void Commands_TrimWhitespace()
    {
        var service = Create();

        Assert.IsNotNull(service.Execute(" A ", " Pump ", true));
        Assert.IsTrue(service.GetState("A", "Pump"));
    }

    [TestMethod]
    public void EmptyDevice_IsRejected()
    {
        var service = Create();
        Assert.IsNull(service.Execute("", "Pump", true));
    }

    [TestMethod]
    public void BlankCommand_IsRejected()
    {
        var service = Create();
        Assert.IsNull(service.Execute("A", " ", true));
    }

    [TestMethod]
    public void RejectedCommand_DoesNotChangeHistory()
    {
        var service = Create();

        service.Execute("A", "Pump", true);
        service.Execute("A", "Bad", false);

        Assert.AreEqual(1, service.GetHistory().Count);
    }

    [TestMethod]
    public void DeviceStates_AreIndependent()
    {
        var service = Create();

        service.Execute("A", "Pump", true);

        Assert.IsFalse(service.GetState("B", "Pump"));
    }

    [TestMethod]
    public void ActuatorStates_AreIndependent()
    {
        var service = Create();

        service.Execute("A", "Pump", true);

        Assert.IsFalse(service.GetState("A", "Valve"));
        Assert.IsFalse(service.GetState("A", "Relay"));
    }

    [TestMethod]
    public void MultipleUndos_RestoreOriginalStates()
    {
        var service = Create();

        service.Execute("A", "Pump", true);
        service.Execute("A", "Relay", true);
        service.Execute("A", "Valve", true);

        Assert.IsNotNull(service.UndoLast());
        Assert.IsNotNull(service.UndoLast());
        Assert.IsNotNull(service.UndoLast());

        Assert.IsFalse(service.GetState("A", "Pump"));
        Assert.IsFalse(service.GetState("A", "Relay"));
        Assert.IsFalse(service.GetState("A", "Valve"));
        Assert.AreEqual(0, service.GetHistory().Count);
    }

    [TestMethod]
    public void Undo_SecondUndoReturnsNull()
    {
        var service = Create();

        service.Execute("A", "Pump", true);
        service.UndoLast();

        Assert.IsNull(service.UndoLast());
    }

    [TestMethod]
    public void Undo_DoesNotChangeOtherDevice()
    {
        var service = Create();

        service.Execute("B", "Pump", true);
        service.Execute("A", "Relay", true);
        service.UndoLast();

        Assert.IsTrue(service.GetState("B", "Pump"));
    }

    [TestMethod]
    public void GetState_UnknownDeviceReturnsFalse()
    {
        var service = Create();

        Assert.IsFalse(service.GetState("UNKNOWN", "Pump"));
    }

    [TestMethod]
    public void GetState_BlankInputReturnsFalse()
    {
        var service = Create();

        Assert.IsFalse(service.GetState(" ", "Pump"));
    }

    [TestMethod]
    public void History_NewestFirstAcrossDevices()
    {
        var service = Create();

        service.Execute("A", "Pump", true);
        service.Execute("B", "Relay", true);

        Assert.AreEqual("B", service.GetHistory()[0].DeviceIdentifier);
    }

    [TestMethod]
    public void Execute_RecordsPreviousState()
    {
        var service = Create();

        service.Execute("A", "Pump", true);
        var command = service.Execute("A", "Pump", false);

        Assert.IsNotNull(command);
        Assert.IsTrue(command.PreviousState);
        Assert.IsFalse(command.NewState);
    }
}
