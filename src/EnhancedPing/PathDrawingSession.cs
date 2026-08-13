using System.Collections.Generic;
using EnhancedPing.Core;
using UnityEngine;

namespace EnhancedPing;

internal sealed class PathDrawingSession
{
    private readonly List<RuntimePingSample> _accepted = new();
    private RuntimePingSample _latest;
    private bool _hasLatest;

    public IReadOnlyList<RuntimePingSample> PreviewSamples
    {
        get
        {
            if (!_hasLatest || _accepted.Count == 0)
                return _accepted;

            float angle = Angle(_accepted[_accepted.Count - 1].Direction, _latest.Direction);
            if (angle <= 1e-4f)
                return _accepted;

            List<RuntimePingSample> preview = new(_accepted.Count + 1);
            preview.AddRange(_accepted);
            preview.Add(_latest);
            return preview;
        }
    }

    public void Capture(RuntimePingSample sample, float minimumAngleDegrees)
    {
        _latest = sample;
        _hasLatest = true;
        if (_accepted.Count == 0 ||
            Angle(_accepted[_accepted.Count - 1].Direction, sample.Direction) >= minimumAngleDegrees)
        {
            _accepted.Add(sample);
        }
    }

    public PathDrawingResult Finish(int maximumPoints)
    {
        if (_hasLatest && (_accepted.Count == 0 ||
            Angle(_accepted[_accepted.Count - 1].Direction, _latest.Direction) > 1e-4f))
        {
            _accepted.Add(_latest);
        }

        List<SphericalPathSample> coreSamples = new(_accepted.Count);
        foreach (RuntimePingSample sample in _accepted)
            coreSamples.Add(sample.ToCore());

        IReadOnlyList<SphericalPathSample> selected = SphericalPathSelector.Select(
            coreSamples,
            maximumPoints,
            out float totalAngle);
        List<RuntimePingSample> result = new(selected.Count);
        foreach (SphericalPathSample sample in selected)
            result.Add(RuntimePingSample.FromCore(sample));
        return new PathDrawingResult(result, totalAngle);
    }

    private static float Angle(Vector3 left, Vector3 right)
    {
        if (left.sqrMagnitude <= 1e-8f || right.sqrMagnitude <= 1e-8f)
            return float.NaN;
        return Vector3.Angle(left, right);
    }
}
