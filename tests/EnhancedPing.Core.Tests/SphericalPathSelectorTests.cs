using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EnhancedPing.Core;
using NUnit.Framework;

namespace EnhancedPing.Core.Tests;

[TestFixture]
public sealed class SphericalPathSelectorTests
{
    [Test]
    public void Select_PreservesEndpointsAndMaximumCount()
    {
        List<SphericalPathSample> samples = Enumerable.Range(0, 31)
            .Select(index => SampleAtYaw(index))
            .ToList();

        IReadOnlyList<SphericalPathSample> selected = SphericalPathSelector.Select(samples, 10);

        Assert.That(selected, Has.Count.EqualTo(10));
        Assert.That(selected[0].Point.X, Is.EqualTo(0f));
        Assert.That(selected[selected.Count - 1].Point.X, Is.EqualTo(30f));
    }

    [Test]
    public void Select_UsesApproximatelyEqualCumulativeAngles()
    {
        List<SphericalPathSample> samples = Enumerable.Range(0, 101)
            .Select(index => SampleAtYaw(index * 0.5f))
            .ToList();

        IReadOnlyList<SphericalPathSample> selected = SphericalPathSelector.Select(samples, 6);
        float[] yaws = selected.Select(sample => sample.Point.X).ToArray();

        Assert.That(yaws, Is.EqualTo(new[] { 0f, 10f, 20f, 30f, 40f, 50f }).Within(0.51f));
    }

    [Test]
    public void Select_ProducesStrictlyOrderedUniqueSamplesAcrossLargeGap()
    {
        float[] yaws = { 0f, 1f, 2f, 3f, 80f, 81f, 82f, 83f, 84f, 85f, 86f, 87f };
        List<SphericalPathSample> samples = yaws.Select(yaw => SampleAtYaw(yaw)).ToList();

        IReadOnlyList<SphericalPathSample> selected = SphericalPathSelector.Select(samples, 10);
        float[] selectedYaws = selected.Select(sample => sample.Point.X).ToArray();

        Assert.That(selectedYaws, Is.Ordered.Ascending);
        Assert.That(selectedYaws.Distinct().Count(), Is.EqualTo(selectedYaws.Length));
        Assert.That(selectedYaws[0], Is.EqualTo(0f));
        Assert.That(selectedYaws[selectedYaws.Length - 1], Is.EqualTo(87f));
    }

    [Test]
    public void Select_IgnoresInvalidAndConsecutiveDuplicateDirections()
    {
        List<SphericalPathSample> samples = new()
        {
            SampleAtYaw(0f),
            SampleAtYaw(0f),
            new SphericalPathSample(new Vector3(float.NaN, 0f, 1f), Vector3.Zero, Vector3.UnitY),
            SampleAtYaw(5f)
        };

        IReadOnlyList<SphericalPathSample> selected = SphericalPathSelector.Select(samples, 10);

        Assert.That(selected, Has.Count.EqualTo(2));
        Assert.That(selected[0].Point.X, Is.EqualTo(0f));
        Assert.That(selected[1].Point.X, Is.EqualTo(5f));
    }

    [Test]
    public void AngularSelectionIsIndependentOfTerrainDistance()
    {
        List<SphericalPathSample> near = Enumerable.Range(0, 21)
            .Select(index => SampleAtYaw(index, 5f))
            .ToList();
        List<SphericalPathSample> far = Enumerable.Range(0, 21)
            .Select(index => SampleAtYaw(index, 500f))
            .ToList();

        IReadOnlyList<SphericalPathSample> selectedNear = SphericalPathSelector.Select(near, 10);
        IReadOnlyList<SphericalPathSample> selectedFar = SphericalPathSelector.Select(far, 10);

        Assert.That(
            selectedFar.Select(sample => sample.Point.X).ToArray(),
            Is.EqualTo(selectedNear.Select(sample => sample.Point.X).ToArray()));
    }

    [Test]
    public void AngleDegrees_ReturnsExpectedSeparation()
    {
        float angle = SphericalPathSelector.AngleDegrees(Vector3.UnitZ, Vector3.UnitX);
        Assert.That(angle, Is.EqualTo(90f).Within(1e-4f));
    }

    [Test]
    public void Select_ReportsTheWholeCurvedGestureAngle()
    {
        List<SphericalPathSample> samples = new()
        {
            SampleAtYaw(0f),
            SampleAtYaw(2f),
            SampleAtYaw(0f)
        };

        SphericalPathSelector.Select(samples, 10, out float totalAngle);
        Assert.That(totalAngle, Is.EqualTo(4f).Within(1e-3f));
    }

    [Test]
    public void Select_ReportsAngleIndependentOfTerrainDistance()
    {
        List<SphericalPathSample> near = new() { SampleAtYaw(0f, 2f), SampleAtYaw(3f, 2f) };
        List<SphericalPathSample> far = new() { SampleAtYaw(0f, 500f), SampleAtYaw(3f, 500f) };

        SphericalPathSelector.Select(near, 10, out float nearAngle);
        SphericalPathSelector.Select(far, 10, out float farAngle);
        Assert.That(farAngle, Is.EqualTo(nearAngle).Within(1e-4f));
    }

    private static SphericalPathSample SampleAtYaw(int yaw) => SampleAtYaw((float)yaw);

    private static SphericalPathSample SampleAtYaw(float yaw, float distance = 1f)
    {
        float radians = yaw * MathF.PI / 180f;
        Vector3 direction = new(MathF.Sin(radians), 0f, MathF.Cos(radians));
        return new SphericalPathSample(direction, new Vector3(yaw, distance, 0f), Vector3.UnitY);
    }
}
