#ifndef MATH_UTILS_CGINC
#define MATH_UTILS_CGINC

float SelfDot(float3 val)
{
    return dot(val, val);
}
float SelfDot(float2 val)
{
    return dot(val, val);
}

float LinePlaneIntersectionCoef(float3 planeNormal, float3 planePosition, float3 rayDirection, float3 rayPosition)
{
    return dot(planeNormal, planePosition - rayPosition) / dot(planeNormal, rayDirection);
}
float3 Projection(float3 relativePosition, float3 rayDirection)
{
    return dot(relativePosition, rayDirection) * rayDirection - relativePosition;
}

// fast sqrt
float fsqrt(float squaredVal)
{
    return squaredVal * rsqrt(squaredVal);
}

#endif