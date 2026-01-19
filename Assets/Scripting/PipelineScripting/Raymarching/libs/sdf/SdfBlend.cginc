#ifndef SDF_BLEND_CGINC
#define SDF_BLEND_CGINC

float Union_SDF(float domain, float evaluate)
{
    return min(domain, evaluate);
}
float Subtraction_SDF(float domain, float evaluate)
{
    return max(domain, -evaluate);
}

#endif