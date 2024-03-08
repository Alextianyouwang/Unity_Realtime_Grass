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
    float4 debug : TEXCOORD3;
   
    
};
struct SpawnData
{
    float3 positionWS;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
float _Scale, _Bend, _WindSpeed, _WindFrequency, _WindNoiseAmplitude, _WindDirection, _WindNoiseFrequency,_RandomBendOffset,_WindAmplitude,
_DetailSpeed, _DetailAmplitude, _DetailFrequency,
_HeightRandomnessAmplitude;
float4 _TopColor, _BotColor;

float Perlin(float2 uv)
{
    return perlinNoise(uv, float2(12.9898, 78.233));
}

float SinWaveWithNoise(float2 uv,float direction, float noiseFreq, float noiseWeight, float waveSpeed, float waveFreq)
{
    float2 rotatedUV = Rotate2D(uv,direction * 360);
    float noise = Perlin(rotatedUV * noiseFreq) *  noiseWeight * 10;
    float wave = sin((rotatedUV.x + rotatedUV.y + noise - _Time.y * waveSpeed * 10) * waveFreq);
    return wave;
}




VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    float3 spawnPosWS = _SpawnBuffer[instanceID].positionWS ;
    
    float wave;
    #if  _USE_MAINWAVE_ON
        wave = SinWaveWithNoise(spawnPosWS.xz, _WindDirection, _WindNoiseFrequency, _WindNoiseAmplitude, _WindSpeed, _WindFrequency) ;
    #else
        wave = 0;
    #endif
    
    float detail;
    #if _USE_DETAIL_ON
        detail = Perlin(Rotate2D(spawnPosWS.xz, _WindDirection * 360) * _DetailFrequency * 10 - _Time.y * _DetailSpeed * 10) ;
    #else
        detail = 0;
    #endif
    
    float heightPerlin;
    #if _USE_RANDOM_HEIGHT_ON
        heightPerlin = Perlin(spawnPosWS.xz * 20)*_HeightRandomnessAmplitude;
    #else
        heightPerlin = 0;
    #endif
    
    float3 pos = RotateAroundYInDegrees(float4(v.positionOS, 1), rand1dTo1d(spawnPosWS * 78.233) * 360).xyz;
    float2 rotatedWindDir = Rotate2D(float2(1, -1), _WindDirection * 360);
    float rand = rand1dTo1d(spawnPosWS * 78.233);
    pos = RotateAroundAxis(float4(pos, 1), float3(rotatedWindDir.x, 0, rotatedWindDir.y),
        v.uv.y * ((_Bend * 20 + rand * _RandomBendOffset * 20) + (wave * _WindAmplitude * 20 + (wave / 2 + 0.75) * detail * _DetailAmplitude * 20))).
    xyz;

    pos *= _Scale + heightPerlin;
    pos += spawnPosWS;
    o.positionWS = pos;
    o.positionCS = TransformObjectToHClip(pos);
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    o.debug = float4(wave, detail, 0, 0);
    return o;
}

float4 frag(VertexOutput v) : SV_Target
{
    float4 color = lerp(_BotColor,_TopColor ,v.uv.y);
    
#if _DEBUG_OFF
        return color;
#elif _DEBUG_MAINWAVE
        return v.debug.x;
#elif _DEBUG_DETAILEDWAVE
        return v.debug.y;
#else 
    return color;
#endif
}
#endif