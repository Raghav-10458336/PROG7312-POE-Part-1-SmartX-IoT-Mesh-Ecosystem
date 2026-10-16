
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests;

[TestClass]
public class SensorRegistryExtendedTests
{
    private static SensorRegistration Sensor(string id) =>
        new()
        {
            DeviceIdentifier = id,
            DeploymentLocation = "Lab"
        };

    [TestMethod]
    public void EmptyRegistry_HasNoSensors()
    {
        var registry = new SensorRegistryService();

        Assert.AreEqual(0, registry.GetAll().Count);
    }

    [TestMethod]
    public void Register_ReturnsSameSensor()
    {
        var registry = new SensorRegistryService();
        var sensor = Sensor("A");

        Assert.AreSame(sensor, registry.Register(sensor));
    }

    [TestMethod]
    public void Register_CanFindById()
    {
        var registry = new SensorRegistryService();
        var sensor = registry.Register(Sensor("A"));

        Assert.AreSame(sensor, registry.GetById(sensor.Id));
    }

    [TestMethod]
    public void Register_CanFindByIdentifier()
    {
        var registry = new SensorRegistryService();
        var sensor = registry.Register(Sensor("A"));

        Assert.AreSame(sensor,
            registry.GetByDeviceIdentifier("A"));
    }

    [TestMethod]
    public void IdentifierLookup_IsCaseInsensitive()
    {
        var registry = new SensorRegistryService();

        registry.Register(Sensor("Device-A"));

        Assert.IsNotNull(
            registry.GetByDeviceIdentifier("device-a"));
    }

    [TestMethod]
    public void DuplicateIdentifier_IsRejected()
    {
        var registry = new SensorRegistryService();

        registry.Register(Sensor("A"));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => registry.Register(Sensor("a")));
    }

    [TestMethod]
    public void DuplicateGuid_IsRejected()
    {
        var registry = new SensorRegistryService();
        var first = registry.Register(Sensor("A"));
        var second = Sensor("B");

        second.Id = first.Id;

        Assert.ThrowsExactly<InvalidOperationException>(
            () => registry.Register(second));
    }

    [TestMethod]
    public void BlankIdentifier_IsRejected()
    {
        var registry = new SensorRegistryService();

        Assert.ThrowsExactly<ArgumentException>(
            () => registry.Register(Sensor(" ")));
    }

    [TestMethod]
    public void NullRegistration_IsRejected()
    {
        var registry = new SensorRegistryService();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.Register(null!));
    }

    [TestMethod]
    public void RemoveExisting_RemovesBothIndexes()
    {
        var registry = new SensorRegistryService();
        var sensor = registry.Register(Sensor("A"));

        Assert.IsTrue(registry.Remove(sensor.Id));
        Assert.IsNull(registry.GetById(sensor.Id));
        Assert.IsNull(registry.GetByDeviceIdentifier("A"));
    }

    [TestMethod]
    public void RemoveMissing_ReturnsFalse()
    {
        var registry = new SensorRegistryService();

        Assert.IsFalse(registry.Remove(Guid.NewGuid()));
    }

    [TestMethod]
    public void Remove_AllowsReregisteringIdentifier()
    {
        var registry = new SensorRegistryService();
        var sensor = registry.Register(Sensor("A"));

        registry.Remove(sensor.Id);

        Assert.IsNotNull(registry.Register(Sensor("A")));
    }

    [TestMethod]
    public void MultipleRegistrations_AreAllListed()
    {
        var registry = new SensorRegistryService();

        registry.Register(Sensor("A"));
        registry.Register(Sensor("B"));

        Assert.AreEqual(2, registry.GetAll().Count);
    }

    [TestMethod]
    public void MissingIdentifier_ReturnsNull()
    {
        var registry = new SensorRegistryService();

        Assert.IsNull(
            registry.GetByDeviceIdentifier("UNKNOWN"));
    }
}
