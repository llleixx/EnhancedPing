using EnhancedPing.Core;
using UnityEngine;
using CoreVector3 = System.Numerics.Vector3;

namespace EnhancedPing;

internal readonly struct RuntimePingSample
{
    public RuntimePingSample(Vector3 direction, Vector3 point, Vector3 normal)
    {
        Direction = direction;
        Point = point;
        Normal = normal;
    }

    public Vector3 Direction { get; }
    public Vector3 Point { get; }
    public Vector3 Normal { get; }

    public SphericalPathSample ToCore() => new(ToCore(Direction), ToCore(Point), ToCore(Normal));

    public static RuntimePingSample FromCore(SphericalPathSample sample) =>
        new(ToUnity(sample.Direction), ToUnity(sample.Point), ToUnity(sample.Normal));

    private static CoreVector3 ToCore(Vector3 value) => new(value.x, value.y, value.z);
    private static Vector3 ToUnity(CoreVector3 value) => new(value.X, value.Y, value.Z);
}

