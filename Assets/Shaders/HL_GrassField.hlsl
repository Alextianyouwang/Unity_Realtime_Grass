#ifndef GRASSFIELD_GRAPHIC_INCLUDE
#define GRASSFIELD_GRAPHIC_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../INCLUDE/HL_GraphicsHelper.hlsl"

struct VertexInput
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};
struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
};
struct SpawnData
{
    float3 positionWS;
    float radius;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
float _Scale;


VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    float3 posOffsetWS = _SpawnBuffer[instanceID].positionWS ;
    float3 posWS = RotateAroundYInDegrees(float4(v.positionOS, 1), instanceID * 12.9898).xyz;
    posWS *= _Scale;
    posWS += posOffsetWS;
    o.positionWS = posWS;
    o.positionCS = mul(UNITY_MATRIX_MVP, float4(posWS,1));
    o.normalWS = mul(UNITY_MATRIX_M, float4(v.normalOS, 0)).xyz;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    return o;
}

float4 frag(VertexOutput v) : SV_Target
{
    return float4(v.positionWS, 1);
}
#endif