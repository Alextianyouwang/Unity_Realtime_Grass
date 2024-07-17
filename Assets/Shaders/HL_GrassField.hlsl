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
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    
};
struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float4 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 groundNormalWS : TEXCOOR2;
    float3 tangentWS : TEXCOORD3;
    float3 positionWS : TEXCOORD4;
    float4 clumpInfo : TEXCOORD5;
    float4 debug : TEXCOOR6;
    float height : TEXCOOR7;
    float3 bakedGI : TEXCOORD8;
    float4 mask : TEXCOORD9;
    float4 flow : TEXCOORD10;
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
StructuredBuffer<float4> _WindBuffer;
StructuredBuffer<float4> _MaskBuffer;
Texture2D<float> _InteractionTexture;
Texture2D<float4> _FlowTexture;

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
float _GrassScale, _GrassRandomLength,
_GrassTilt, _GrassHeight, _GrassBend, _GrassWaveAmplitude, _GrassWaveFrequency, _GrassWaveSpeed, _GrassPostureFacing,
_ClumpEmergeFactor, _ClumpThreshold, _ClumpHeightOffset, _ClumpHeightMultiplier, _ClumpTopThreshold,
_GrassRandomFacing,
_SpecularTightness,
_SSSTightness,
_NormalScale,
_BladeThickenFactor,_TextureShift;


void CalculateGrassCurve(float t, float interaction,float wind, float variance, float hash, float4 posture, out float3 pos, out float3 tan)
{
    float lengthMult = 1 + _GrassRandomLength * frac(hash * 50);
    float waveAmplitudeMult = 1 - interaction;
    float offset = hash * 4 + variance * 30;
    float bendFactor = wind * 0.5 + 0.5;
    float tiltFactor = wind + interaction;
    // Maximum tilt angle
    tiltFactor *= -50;
    tiltFactor = max(-50, tiltFactor);
    float2 tiltHeight = float2(_GrassTilt, _GrassHeight) * lengthMult;
    tiltHeight = Rotate2D(tiltHeight, tiltFactor);

  
    float2 waveDir = normalize(tiltHeight);
    float propg = dot(waveDir, tiltHeight);
    float grassWave = 0;
    float freq = 20 * _GrassWaveFrequency;
    float amplitude = _GrassWaveAmplitude * waveAmplitudeMult * bendFactor ; 
    float speed = _Time.y * _GrassWaveSpeed * 10 + bendFactor * 10;
    grassWave += sin(t * freq - speed + offset) * amplitude * lengthMult;

    float2 P3 = tiltHeight ;
    float2 P2 = tiltHeight * (0.6 + posture.x * 0.3) + normalize(float2(-tiltHeight.y, tiltHeight.x)) * (_GrassBend * 2 * frac((hash * 0.5 + 0.5) * 30) + bendFactor);
    P2 = float2(P2.x, P2.y) + normalize(float2(-P3.y, P3.x)) * grassWave * lengthMult;
    P3 = float2(P3.x, P3.y) + normalize(float2(-P3.y, P3.x)) * grassWave * 1.3* lengthMult;
    CubicBezierCurve_Tilt_Bend(float3(0, P2.y, P2.x), float3(0, P3.y, P3.x), t, pos, tan);
}


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
    float2 windDir = _WindBuffer[x * _NumTilePerClusterSide + y].yz; // [0,360]
    float windVariance = _WindBuffer[x * _NumTilePerClusterSide + y].w; // [0,1]
    float4 maskBuffer = _MaskBuffer[x * _NumTilePerClusterSide + y]; // [0,1]
    float interaction = saturate(_InteractionTexture[int2(x, y)]);
    float4 flow = normalize(_FlowTexture[int2(x, y)]);
    
    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
    float rand = _SpawnBuffer[instanceID].hash * 2 - 1; // [-1,1]
    float3 clumpCenter = float3(_SpawnBuffer[instanceID].clumpInfo.x, 0, _SpawnBuffer[instanceID].clumpInfo.y);
    float2 dirToClump = normalize((spawnPosWS).xz - _SpawnBuffer[instanceID].clumpInfo.xy);
    float distToClump = _SpawnBuffer[instanceID].clumpInfo.z;
    float clumpHash = _SpawnBuffer[instanceID].clumpInfo.w; // [0,1]
    float4 posture = _SpawnBuffer[instanceID].postureData * 2 - 1; // [-1,1]
    float3 posOS = v.positionOS;
    float viewDist = length(_WorldSpaceCameraPos - spawnPosWS);
    float nearGrass = 20;
    float farGrass = 120;
    float mask = 1 - smoothstep(nearGrass, farGrass, viewDist);
    ////////////////////////////////////////////////


    ////////////////////////////////////////////////
    // Apply Curve
    float3 curvePosOS = 0;
    float3 curveTangentOS = 0;
    float3 windAffectDegree = 45;
    CalculateGrassCurve(uv.y, interaction, windStrength * 0.55, windVariance, rand, posture, curvePosOS, curveTangentOS);
    float3 curveNormalOS = cross(float3(-1, 0, 0), normalize(curveTangentOS));
    posOS.yz = curvePosOS.yz;
    ////////////////////////////////////////////////
    
    
    ////////////////////////////////////////////////
    // Apply Transform
    float3 posWS = posOS * _GrassScale + spawnPosWS;
    float3 curvePosWS = curvePosOS * _GrassScale + spawnPosWS;
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // Apply Clump
    float2 clumpDir = dirToClump * clumpHash * step(_ClumpThreshold, clumpHash);
    float reverseWind01 = 1 - (windStrength * 0.5 + 0.5);
    float postureHeight = step(0.97, posture.x ) ;
    float scale = 1 + (_ClumpHeightOffset * 5 - distToClump) * _ClumpHeightMultiplier * clumpHash * step(_ClumpThreshold, clumpHash) * rand + postureHeight;

    float2 finalDir = lerp(windDir, clumpDir, mask * _ClumpEmergeFactor * reverseWind01);
    float2 postureDir = normalize(ReverseAtan2Degrees(360 * (posture.x * 0.5 + posture.y * 0.5 + posture.w * 0.5)));
    float2 randomDir = normalize(ReverseAtan2Degrees( 360 * (frac(rand * 60) - 0.5)));
    finalDir = lerp(finalDir, postureDir, _GrassPostureFacing * reverseWind01);
    finalDir = lerp(finalDir, randomDir, _GrassRandomFacing * reverseWind01);
    
    posWS = ScaleWithCenter(posWS, scale, spawnPosWS);
    curvePosWS = ScaleWithCenter(curvePosWS, scale, spawnPosWS);
    posWS = TransformWithAlignment(float4(posWS, 1), float3(0, 0, 1), float3(finalDir.x, 0, finalDir.y), spawnPosWS);
    curvePosWS = TransformWithAlignment(float4(curvePosWS, 1), float3(0, 0, 1), float3(finalDir.x, 0, finalDir.y), spawnPosWS);
    float3 normalWS = TransformWithAlignment(float4(curveNormalOS, 0), float3(0, 0, 1), float3(finalDir.x, 0, finalDir.y));
    float3 tangentWS = TransformWithAlignment(float4(curveTangentOS, 0), float3(0, 0, 1), float3(finalDir.x, 0, finalDir.y));
    ////////////////////////////////////////////////
    
    
    ////////////////////////////////////////////////
    // Apply View Space Adjustment
    float offScreenFactor =  1 - abs(dot(normalWS, normalize(_WorldSpaceCameraPos - posWS)));
    float3 posVS = mul(UNITY_MATRIX_V, float4(posWS, 1)).xyz;
    float3 curvePosVS = mul(UNITY_MATRIX_V, float4(curvePosWS, 1)).xyz;
    float3 shiftDistVS = posVS - curvePosVS;
    float3 projectedShiftDistVS = normalize(ProjectOntoPlane(shiftDistVS, float3(0, 0, -1)));
    posVS.xy += length(shiftDistVS) > 0.0001 ?
    projectedShiftDistVS.xy * (_BladeThickenFactor * 0.05 - curvePosVS.z * 0.0005) : 0;
    ////////////////////////////////////////////////

    ////////////////////////////////////////////////
    // GI
    float2 lightmapUV;
    OUTPUT_LIGHTMAP_UV(v.uv1, unity_LightmapST, lightmapUV);
    float3 vertexSH;
    OUTPUT_SH(normalWS, vertexSH);
    ////////////////////////////////////////////////
    float4 positionCS= mul(UNITY_MATRIX_P, float4(posVS, 1));
    float2 uvSS = (positionCS.xy / positionCS.w) * 0.5 + 0.5;
    uvSS.y = 1 - uvSS.y;
    
    o.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, normalWS);
    o.uv = float4(TRANSFORM_TEX(v.uv, _MainTex),uvSS);
    o.positionWS = posWS;
    o.normalWS = normalWS;
    o.tangentWS = tangentWS;
    o.groundNormalWS = groundNormalWS;
    o.clumpInfo = _SpawnBuffer[instanceID].clumpInfo;
    o.debug = float4(lerp(float2(0, 1), float2(1, 0), windStrength + 0.5), interaction,rand);
    o.height = max(scale, _GrassRandomLength * rand) * clumpHash;
    o.mask = maskBuffer;
    o.flow = atan2(flow.x, flow.z);

    #ifdef SHADOW_CASTER_PASS
        o.positionCS = CalculatePositionCSWithShadowCasterLogic(posWS,normalWS);
    #else
        o.positionCS = positionCS;
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
    // Shadow in Project Setting set to 30 meters
    float atten = lerp(l.shadowAttenuation, 1, smoothstep(20, 30, d.viewDist)) * l.distanceAttenuation;
    float3 radiance = l.color * atten;
    float diffuse = saturate(dot(l.direction, d.normalWS));
    float diffuseGround = saturate(dot(l.direction, d.groundNormalWS));
    float3 sss = 0;
    FastSSS_float(d.viewDir, l.direction, d.groundNormalWS, l.color, 0, d.sssTightness, sss);
    float3 lv_dir = normalize(l.direction + d.viewDir);
    float specularDot = saturate(dot(d.normalWS, lv_dir ));
    float specularDotGround = saturate(dot(d.groundNormalWS, lv_dir));
    float specularBlend = lerp(specularDot, specularDotGround * 0.99, smoothstep(20, 120, d.viewDist));
    float diffuseBlend = diffuseGround * 1 + diffuse * 0.5;
    float specular = pow(specularBlend, d.smoothness) * diffuseBlend;
    float3 phong = saturate (diffuseBlend * d.albedo + specular * d.specularColor);
    return phong * radiance + sss * d.sss * smoothstep(-0.2,1,diffuseGround) * atten;
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

float4 StaticElectricity(float2 uvSS, float depth)
{
    float noise = step(0.5,rand2dTo1d(uvSS + _Time.y));
    float4 color = float4(0.1, 0.5, 1, 1) * 2;
    color *= noise ;
    return color;
}

float4 frag(VertexOutput v, bool frontFace : SV_IsFrontFace) : SV_Target
{
#ifdef SHADOW_CASTER_PASS
    return 0;
#else
    float rand01 = frac(((v.debug.w + 1) * 5));
    float2 texShift = float2(_TextureShift * step(0.5, rand01), 0);
    float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv.xy + texShift);
    clip(albedo.a +step(0.5, rand01) - 0.5);
    
    float3 normalWS = normalize(v.normalWS);
    float3 tangentWS = normalize(v.tangentWS);
    float3 bitangentWS = cross(normalWS, tangentWS);
    float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, v.uv .xy + texShift), -_NormalScale);
    normalWS = normalize(
    normalTS.x * tangentWS +
    normalTS.z * normalWS +
    normalTS.y * bitangentWS);
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
	d.albedo = lerp(_BotColor, lerp(_TopColor, _VariantTopColor, saturate(v.height + _ClumpTopThreshold * 2 - 1)), v.uv.y).xyz * albedo.xyz;
    d.specularColor = _SpecularColor.xyz;
    d.bakedGI = v.bakedGI;

    
    float3 finalColor = CustomCombineLight(d) ;
    finalColor = v.mask.x >= 0.5 ? StaticElectricity(v.uv.zw, v.positionCS.w) : finalColor;
#if _DEBUG_OFF

        return finalColor.xyzz;
#elif _DEBUG_CHUNKID
        return _ChunkColor.xyzz;
#elif _DEBUG_LOD
        return _LOD_Color.xyzz;
#elif _DEBUG_CLUMPCELL
        return v.clumpInfo.wwzz;
#elif _DEBUG_GLOBALWIND
        return float4 (v.debug.xy,0,0);
#elif _DEBUG_HASH
        return v.debug.wwww;
#elif _DEBUG_INTERACTION
        return v.debug.zzzz;
#else 
    return d.albedo.xyzz;
#endif
#endif
   
}
#endif