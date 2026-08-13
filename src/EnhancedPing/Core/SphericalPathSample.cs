using System.Numerics;

namespace EnhancedPing.Core;

public readonly struct SphericalPathSample
{
    public SphericalPathSample(Vector3 direction, Vector3 point, Vector3 normal)
    {
        Direction = direction;
        Point = point;
        Normal = normal;
    }

    public Vector3 Direction { get; }
    public Vector3 Point { get; }
    public Vector3 Normal { get; }
}

