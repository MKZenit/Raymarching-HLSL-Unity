#ifndef GEOMETRY_STRUCTS_CGINC
#define GEOMETRY_STRUCTS_CGINC

struct Sphere
{
    float4 positionRadius; // x,y,z = position ; w = radius
    float4 rotation;
};

struct Box
{
    float3 position;
    float3 dimension;
    float4 rotation;
};

struct Slash
{
    float3 position;
    float3 dimensions; // need for the angle of the slash blade : halfWidth, Tan(angle), depth
    float4 rotation;
};

struct Stab
{
    float3 position;
    float3 tdr; // x = Tan(angle) ; y = depth ; z = radius
    float4 rotation;
};
#endif