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
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
    
};
struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 groundNormalWS : TEXCOOR2;
    float4 tangentWS : TEXCOORD3;
    float3 positionWS : TEXCOORD4;
    float4 clumpInfo : TEXCOORD5;
    float4 debug : TEXCOOR6;
    float2 uv2 : TEXCOORD7;
    float3 bakedGI : TEXCOORD8;
};
////////////////////////////////////////////////
// Spawn Data
struct SpawnData
{
    float3 positionWS;
    float hash;
    float4 clumpInfo;
    float4 postureData;
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

float4 _TopColor, _BotColor, _VariantTopColor, _SpecularColor, _SSSColor;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
TEXTURE2D( _Normal);SAMPLER (sampler_Normal);float4 _Normal_ST;
float _GrassScale, _GrassRandomLength, _GrassWaveAmplitude, _GrassWaveFrequency, _GrassWaveSpeed,
_ClumpEmergeFactor, _ClumpThreshold, _ClumpHeightOffset, _ClumpHeightMultiplier, _ClumpTopThreshold,
_SpecularTightness,
_NormalScale,
_BladeThickenFactor,
_SSSTightness,
 _MasterScale, _RandomFacing, _ClumpTightness, _RandomScale;

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
    float2 uv2 = v.uv2;
    float rand = _SpawnBuffer[instanceID].hash * 2 - 1; // [-1,1]
    float3 clumpCenter = float3(_SpawnBuffer[instanceID].clumpInfo.x, 0, _SpawnBuffer[instanceID].clumpInfo.y);
    float2 dirToClump = normalize((spawnPosWS).xz - _SpawnBuffer[instanceID].clumpInfo.xy);
    float distToClump = _SpawnBuffer[instanceID].clumpInfo.z;
    float clumpHash = _SpawnBuffer[instanceID].clumpInfo.w; // [0,1]
    float4 posture = _SpawnBuffer[instanceID].postureData * 2 - 1; // [-1,1]
    float3 posOS = v.positionOS;
    float viewDist = length(_WorldSpaceCameraPos - spawnPosWS);
     ////////////////////////////////////////////////
    // Apply Curve
    float wind01 = (windStrength * 0.5 + 0.5);
    float offset = rand * 2 + windVariance * 30;
    float speed = _Time.y * _GrassWaveSpeed + wind01 * 5;
    float freq = _GrassWaveFrequency;
    float amplitude = _GrassWaveAmplitude * (wind01-0.2) * (1 - interaction);
    float waveX = cos(uv2.y * freq - speed + offset) * amplitude;
    float waveY = sin((uv2.y * freq - speed + offset)*2 ) * amplitude / 2;
    posOS.y += waveY * uv2.y;
    posOS.x += waveX * uv2.y;
    ////////////////////////////////////////////////
    // Apply Transform
	spawnPosWS.xz = lerp(spawnPosWS.xz, clumpCenter.xz, _ClumpTightness);
    float3 posWS = spawnPosWS + posOS * _MasterScale * 5;
    float3 normalWS = v.normalOS;
    float4 tangentWS = v.tangentOS;
    
    float windAngle = -windDir + 90;
    float randomRotationMaxSpan = 180;
    float reverseWind01 = 1 - (windStrength * 0.5 + 0.5);
    float poseAngle = (frac(rand * 12.9898) - 0.5) * randomRotationMaxSpan * _RandomFacing + 90;
    float rotAngle = lerp(poseAngle, windAngle, reverseWind01 * 2) + windStrength * 30;
    float bendAngle = interaction * 45;
    
    

    float scale = 1 + rand * _RandomScale;
    posWS = ScaleWithCenter(posWS, scale, spawnPosWS);
     posWS = RotateAroundAxis(float4(posWS, 1), float3(1, 0, 0), bendAngle, spawnPosWS).xyz;
    posWS = RotateAroundAxis(float4(posWS, 1), float3(0, 1, 0), rotAngle, spawnPosWS).xyz;
    
    normalWS = normalize(RotateAroundXInDegrees(float4(normalWS, 0), bendAngle)).xyz;
    normalWS = normalize(RotateAroundYInDegrees(float4(normalWS, 0), rotAngle)).xyz;
    
    tangentWS = normalize(RotateAroundXInDegrees(float4(tangentWS.xyz, 0), bendAngle));
    tangentWS = normalize(RotateAroundYInDegrees(float4(tangentWS.xyz, 0), rotAngle));
    
    
    tangentWS.w = v.tangentOS.w;
    
    
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

struct CustomInputData
{
    float3 normalWS;
    float3 groundNormalWS;
    float3 positionWS;
    float3 viewDir;
    float viewDist;
    
    float3 albedo;
    float3 specularColor;
    float smoothness;
    
    float3 sss;
    float sssTightness;

    float3 bakedGI;
    float4 shadowCoord;
};

float3 CustomLightHandling(CustomInputData d, Light l)
{
    float atten = lerp(l.shadowAttenuation, 1, smoothstep(20, 30, d.viewDist)) * l.distanceAttenuation;
    float diffuseGround = saturate(dot(l.direction, d.groundNormalWS));
    float3 sss = 0;
    FastSSS_float(d.viewDir, l.direction, d.groundNormalWS, l.color, 0, d.sssTightness, sss);
    return saturate(sss  * d.sss * atten);
}
float3 CustomCombineLight(CustomInputData d)
{
    Light mainLight = GetMainLight(d.shadowCoord);
    MixRealtimeAndBakedGI(mainLight, d.normalWS, d.bakedGI);
    float3 color = d.bakedGI * d.albedo;
    color += CustomLightHandling(d, mainLight);
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++)
        color += CustomLightHandling(d, GetAdditionalLight(lightI, d.positionWS, d.shadowCoord));
    return color;
}


float4 frag(VertexOutput v, bool frontFace : SV_IsFrontFace) : SV_Target
{
    float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv);
#ifdef SHADOW_CASTER_PASS
    clip(albedo.w - 0.5);
    return 0;
#else
    clip(albedo.w - 0.5);
    float3 normalWS = normalize(v.normalWS);
    float3 tangentWS = normalize(v.tangentWS);
    float3 bitangentWS = cross(normalWS, tangentWS);
    float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, v.uv), -_NormalScale );
  

    float sgn =v.tangentWS.w; // should be either +1 or -1
    float3 bitangent = sgn * cross(v.normalWS.xyz, v.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(v.tangentWS.xyz, bitangent.xyz,v.normalWS.xyz);
    normalWS = mul(normalTS, tangentToWorld);
   
    float3 posNDS = v.positionCS / v.positionCS.w;
    float2 uvSS = posNDS.xy / 2 + 0.5;
    
    CustomInputData d = (CustomInputData) 0;
    d.normalWS = normalize(normalWS);
    d.groundNormalWS = normalize(v.groundNormalWS);
    d.positionWS = v.positionWS;
    d.shadowCoord = CalculateShadowCoord(v.positionWS, v.positionCS);
    d.viewDir = normalize(_WorldSpaceCameraPos - v.positionWS);
    d.viewDist = length(_WorldSpaceCameraPos - v.positionWS);
    d.smoothness = exp2(_SpecularTightness * 10 + 1);
    d.sss = _SSSColor;
    d.sssTightness = exp2(_SSSTightness * 10 + 1);
    d.albedo = 0;
    d.specularColor = _SpecularColor.xyz;
    d.bakedGI = v.bakedGI;
    
    InputData data = (InputData) 0;
    
    data.positionWS = v.positionWS;
    data.positionCS = v.positionCS;
    data.normalWS = lerp(normalWS,d.groundNormalWS,0.5);
    data.viewDirectionWS = normalize(_WorldSpaceCameraPos - v.positionWS);
    data.shadowCoord = CalculateShadowCoord(v.positionWS, v.positionCS);
    data.fogCoord = 0;
    data.vertexLighting = 0;
    data.bakedGI = v.bakedGI;
    data.normalizedScreenSpaceUV = uvSS;
    data.shadowMask = 0;
    data.tangentToWorld = tangentToWorld;
    
    SurfaceData surf = (SurfaceData) 0;
    
    
    surf.albedo = albedo.xyz;
    surf.specular = 1;
    surf.metallic = 0;
    surf.smoothness = 0.1;
    surf.normalTS = normalTS;
    surf.emission = 0;
    surf.occlusion = 1;
    surf.alpha = albedo.w;
    surf.clearCoatMask = 0;
    surf.clearCoatSmoothness = 0;
    
    float3 customSSS = CustomCombineLight(d);
    
    float4 finalColor =  UniversalFragmentPBR(data, surf);
    finalColor.xyz += customSSS;
    return finalColor;

#endif
   
}
#endif