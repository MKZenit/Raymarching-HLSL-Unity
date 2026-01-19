#ifndef FIELD_BUFFERS_CGINC
#define FIELD_BUFFERS_CGINC

RWTexture2D<float4> Result;
Texture2D<float4> ResultSRV;

RWTexture2D<float> DepthTex;
Texture2D<float> DepthTexSRV;
RWTexture2D<uint> GeometryTex;
Texture2D<uint> GeometryTexSRV;
RWTexture2D<float3> NormalTex;
Texture2D<float3> NormalTexSRV;

RWTexture2D<uint> IterationTex;
Texture2D<uint> IterationTexSRV;

#endif