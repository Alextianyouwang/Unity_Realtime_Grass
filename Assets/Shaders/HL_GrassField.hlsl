#ifndef GRASSFIELD_GRAPHIC_INCLUDE
#define GRASSFIELD_GRAPHIC_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../INCLUDE/HL_GraphicsHelper.hlsl"
#include "../INCLUDE/HL_Noise.hlsl"

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
float _Scale, _Bend, _WindSpeed, _WindFrequency, _WindNoiseWeight, _WindDirection, _WindNoiseFrequency;
float4 _TopColor, _BotColor;

float SinWaveWithNoise(float2 uv)
{
    float2 rotatedUV = RotateAroundYInDegrees(float4(uv.x, 0, uv.y, 1), _WindDirection * 360).xz;
    float noise = (perlinNoise(rotatedUV * _WindNoiseFrequency, float2(12.9898, 78.233))) *  _WindNoiseWeight * 10;
    float wave = sin((rotatedUV.x + rotatedUV.y + noise + _Time.y * _WindSpeed * 10) * _WindFrequency);
    return wave * 2 - 1;
}



VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    float3 posOffsetWS = _SpawnBuffer[instanceID].positionWS ;
    float wave = SinWaveWithNoise(posOffsetWS.xz);
    
    float3 posWS = RotateAroundXInDegrees(float4(v.positionOS, 1), _Bend * 90 * v.uv.y * wave * 2).xyz;
    posWS = RotateAroundYInDegrees(float4(posWS, 1), instanceID * 78.233).xyz;

    

    posWS *= _Scale;
    posWS += posOffsetWS;
    o.positionWS = posWS;
    o.positionCS = TransformObjectToHClip(posWS);
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    
    return o;
}

float4 frag(VertexOutput v) : SV_Target
{
    float4 color = lerp(_BotColor,_TopColor ,v.uv.y);
    return color;
}
#endif