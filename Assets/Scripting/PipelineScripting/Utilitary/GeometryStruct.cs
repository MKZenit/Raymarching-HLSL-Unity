using UnityEngine;

public struct Sphere
{
    public Vector4 positionRadius; // x,y,z = position ; w = radius
    public Vector4 rotation;
}

public struct Box
{
    public Vector3 position;
    public Vector3 dimension;
    public Vector4 rotation;
}

public struct Slash
{
    public Vector3 position;
    public Vector3 dimensions; // x = halfWidth; y = Tan(angle); z = depth
    public Vector4 rotation;
}

public struct Stab
{
    public Vector3 position;
    public Vector3 tdr; // x = Tan(angle) ; y = depth ; z = radius
    public Vector4 rotation;
}

public static class GeometryOverall
{
    public readonly static int SphereStride = sizeof(float) * 8;
    public readonly static int BoxStride = sizeof(float) * 10;
    public readonly static int SlashStride = sizeof(float) * 10;
    public readonly static int StabStride = sizeof(float) * 10;
}