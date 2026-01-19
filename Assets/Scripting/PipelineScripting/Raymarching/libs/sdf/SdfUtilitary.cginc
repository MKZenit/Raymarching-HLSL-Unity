#ifndef SDF_UTILITARY_CGINC
#define SDF_UTILITARY_CGINC

#include "../geometry/GeometryPrimitives.cginc"
#include "../Math/Quaternion.cginc"
#include "../Math/MathUtils.cginc"

// Plane position of a geometry depending on its normal
float3 TSpherePlanePosition(float3 position, float radius, float3 sphere_normal)
{
    return position + sphere_normal * radius;
}
float3 SpherePlanePosition(Sphere sphere, float3 sphere_normal)
{
    return TSpherePlanePosition(sphere.positionRadius.xyz, sphere.positionRadius.w, sphere_normal);
}

float3 TBoxPlanePosition(float3 position, float3 dimension, float4 rotation, float3 box_normal)
{
    return position + rotate_vector(box_normal * dimension, rotation);
}
float3 BoxPlanePosition(Box box, float3 box_normal)
{
    return TBoxPlanePosition(box.position, box.dimension, box.rotation, box_normal);
}

float3 TTriPrismPlanePosition(float3 position, float3 dimensions, float4 rotation, float3 slash_normal)
{
    return position + rotate_vector(
                    slash_normal * float3(dimensions.x, (slash_normal.y < -0.00001) ? 0 : dimensions.x, (slash_normal.z < -0.00001) ? 0 : dimensions.z),
                    rotation);
}
float3 TriPrismPlanePosition(Slash slash, float3 slash_normal)
{
    return TTriPrismPlanePosition(slash.position, slash.dimensions, slash.rotation, slash_normal);
}

float3 TSphericConePlanePosition(float3 position, float3 tdr, float4 rotation, float3 stab_normal)
{
    return position + ((stab_normal.y > -0.00001) ?
          rotate_vector(stab_normal * min(tdr.z, tdr.x * tdr.y) + float3(0, tdr.y, 0), rotation)
        : float3(0, 0, 0));
}
float3 SphericConePlanePosition(Stab stab, float3 stab_normal)
{
    return stab.position + ((stab_normal.y > -0.00001) ? 
          rotate_vector(stab_normal * min(stab.tdr.z, stab.tdr.x * stab.tdr.y) + float3(0, stab.tdr.y, 0), stab.rotation)
        : float3(0, 0, 0));
}

// Square of the maximum length of a geometry from its position
float SquareSizeSphere(float radius)
{
    return radius * radius;
}
float SquareSizeSphere(Sphere sphere)
{
    return sphere.positionRadius.w * sphere.positionRadius.w;
}
float SquareSizeBox(Box box)
{
    return SelfDot(box.dimension);
}
float SquareSizeTriPrism(float4 wdcs)
{
    return wdcs.x * max(rcp(wdcs.w * wdcs.w), wdcs.w * rcp(wdcs.z * wdcs.z) + 1) + wdcs.y;
}
float SquareSizeSphericCone(Stab stab)
{
    return (stab.tdr.y + stab.tdr.z) * (stab.tdr.y + stab.tdr.z);
}

// displacement for centered geometries on their position attribute
float SquareSizeCenteredTriPrism(float4 wdcs)
{
    return wdcs.x * max(rcp(wdcs.w * wdcs.w), wdcs.w * rcp(wdcs.z * wdcs.z) + 1) + wdcs.y * 0.5;
}
float SquareSizeCenteredSphericCone(Stab stab)
{
    float d = stab.tdr.y * 0.5 + min(stab.tdr.x * stab.tdr.y, stab.tdr.z);
    return d * d;
}

// displace geometry so that it centers their position 
float3 TriPrismCenterDisplacement(Slash slash)
{
    return rotate_vector(float3(0, 0, slash.dimensions.z * 0.5), slash.rotation);
}
float3 SphericConeCenterDisplacement(Stab stab)
{
    return -rotate_vector(float3(0, stab.tdr.y * 0.5, 0), stab.rotation);
}


#endif