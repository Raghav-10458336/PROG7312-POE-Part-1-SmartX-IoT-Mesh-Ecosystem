
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests;

[TestClass]
public class TelemetryAlertExtendedTests
{
    private static TelemetryPacket<float> Float(
        string metric, float value, string device = "A") =>
        new()
        {
            DeviceIdentifier = device,
            MetricName = metric,
            Value = value
        };

    private static TelemetryPacket<int> Integer(
        string metric, int value, string device = "A") =>
        new()
        {
            DeviceIdentifier = device,
            MetricName = metric,
            Value = value
        };

    [TestMethod]
    public void EmptyAlerts_HaveZeroCounts()
    {
        var alerts = new TelemetryAlertService();

        Assert.AreEqual(0, alerts.GetAlertCount());
        Assert.AreEqual(0, alerts.GetActiveFaultCount());
    }

    [TestMethod]
    public void CriticalTemperature_CreatesFault()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 39));

        Assert.AreEqual(1, alerts.GetActiveFaultCount());
        Assert.AreEqual("Critical", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void NormalTemperature_CreatesNoFault()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 38));

        Assert.AreEqual(0, alerts.GetAlertCount());
    }

    [TestMethod]
    public void RepeatedFault_IsDeduplicated()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 39));
        alerts.EvaluateFloat(Float("Temperature", 40));

        Assert.AreEqual(1, alerts.GetAlertCount());
        Assert.AreEqual(1, alerts.GetActiveFaultCount());
    }

    [TestMethod]
    public void Recovery_ClearsActiveFaultButKeepsHistory()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 39));
        alerts.EvaluateFloat(Float("Temperature", 20));

        Assert.AreEqual(0, alerts.GetActiveFaultCount());
        Assert.AreEqual(1, alerts.GetAlertCount());
    }

    [TestMethod]
    public void NewFaultAfterRecovery_CreatesNewAlert()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 39));
        alerts.EvaluateFloat(Float("Temperature", 20));
        alerts.EvaluateFloat(Float("Temperature", 40));

        Assert.AreEqual(2, alerts.GetAlertCount());
    }

    [TestMethod]
    public void DifferentDevices_HaveSeparateFaults()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 40, "A"));
        alerts.EvaluateFloat(Float("Temperature", 40, "B"));

        Assert.AreEqual(2, alerts.GetActiveFaultCount());
    }

    [TestMethod]
    public void HumidityBelow25_IsWarning()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Humidity", 24));

        Assert.AreEqual("Warning", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void SoilBelow20_IsCritical()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Soil Moisture", 19));

        Assert.AreEqual("Critical", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void HighPower_IsCritical()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateInteger(Integer("Power Draw", 4501));

        Assert.AreEqual("Critical", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void HighEnergy_IsWarning()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateInteger(Integer("Energy Usage", 5501));

        Assert.AreEqual("Warning", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void HighFlow_IsWarning()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateInteger(Integer("Flow Rate", 351));

        Assert.AreEqual("Warning", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void AcknowledgeUnknown_ReturnsFalse()
    {
        var alerts = new TelemetryAlertService();

        Assert.IsFalse(alerts.Acknowledge(Guid.NewGuid()));
    }

    [TestMethod]
    public void AcknowledgeExisting_MarksAlert()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 40));

        var alert = alerts.GetAll()[0];

        Assert.IsTrue(alerts.Acknowledge(alert.AlertId));
        Assert.IsTrue(alert.IsAcknowledged);
        Assert.IsNotNull(alert.AcknowledgedAtUtc);
    }

    [TestMethod]
    public void AcknowledgeTwice_IsIdempotent()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 40));

        var id = alerts.GetAll()[0].AlertId;

        Assert.IsTrue(alerts.Acknowledge(id));
        Assert.IsTrue(alerts.Acknowledge(id));
        Assert.AreEqual(0, alerts.GetUnacknowledgedCount());
    }

    [TestMethod]
    public void Acknowledge_DoesNotClearActiveFault()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateFloat(Float("Temperature", 40));
        alerts.Acknowledge(alerts.GetAll()[0].AlertId);

        Assert.AreEqual(1, alerts.GetActiveFaultCount());
    }

    [TestMethod]
    public void DisconnectWithoutTelemetry_CreatesCriticalAlert()
    {
        var alerts = new TelemetryAlertService();

        alerts.EvaluateDisconnect(
            "A", "Lab", null,
            DateTime.UtcNow, TimeSpan.FromMinutes(5));

        Assert.AreEqual(1, alerts.GetActiveFaultCount());
        Assert.AreEqual("Critical", alerts.GetAll()[0].Severity);
    }

    [TestMethod]
    public void RecentTelemetry_DoesNotCreateDisconnectFault()
    {
        var alerts = new TelemetryAlertService();
        var now = DateTime.UtcNow;

        alerts.EvaluateDisconnect(
            "A", "Lab", now.AddMinutes(-2),
            now, TimeSpan.FromMinutes(5));

        Assert.AreEqual(0, alerts.GetActiveFaultCount());
    }

    [TestMethod]
    public void DisconnectRecovery_ClearsFault()
    {
        var alerts = new TelemetryAlertService();
        var now = DateTime.UtcNow;

        alerts.EvaluateDisconnect(
            "A", "Lab", null,
            now, TimeSpan.FromMinutes(5));

        alerts.EvaluateDisconnect(
            "A", "Lab", now,
            now, TimeSpan.FromMinutes(5));

        Assert.AreEqual(0, alerts.GetActiveFaultCount());
    }
}
