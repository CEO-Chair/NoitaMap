#include "Common.hlsl"

cbuffer VertexShaderBuffer : register(b0)
{
    float4x4 ViewProjection; // 64 bytes
    float4x4 World; // 64 bytes
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    
    output.uv = input.uv;
    
    float4 worldPosition = mul(float4(input.position, 1.0), World);
    
    output.position = mul(worldPosition, ViewProjection);
    
    return output;
}