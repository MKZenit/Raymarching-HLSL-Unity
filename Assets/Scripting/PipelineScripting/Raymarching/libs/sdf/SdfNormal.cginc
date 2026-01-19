#ifndef SDF_NORMALS_CGINC
#define SDF_NORMALS_CGINC

#include "../geometry/GeometryPrimitives.cginc"
#include "../geometry/GeometryType.cginc"
#include "../geometry/GeometryBuffers.cginc"
#include "../math/Quaternion.cginc"

// OT : Optimized Transformed ...
// T : Transformed ...

    // 1. world → local
    // 2. local normal
    // 3. local → world

// Note : position != transformedPosition because a sphere world_normal isn't affected by rotation
float3 OTSphere_SDFNormal(float3 transformed_position)
{
    return normalize(transformed_position);
}
float3 TSphere_SDFNormal(float3 position, float3 spherePosition)
{
    return normalize(position - spherePosition);
}
float3 Sphere_SDFNormal(float3 position, Sphere sphere)
{
    float3 p = position - sphere.positionRadius.xyz;
    float3 pLocal = rotate_vector(p, q_conj(sphere.rotation));
    
    float3 nLocal = TSphere_SDFNormal(position, sphere.positionRadius.xyz);

    return rotate_vector(nLocal, sphere.rotation);
}

float3 OTBox_SDFNormal(float3 relativePosition, float3 transformedPosition)
{
    return step(relativePosition.yzx, relativePosition.xyz) * step(relativePosition.zxy, relativePosition.xyz) * sign(transformedPosition);
}
float3 TBox_SDFNormal(float3 transformedPosition, float3 dimensions)
{
    float3 q = abs(transformedPosition) - dimensions;
    return OTBox_SDFNormal(q, transformedPosition);
}
float3 Box_SDFNormal(float3 position, Box box)
{
    float3 p = position - box.position;
    float3 pLocal = rotate_vector(p, q_conj(box.rotation));
    
    float3 nLocal = TBox_SDFNormal(pLocal, box.dimension);

    return rotate_vector(nLocal, box.rotation);
}

// Note : Cylinder with 3-vertices bases
float3 OTTriPrism_SDFNormal(float3 transformedPosition, float halfWidth, float t, float depth, float invLen, float2 geoNormal)
{
    // geoNormal : cos, sin
    return (transformedPosition.y < 0.00001) ? float3(0, -1, 0)
         : ((transformedPosition.z > depth - 0.00001) ? float3(0, 0, 1)
         : ((transformedPosition.z < 0.00001) ? float3(0, 0, -1)
         : float3(geoNormal.x * sign(transformedPosition.x), geoNormal.y, 0)));
}
float3 TTriPrism_SDFNormal(float3 transformedPosition, float halfWidth, float t, float depth)
{
    float invLen = rsqrt(t * t + 1.0); // Cos
    float2 geoNormal = float2(invLen, t * invLen); // cos(angle), sin(angle) // float2(0.866025, 0.5)
    
    return OTTriPrism_SDFNormal(transformedPosition, halfWidth, t, depth, invLen, geoNormal);
}
float3 TriPrism_SDFNormal(float3 position, Slash slash)
{
    float3 p = position - slash.position;
    float3 pLocal = rotate_vector(p, q_conj(slash.rotation) );
    
    float3 nLocal = TTriPrism_SDFNormal(pLocal, slash.dimensions.x, slash.dimensions.y, slash.dimensions.z);
    
    return rotate_vector(nLocal, slash.rotation);
}

// Note : A cone which base' width is given by an angle
float3 OTSphericCone_SDFNormal(float3 transformedPosition, float3 tdr, float3 nCapsule, float invR)
{
    return (transformedPosition.y > min(rcp(tdr.x) * tdr.z, tdr.y))
       ? normalize(nCapsule)
       : normalize(float3(transformedPosition.x * invR, -tdr.x, transformedPosition.z * invR));
}
float3 TSphericCone_SDFNormal(float3 transformedPosition, float3 tdr)
{
    // Capsule
    float y = clamp(transformedPosition.y, 0.0, tdr.y);
    float3 nCapsule = float3(transformedPosition.x, transformedPosition.y - y, transformedPosition.z);
   
    return OTSphericCone_SDFNormal(transformedPosition, tdr, nCapsule,
        rsqrt(transformedPosition.x * transformedPosition.x + transformedPosition.z * transformedPosition.z) );
}
float3 SphericCone_SDFNormal(float3 position, Stab stab)
{
    float3 p = position - stab.position;
    float3 pLocal = rotate_vector(p, q_conj(stab.rotation) );
    
    float3 nLocal = TSphericCone_SDFNormal(pLocal, stab.tdr);
    
    return rotate_vector(nLocal, stab.rotation);
}

float OTCapsule_SDFNormal(float radius, float3 nCapsule)
{
    return length(nCapsule) - radius;
}
float TCapsule_SDFNormal(float3 transformedPosition, float2 depthRadius)
{
    float y = clamp(transformedPosition.y, 0.0, depthRadius.x);
    float3 nCapsule = float3(transformedPosition.x, transformedPosition.y - y, transformedPosition.z);
    return OTCapsule_SDFNormal(depthRadius.y, nCapsule);
}
float Capsule_SDFNormal(float3 position, float3 pCapsule, float2 depthRadius, float4 rotation)
{
    float3 q = rotate_vector(position - pCapsule, q_conj(rotation));
    return TCapsule_SDFNormal(q, depthRadius);
}

float3 OLineCapsule_SDFNormal(float3 p, float3 a, float3 bah)
{
    float3 c = a + bah;
    return normalize(p - c);
}
float3 LineCapsule_SDFNormal(float3 p, float3 a, float3 b, float r)
{
    float3 ba = b - a, pa = p - a;
    float h = clamp(dot(pa, ba) * rcp(dot(ba, ba)), 0.0f, 1.0f);
    return OLineCapsule_SDFNormal(p, a, ba*h);
}

float3 SDFNormal(float3 position, in Geometry closestGeometry)
{
    float3 normal = float3(0, 0, 0);

    switch (closestGeometry.type)
    {
    //  case GT_Structural : normal = float3(0,0,0);
    //  case GT_Skybox: normal = float3(0, 0, 0);
        case GT_Sphere:
            // (re)Note : A sphere's world normal isn't affected by rotation
            normal = TSphere_SDFNormal(position, Spheres[closestGeometry.id].positionRadius.xyz);
            break;
        case GT_Box:
            normal = Box_SDFNormal(position, Boxes[closestGeometry.id]);
            break;
        case GT_Slash:
            normal = TriPrism_SDFNormal(position, Slashes[closestGeometry.id]);
            break;
        case GT_Stab:
            normal = SphericCone_SDFNormal(position, Stabs[closestGeometry.id]);
            break;
    }
    return normal;
}

#endif