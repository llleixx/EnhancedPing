using System.Collections.Generic;

namespace EnhancedPing;

internal readonly struct PathDrawingResult
{
    public PathDrawingResult(IReadOnlyList<RuntimePingSample> samples, float totalAngleDegrees)
    {
        Samples = samples;
        TotalAngleDegrees = totalAngleDegrees;
    }

    public IReadOnlyList<RuntimePingSample> Samples { get; }
    public float TotalAngleDegrees { get; }
}

