using System;
using System.Collections.Generic;
using System.Numerics;

namespace EnhancedPing.Core;

public static class SphericalPathSelector
{
    private const float DirectionEpsilonSquared = 1e-10f;
    private const float DuplicateAngleDegrees = 1e-4f;
    private const float RadToDeg = 180f / MathF.PI;

    public static float AngleDegrees(Vector3 left, Vector3 right)
    {
        if (!TryNormalize(left, out Vector3 normalizedLeft) ||
            !TryNormalize(right, out Vector3 normalizedRight))
        {
            return float.NaN;
        }

        float dot = MathF.Max(-1f, MathF.Min(1f, Vector3.Dot(normalizedLeft, normalizedRight)));
        return MathF.Acos(dot) * RadToDeg;
    }

    public static IReadOnlyList<SphericalPathSample> Select(
        IReadOnlyList<SphericalPathSample> samples,
        int maximumPoints) => Select(samples, maximumPoints, out _);

    public static IReadOnlyList<SphericalPathSample> Select(
        IReadOnlyList<SphericalPathSample> samples,
        int maximumPoints,
        out float totalAngleDegrees)
    {
        if (samples == null)
            throw new ArgumentNullException(nameof(samples));
        if (maximumPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(maximumPoints));

        List<SphericalPathSample> valid = Sanitize(samples);
        float[] cumulativeAngles = BuildCumulativeAngles(valid);
        totalAngleDegrees = cumulativeAngles.Length == 0 ? 0f : cumulativeAngles[cumulativeAngles.Length - 1];
        if (valid.Count <= maximumPoints)
            return valid;
        if (totalAngleDegrees <= DuplicateAngleDegrees)
            return new[] { valid[0] };

        int selectedCount = Math.Min(valid.Count, maximumPoints);
        List<SphericalPathSample> selected = new(selectedCount) { valid[0] };
        int previousIndex = 0;

        for (int slot = 1; slot < selectedCount - 1; slot++)
        {
            float targetAngle = totalAngleDegrees * slot / (selectedCount - 1);
            int minimumIndex = previousIndex + 1;
            int maximumIndex = valid.Count - (selectedCount - slot);
            int bestIndex = minimumIndex;
            float bestDistance = MathF.Abs(cumulativeAngles[bestIndex] - targetAngle);

            for (int candidate = minimumIndex + 1; candidate <= maximumIndex; candidate++)
            {
                float distance = MathF.Abs(cumulativeAngles[candidate] - targetAngle);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = candidate;
                }
            }

            selected.Add(valid[bestIndex]);
            previousIndex = bestIndex;
        }

        selected.Add(valid[valid.Count - 1]);
        return selected;
    }

    private static float[] BuildCumulativeAngles(IReadOnlyList<SphericalPathSample> samples)
    {
        float[] cumulativeAngles = new float[samples.Count];
        for (int i = 1; i < samples.Count; i++)
        {
            float segmentAngle = AngleDegrees(samples[i - 1].Direction, samples[i].Direction);
            cumulativeAngles[i] = cumulativeAngles[i - 1] + (IsFinite(segmentAngle) ? segmentAngle : 0f);
        }
        return cumulativeAngles;
    }

    private static List<SphericalPathSample> Sanitize(IReadOnlyList<SphericalPathSample> samples)
    {
        List<SphericalPathSample> valid = new(samples.Count);
        foreach (SphericalPathSample sample in samples)
        {
            if (!TryNormalize(sample.Direction, out Vector3 direction) ||
                !IsFinite(sample.Point) || !IsFinite(sample.Normal))
            {
                continue;
            }

            Vector3 normal = TryNormalize(sample.Normal, out Vector3 normalizedNormal)
                ? normalizedNormal
                : Vector3.UnitY;
            SphericalPathSample normalized = new(direction, sample.Point, normal);
            if (valid.Count > 0 && AngleDegrees(valid[valid.Count - 1].Direction, direction) <= DuplicateAngleDegrees)
                continue;

            valid.Add(normalized);
        }

        return valid;
    }

    private static bool TryNormalize(Vector3 value, out Vector3 normalized)
    {
        float lengthSquared = value.LengthSquared();
        if (!IsFinite(lengthSquared) || lengthSquared <= DirectionEpsilonSquared)
        {
            normalized = Vector3.Zero;
            return false;
        }

        normalized = value / MathF.Sqrt(lengthSquared);
        return IsFinite(normalized);
    }

    private static bool IsFinite(Vector3 value) =>
        IsFinite(value.X) && IsFinite(value.Y) && IsFinite(value.Z);

    private static bool IsFinite(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);
}
