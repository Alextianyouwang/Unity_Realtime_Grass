#ifndef GRASSFIELD_GRAPHIC_INCLUDE
#define GRASSFIELD_GRAPHIC_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../INCLUDE/HL_GraphicsHelper.hlsl"
#include "../INCLUDE/HL_Noise.hlsl"
#include "../INCLUDE/HL_ShadowHelper.hlsl"

struct VertexInput
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float3 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    
};
struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 groundNormalWS : TEXCOOR2;
    float3 tangentWS : TEXCOORD3;
    float3 positionWS : TEXCOORD4;
    float4 clumpInfo : TEXCOORD5;
    float4 debug : TEXCOOR6;
    float3 bakedGI : TEXCOORD8;
};
////////////////////////////////////////////////
// Spawn Data
struct SpawnData
{
    float3 positionWS;
    float hash;
    float4 clumpInfo;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
////////////////////////////////////////////////

////////////////////////////////////////////////
// Field Data
StructuredBuffer<float3> _GroundNormalBuffer;
StructuredBuffer<float3> _WindBuffer;
Texture2D<float> _InteractionTexture;
int _NumTilePerClusterSide;
float _ClusterBotLeftX, _ClusterBotLeftY, _TileSize;
////////////////////////////////////////////////

////////////////////////////////////////////////
// Debug
float3 _ChunkColor, _LOD_Color;
////////////////////////////////////////////////

float4 _TopColor, _BotColor, _VariantTopColor, _SpecularColor;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
TEXTURE2D( _Normal);SAMPLER (sampler_Normal);float4 _Normal_ST;
float _GrassScale, _GrassRandomLength,
_GrassTilt, _GrassHeight, _GrassBend, _GrassWaveAmplitude, _GrassWaveFrequency, _GrassWaveSpeed,
_ClumpEmergeFactor, _ClumpThreshold, _ClumpHeightOffset, _ClumpHeightMultiplier, _ClumpTopThreshold,
_GrassRandomFacing,
_SpecularTightness,
_NormalScale,
_BladeThickenFactor,

 _MasterScale;



VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    ////////////////////////////////////////////////
    // Fetch Input
    float3 spawnPosWS = _SpawnBuffer[instanceID].positionWS;
    
    int x = (spawnPosWS.x - _ClusterBotLeftX) / _TileSize;
    int y = (spawnPosWS.z - _ClusterBotLeftY) / _TileSize;
    // Sample Buffers Based on xy
    float3 groundNormalWS = _GroundNormalBuffer[x * _NumTilePerClusterSide + y];
    float windStrength = _WindBuffer[x * _NumTilePerClusterSide + y].x; // [-1,1]
    float windDir = _WindBuffer[x * _NumTilePerClusterSide + y].y * 360; // [0,360]
    float windVariance = _WindBuffer[x * _NumTilePerClusterSide + y].z; // [0,1]
    float interaction = saturate(_InteractionTexture[int2(x, y)]);
    
    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
    float rand = _SpawnBuffer[instanceID].hash * 2 - 1; // [-1,1]
    float3 clumpCenter = float3(_SpawnBuffer[instanceID].clumpInfo.x, 0, _SpawnBuffer[instanceID].clumpInfo.y);
    float2 dirToClump = normalize((spawnPosWS).xz - _SpawnBuffer[instanceID].clumpInfo.xy);
    float distToClump = _SpawnBuffer[instanceID].clumpInfo.z;
    float clumpHash = _SpawnBuffer[instanceID].clumpInfo.w; // [0,1]
    float3 posOS = v.positionOS;
    float viewDist = length(_WorldSpaceCameraPos - spawnPosWS);
    
    ////////////////////////////////////////////////
    // Apply Transform
    float3 posWS = spawnPosWS + posOS * _MasterScale * 5;
    float3 normalWS = v.normalOS;
    float3 tangentWS = v.tangentOS;
    
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // GI
    float2 lightmapUV;
    OUTPUT_LIGHTMAP_UV(v.uv1, unity_LightmapST, lightmapUV);
    float3 vertexSH;
     // OUTPUT_SH(normalWS, vertexSH);
    ////////////////////////////////////////////////

    o.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, normalWS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.positionWS = posWS;
    o.normalWS = normalWS;
    o.tangentWS = tangentWS;
    o.groundNormalWS = groundNormalWS;
    o.clumpInfo = _SpawnBuffer[instanceID].clumpInfo;
    o.debug = float4(lerp(float2(0, 1), float2(1, 0), windStrength + 0.5), interaction,rand);

    #ifdef SHADOW_CASTER_PASS
        o.positionCS = CalculatePositionCSWithShadowCasterLogic(posWS,normalWS);
    #else
        o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
    #endif
    return o;
   
}


float4 frag(VertexOutput v, bool frontFace : SV_IsFrontFace) : SV_Target
{
#ifdef SHADOW_CASTER_PASS
    return 0;
#else
    float3 normalWS = normalize(v.normalWS);
    float3 tangentWS = normalize(v.tangentWS);
    float3 bitangentWS = cross(normalWS, tangentWS);
    float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, v.uv), -_NormalScale);
    normalTS.xyz = normalTS.xzy;
    normalWS = normalize(
    normalTS.x * tangentWS +
    normalTS.y * normalWS +
    normalTS.z * bitangentWS);
    float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv);
    return albedo;
   

#endif
   
}
#endif