using System;

namespace EnhancedPing.Core;

public static class SequenceTiming
{
    public static float EffectivePointDuration(
        int pointCount,
        float preferredPointDurationSeconds,
        float maximumSequenceDurationSeconds)
    {
        if (pointCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(pointCount));
        if (!IsFinite(preferredPointDurationSeconds) || preferredPointDurationSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(preferredPointDurationSeconds));
        if (!IsFinite(maximumSequenceDurationSeconds) || maximumSequenceDurationSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(maximumSequenceDurationSeconds));

        return MathF.Min(preferredPointDurationSeconds, maximumSequenceDurationSeconds / pointCount);
    }

    private static bool IsFinite(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);
}

