#ifndef SDFS_CGINC
#define SDFS_CGINC

#include "../geometry/GeometryPrimitives.cginc"
#include "../math/Quaternion.cginc"

// OT : Optimized Transformed ...
// T : Transformed ...

float TSphere_SDF(float3 transformedPosition, float radius)
{
    return length(transformedPosition) - radius;
}
float Sphere_SDF(float3 position, Sphere sphere)
{
    float3 diffPos = position - sphere.positionRadius.xyz;
    return TSphere_SDF(diffPos, sphere.positionRadius.w);
}

float OTBox_SDF(float3 relativePosition)
{
    return length(max(relativePosition, 0.0)) + min(max(relativePosition.x, max(relativePosition.y, relativePosition.z)), 0.0);
}
float TBox_SDF(float3 transformedPosition, float3 dimension)
{
    float3 q = abs(transformedPosition) - dimension;
    return OTBox_SDF(q);
}
float Box_SDF(float3 position, Box box)
{
    float3 rotatedPosition = rotate_vector(position - box.position, box.rotation);
    return TBox_SDF(rotatedPosition, box.dimension);
}

// Note : Cylinder with 3-vertices bases
float OTTriPrism_SDF(float3 transformedPosition, float halfWidth, float t, float depth, float invLen, float2 geoNormal)
{
    // geoNormal : cos, sin
    return max(abs(transformedPosition.z - depth * 0.5) - depth * 0.5,
        max(abs(transformedPosition.x) * geoNormal.x + transformedPosition.y * geoNormal.y - halfWidth, -transformedPosition.y));
}
float TTriPrism_SDF(float3 transformedPosition, float halfWidth, float t, float depth)
{
    float invLen = rsqrt(t * t + 1.0); // Cos
    float2 geoNormal = float2(invLen, t * invLen); // cos(angle), sin(angle) // float2(0.866025, 0.5)
    return OTTriPrism_SDF(transformedPosition, halfWidth, t, depth, invLen, geoNormal);
}
float TriPrism_SDF(float3 position, Slash slash)
{
    float3 rotatedPosition = rotate_vector(position-slash.position, slash.rotation);
    return TTriPrism_SDF(rotatedPosition, slash.dimensions.x, slash.dimensions.y, slash.dimensions.z);
}

// Note : A cone which base' width is given by an angle
float OTSphericCone_SDF(float3 transformedPosition, float3 tdr, float3 nCapsule, float invR)
{
    // Capsule
    float capsuleSDF = length(nCapsule) - tdr.z;
    
    // Cone sides
    float r = dot(transformedPosition.xz, transformedPosition.xz) * invR;
    float coneSDF = (r - tdr.x * transformedPosition.y) * rsqrt(1.0 + tdr.x * tdr.x);

    return max(coneSDF, capsuleSDF);
}
float TSphericCone_SDF(float3 transformedPosition, float3 tdr)
{
    // Capsule
    float y = clamp(transformedPosition.y, 0.0, tdr.y);
    float3 nCapsule = float3(transformedPosition.x, transformedPosition.y - y, transformedPosition.z);
    float invR = rsqrt(transformedPosition.x * transformedPosition.x + transformedPosition.z * transformedPosition.z);
    
    return OTSphericCone_SDF(transformedPosition, tdr, nCapsule, invR);
}
float SphericCone_SDF(float3 position, Stab stab)
{
    float3 rotatedPosition = rotate_vector(position-stab.position, stab.rotation);
    return TSphericCone_SDF(rotatedPosition, stab.tdr);
}

float OTCapsule_SDF(float radius, float3 nCapsule)
{
    return length(nCapsule) - radius;
}
float TCapsule_SDF(float3 transformedPosition, float2 depthRadius)
{
    float y = clamp(transformedPosition.y, 0.0, depthRadius.x);
    float3 nCapsule = float3(transformedPosition.x, transformedPosition.y - y, transformedPosition.z);
    return OTCapsule_SDF(depthRadius.y, nCapsule);
}
float Capsule_SDF(float3 position, float3 pCapsule, float2 depthRadius, float4 rotation)
{
    float3 q = rotate_vector(position - pCapsule, q_conj(rotation));
    return TCapsule_SDF(q, depthRadius);
}

float OLineCapsule_SDF(float3 p, float3 a, float r, float3 bah)
{
    return length(p-a - bah) - r;
}
float LineCapsule_SDF(float3 p, float3 a, float3 b, float r)
{
    float3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) * rcp(dot(ba, ba)), 0.0, 1.0);
    return OLineCapsule_SDF(p, a, r, ba * h);
}

#endif