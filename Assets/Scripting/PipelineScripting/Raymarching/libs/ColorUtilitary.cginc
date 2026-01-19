#ifndef COLOR_UTILITARY_CGINC
#define COLOR_UTILITARY_CGINC

float3 GetColorFromValue(float val, float maxVal)
{
    float modVal = val / maxVal;
    modVal = saturate(modVal);
    
    float3 c = float3(saturate(modVal * 2 - 1), 1 - abs(0.5 - modVal) * 2, saturate(abs(1 - modVal) * 2 - 1));
    
    return c;
}

#endif