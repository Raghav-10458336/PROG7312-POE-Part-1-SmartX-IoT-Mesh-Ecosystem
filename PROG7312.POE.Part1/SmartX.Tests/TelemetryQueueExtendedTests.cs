
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartX.Api.Services;
using SmartX.Shared.Models;
using SmartX.Shared.Telemetry;

namespace SmartX.Tests;

[TestClass]
public class TelemetryQueueExtendedTests
{
    private static TelemetryPacket<float> Float(
        string metric, float value) =>
        new() { MetricName = metric, Value = value };

    private static TelemetryPacket<int> Integer(
        string metric, int value) =>
        new() { MetricName = metric, Value = value };

    [TestMethod]
    public void EmptyQueue_ReturnsFalseAndNull()
    {
        var queue = new TelemetryQueueService();

        Assert.IsFalse(queue.TryDequeue(out object? item));
        Assert.IsNull(item);
    }

    [TestMethod]
    public void EmptyQueue_CountsAreZero()
    {
        var queue = new TelemetryQueueService();

        Assert.AreEqual(0, queue.NormalQueueCount);
        Assert.AreEqual(0, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void NormalFloat_IncrementsNormalCount()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Temperature", 25));

        Assert.AreEqual(1, queue.NormalQueueCount);
    }

    [TestMethod]
    public void CriticalFloat_IncrementsPriorityCount()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Temperature", 40));

        Assert.AreEqual(1, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void WarningFloat_IncrementsPriorityCount()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Humidity", 20));

        Assert.AreEqual(1, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void CriticalInteger_PrecedesNormalFloat()
    {
        var queue = new TelemetryQueueService();
        var normal = Float("Temperature", 20);
        var critical = Integer("Power Draw", 5000);

        queue.EnqueueFloat(normal);
        queue.EnqueueInteger(critical);

        queue.TryDequeue(out object? first);

        Assert.AreSame(critical, first);
    }

    [TestMethod]
    public void WarningInteger_PrecedesNormalFloat()
    {
        var queue = new TelemetryQueueService();
        var normal = Float("Temperature", 20);
        var warning = Integer("Flow Rate", 400);

        queue.EnqueueFloat(normal);
        queue.EnqueueInteger(warning);

        queue.TryDequeue(out object? first);

        Assert.AreSame(warning, first);
    }

    [TestMethod]
    public void NormalBoolean_UsesFIFO()
    {
        var queue = new TelemetryQueueService();

        var firstPacket = new TelemetryPacket<bool> { Value = true };
        var secondPacket = new TelemetryPacket<bool> { Value = false };

        queue.EnqueueBoolean(firstPacket);
        queue.EnqueueBoolean(secondPacket);

        queue.TryDequeue(out object? first);
        queue.TryDequeue(out object? second);

        Assert.AreSame(firstPacket, first);
        Assert.AreSame(secondPacket, second);
    }

    [TestMethod]
    public void CriticalThenWarningThenNormal_UsesPriorityOrder()
    {
        var queue = new TelemetryQueueService();

        var normal = Float("Temperature", 20);
        var warning = Float("Humidity", 20);
        var critical = Float("Soil Moisture", 10);

        queue.EnqueueFloat(normal);
        queue.EnqueueFloat(warning);
        queue.EnqueueFloat(critical);

        queue.TryDequeue(out object? first);
        queue.TryDequeue(out object? second);
        queue.TryDequeue(out object? third);

        Assert.AreSame(critical, first);
        Assert.AreSame(warning, second);
        Assert.AreSame(normal, third);
    }

    [TestMethod]
    public void Dequeue_UpdatesCounts()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Temperature", 40));
        queue.EnqueueFloat(Float("Temperature", 20));
        queue.TryDequeue(out _);

        Assert.AreEqual(0, queue.PriorityQueueCount);
        Assert.AreEqual(1, queue.NormalQueueCount);
    }

    [TestMethod]
    public void FloatBoundary_38IsNotCritical()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Temperature", 38));

        Assert.AreEqual(0, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void FloatBoundary_20SoilIsNotCritical()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueFloat(Float("Soil Moisture", 20));

        Assert.AreEqual(0, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void IntegerBoundary_4500IsNotCritical()
    {
        var queue = new TelemetryQueueService();

        queue.EnqueueInteger(Integer("Power Draw", 4500));

        Assert.AreEqual(0, queue.PriorityQueueCount);
    }

    [TestMethod]
    public void NullFloat_IsRejected()
    {
        var queue = new TelemetryQueueService();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => queue.EnqueueFloat(null!));
    }

    [TestMethod]
    public void NullInteger_IsRejected()
    {
        var queue = new TelemetryQueueService();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => queue.EnqueueInteger(null!));
    }

    [TestMethod]
    public void NullBoolean_IsRejected()
    {
        var queue = new TelemetryQueueService();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => queue.EnqueueBoolean(null!));
    }
}
