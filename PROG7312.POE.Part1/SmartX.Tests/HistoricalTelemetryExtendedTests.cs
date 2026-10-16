
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests;

[TestClass]
public class HistoricalTelemetryExtendedTests
{
    private static readonly DateTime BaseTime =
        new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    private static TelemetryPacket<float> Float(
        string device, string metric, int minutes,
        float value = 1) =>
        new()
        {
            DeviceIdentifier = device,
            MetricName = metric,
            TimestampUtc = BaseTime.AddMinutes(minutes),
            Value = value
        };

    [TestMethod]
    public void EmptyHistory_HasZeroCount()
    {
        var history = new HistoricalTelemetryService();

        Assert.AreEqual(0, history.GetTotalCount());
        Assert.AreEqual(0, history.GetAll().Count);
    }

    [TestMethod]
    public void MixedTypes_ArePreserved()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", 0));

        history.AddInteger(new TelemetryPacket<int>
        {
            DeviceIdentifier = "B",
            TimestampUtc = BaseTime
        });

        history.AddBoolean(new TelemetryPacket<bool>
        {
            DeviceIdentifier = "C",
            TimestampUtc = BaseTime
        });

        Assert.AreEqual(3, history.GetTotalCount());
    }

    [TestMethod]
    public void Range_IncludesStartBoundary()
    {
        var history = new HistoricalTelemetryService();
        var packet = Float("A", "Temp", 0);

        history.AddFloat(packet);

        Assert.AreSame(packet,
            history.GetByTimeRange(
                BaseTime, BaseTime.AddHours(1))[0]);
    }

    [TestMethod]
    public void Range_IncludesEndBoundary()
    {
        var history = new HistoricalTelemetryService();
        var packet = Float("A", "Temp", 0);

        history.AddFloat(packet);

        Assert.AreSame(packet,
            history.GetByTimeRange(
                BaseTime.AddHours(-1), BaseTime)[0]);
    }

    [TestMethod]
    public void Range_ExcludesEarlierPackets()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", -1));

        Assert.AreEqual(0,
            history.GetByTimeRange(
                BaseTime, BaseTime.AddHours(1)).Count);
    }

    [TestMethod]
    public void Range_ExcludesLaterPackets()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", 1));

        Assert.AreEqual(0,
            history.GetByTimeRange(
                BaseTime.AddHours(-1), BaseTime).Count);
    }

    [TestMethod]
    public void DeviceLookup_IsCaseInsensitive()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("Device-A", "Temp", 0));

        Assert.AreEqual(1,
            history.GetByDevice("device-a").Count);
    }

    [TestMethod]
    public void DeviceLookup_UnknownReturnsEmpty()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", 0));

        Assert.AreEqual(0, history.GetByDevice("B").Count);
    }

    [TestMethod]
    public void Recent_ReturnsNewestFirst()
    {
        var history = new HistoricalTelemetryService();

        var old = Float("A", "Temp", -1);
        var newer = Float("A", "Temp", 1);

        history.AddFloat(old);
        history.AddFloat(newer);

        var results = history.GetRecent(
            BaseTime.AddHours(-1),
            BaseTime.AddHours(1), 10);

        Assert.AreSame(newer, results[0]);
        Assert.AreSame(old, results[1]);
    }

    [TestMethod]
    public void Recent_LimitIsRespected()
    {
        var history = new HistoricalTelemetryService();

        for (int i = 0; i < 10; i++)
            history.AddFloat(Float("A", "Temp", i));

        Assert.AreEqual(3,
            history.GetRecent(
                BaseTime, BaseTime.AddHours(1), 3).Count);
    }

    [TestMethod]
    public void Recent_FiltersBeforeLimiting()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("MATCH", "Temp", -1));

        for (int i = 0; i < 10; i++)
            history.AddFloat(Float("OTHER", "Temp", i));

        Assert.AreEqual(1,
            history.GetRecent(
                BaseTime.AddHours(-1),
                BaseTime.AddHours(1), 1, "MATCH").Count);
    }

    [TestMethod]
    public void Recent_MetricFilterIsCaseInsensitive()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Soil Moisture", 0));
        history.AddFloat(Float("A", "Temperature", 1));

        Assert.AreEqual(1,
            history.GetRecent(
                BaseTime.AddHours(-1),
                BaseTime.AddHours(1), 10,
                metric: "soil").Count);
    }

    [TestMethod]
    public void Recent_DeviceFilterAllowsPartialMatch()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("ESP32-ACT-01", "Temp", 0));

        Assert.AreEqual(1,
            history.GetRecent(
                BaseTime.AddHours(-1),
                BaseTime.AddHours(1), 10, "ACT").Count);
    }

    [TestMethod]
    public void Recent_ClampsZeroLimitToOne()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", 0));

        Assert.AreEqual(1,
            history.GetRecent(
                BaseTime.AddHours(-1),
                BaseTime.AddHours(1), 0).Count);
    }

    [TestMethod]
    public void SameTimestamp_PreservesAllPackets()
    {
        var history = new HistoricalTelemetryService();

        history.AddFloat(Float("A", "Temp", 0));
        history.AddFloat(Float("B", "Temp", 0));

        Assert.AreEqual(2,
            history.GetRecent(
                BaseTime, BaseTime, 10).Count);
    }
}
