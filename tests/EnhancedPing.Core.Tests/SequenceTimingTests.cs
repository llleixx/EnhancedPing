using EnhancedPing.Core;
using NUnit.Framework;

namespace EnhancedPing.Core.Tests;

[TestFixture]
public sealed class SequenceTimingTests
{
    [TestCase(5, 0.2f, 2f, 0.2f)]
    [TestCase(10, 0.2f, 2f, 0.2f)]
    [TestCase(20, 0.2f, 2f, 0.1f)]
    public void EffectivePointDuration_OnlyShortensWhenNecessary(
        int pointCount,
        float preferred,
        float maximum,
        float expected)
    {
        Assert.That(
            SequenceTiming.EffectivePointDuration(pointCount, preferred, maximum),
            Is.EqualTo(expected).Within(1e-6f));
    }

    [Test]
    public void EffectivePointDuration_KeepsFinalArrivalWithinMaximum()
    {
        const int pointCount = 20;
        const float maximum = 2f;
        float duration = SequenceTiming.EffectivePointDuration(pointCount, 0.3f, maximum);

        Assert.That((pointCount - 1) * duration, Is.LessThanOrEqualTo(maximum));
    }
}
