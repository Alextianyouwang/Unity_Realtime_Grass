#ifndef GRASSFIELD_GRAPHIC_INCLUDE
#define GRASSFIELD_GRAPHIC_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
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
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
    float4 debug : TEXCOORD3;
    float4 clumpInfo : TEXCOORD4;
    float3 groundNormalWS : TEXCOOR5;
    float height : TEXCOOR6;
    float3 bakedGI : TEXCOORD7;
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
StructuredBuffer<float> _WindBuffer;
int _NumTilePerClusterSide;
float _ClusterBotLeftX, _ClusterBotLeftY, _TileSize;
////////////////////////////////////////////////

////////////////////////////////////////////////
// Debug
float3 _ChunkColor, _LOD_Color;
////////////////////////////////////////////////

float4 _TopColor, _BotColor, _VariantTopColor, _SpecularColor;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
float _GrassScale, _GrassFacingDirection,_GrassRandomLength,
_GrassTilt, _GrassHeight, _GrassBend, _GrassWaveAmplitude, _GrassWaveFrequency, _GrassWaveSpeed,
_ClumpEmergeFactor, _ClumpThreshold, _ClumpHeightOffset, _ClumpHeightMultiplier, _ClumpTopThreshold,
_GrassRandomFacing,
_SpecularTightness, 
_BladeThickenFactor;

void CalculateGrassCurve(float t, float lengthMult, float offset,float tiltFactor, out float3 pos, out float3 tan)
{
    float2 tiltHeight = float2(_GrassTilt, _GrassHeight) * lengthMult;
    tiltHeight = Rotate2D(tiltHeight, -tiltFactor);
    float2 waveDir = normalize(tiltHeight);
    float propg = dot(waveDir, tiltHeight);
    float grassWave = 0;
    float freq = 5 * _GrassWaveFrequency;
    float amplitude = _GrassWaveAmplitude ;
    float speed = _Time.y * _GrassWaveSpeed * 10;
    [unroll]
    for (int i = 0; i < 3; i++)
    {

        grassWave += sin(t * freq - speed + offset) * amplitude * lengthMult;
        freq *= 1.2;
        amplitude *= 0.8;
        speed *= 1.4;
        offset *= 78.233;
    }
        
    float2 P3 = tiltHeight;
    float2 P2 = tiltHeight / 2 + normalize(float2(-tiltHeight.y, tiltHeight.x)) * _GrassBend * lengthMult;
    P2 = float2(P2.x, P2.y) + normalize(float2(-P3.y, P3.x)) * grassWave ;
    P3 = float2(P3.x, P3.y) + normalize(float2(-P3.y, P3.x)) * grassWave;
    CubicBezierCurve_Tilt_Bend(float3(0, P2.y, P2.x), float3(0, P3.y, P3.x), t, pos, tan);
}




VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    ////////////////////////////////////////////////
    // Fetch Input
    //float wind = _WindBuffer[x * _NumTilePerClusterSide + y];
    float3 spawnPosWS = _SpawnBuffer[instanceID].positionWS;
    
    int x = (spawnPosWS.x - _ClusterBotLeftX) / _TileSize;
    int y = (spawnPosWS.z - _ClusterBotLeftY) / _TileSize;
    float3 groundNormalWS = _GroundNormalBuffer[x * _NumTilePerClusterSide + y];
    float wind = _WindBuffer[x * _NumTilePerClusterSide + y];
    
    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
    float rand = _SpawnBuffer[instanceID].hash * 2 - 1;
    float3 clumpCenter = float3(_SpawnBuffer[instanceID].clumpInfo.x, 0, _SpawnBuffer[instanceID].clumpInfo.y);
    float2 dirToClump = normalize((spawnPosWS).xz - _SpawnBuffer[instanceID].clumpInfo.xy);
    float distToClump = _SpawnBuffer[instanceID].clumpInfo.z;
    float clumpHash= _SpawnBuffer[instanceID].clumpInfo.w;
    float3 posOS = v.positionOS;
    ////////////////////////////////////////////////


    ////////////////////////////////////////////////
    // Apply Curve
    float3 curvePosOS = 0;
    float3 curveTangentOS = 0;
    CalculateGrassCurve(uv.y, 1 + _GrassRandomLength * frac(rand * 78.233), rand * 39.346, (wind + 0.1) * 45, curvePosOS, curveTangentOS);
    float3 curveNormalOS = normalize(cross(float3(-1, 0, 0), curveTangentOS));
    posOS.yz = curvePosOS.yz;
    ////////////////////////////////////////////////
    
    
    ////////////////////////////////////////////////
    // Apply Transform
    float3 posWS = posOS * _GrassScale + spawnPosWS;
    float3 curvePosWS = curvePosOS * _GrassScale + spawnPosWS;
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // Apply Clump
    float windAngle = _GrassFacingDirection;
    float clumpAngle = degrees(atan2(dirToClump.x, dirToClump.y)) * clumpHash * step(_ClumpThreshold, clumpHash);
    float viewDist = length(_WorldSpaceCameraPos - posWS);
    float mask = 1 - smoothstep(10, 70, viewDist);
    float rotAngle = lerp(windAngle, clumpAngle * mask, _ClumpEmergeFactor) - (frac(rand * 12.9898) - 0.5) * 120 * _GrassRandomFacing;
    float scale = 1 + (_ClumpHeightOffset * 5 - distToClump) * _ClumpHeightMultiplier * clumpHash * step(_ClumpThreshold, clumpHash) * rand;
    posWS = ScaleWithCenter(posWS, scale, spawnPosWS);
    posWS = RotateAroundAxis(float4(posWS, 1), float3(0,1,0),rotAngle,spawnPosWS).xyz;
    curvePosWS = ScaleWithCenter(curvePosWS, scale, spawnPosWS);
    curvePosWS = RotateAroundAxis(float4(curvePosWS, 1), float3(0, 1, 0), rotAngle, spawnPosWS).xyz;
    float3 normalWS = RotateAroundYInDegrees(float4(curveNormalOS, 0), rotAngle).xyz;
    float3 tangentWS = RotateAroundYInDegrees(float4(curveTangentOS, 0), rotAngle).xyz;
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // Apply Clip Space Adjustment
    float offScreenFactor = smoothstep(0, 1, 1 - abs(dot(normalWS, normalize(_WorldSpaceCameraPos - posWS))));
    float3 posVS = mul(UNITY_MATRIX_V, float4(posWS, 1)).xyz;
    float3 curvePosVS = mul(UNITY_MATRIX_V, float4(curvePosWS, 1)).xyz;
    float3 normalVS = mul(UNITY_MATRIX_VP, float4(normalWS, 0)).xyz;
    float3 projectedNormalVS = normalize(ProjectOntoPlane(normalVS, float3(0, 0, 1)));
    float3 shiftDistVS = posVS - curvePosVS;
    float3 projectedVS = normalize(dot(shiftDistVS, projectedNormalVS) * projectedNormalVS);
    posVS.xy += length(shiftDistVS) > 0.0001 ?
    projectedVS.xy * _BladeThickenFactor * offScreenFactor * 0.05 : 0;
    ////////////////////////////////////////////////

    ////////////////////////////////////////////////
    // GI
    float2 lightmapUV;
    OUTPUT_LIGHTMAP_UV(v.uv1, unity_LightmapST, lightmapUV);
    float3 vertexSH;
    OUTPUT_SH(normalWS, vertexSH);
    ////////////////////////////////////////////////

    o.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, normalWS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.positionWS = posWS;
    o.normalWS = normalWS;
    o.groundNormalWS = groundNormalWS;
    o.clumpInfo = _SpawnBuffer[instanceID].clumpInfo;
    o.debug = float4(lerp(float3(0, 0, 1),float3(1, 1, 0), wind + 0.5),rand);
    o.debug = float4(projectedNormalVS, rand);
    o.height = max(scale, _GrassRandomLength * rand) * clumpHash;
    #ifdef SHADOW_CASTER_PASS
        o.positionCS = CalculatePositionCSWithShadowCasterLogic(posWS,normalWS);
    #else
        o.positionCS = mul(UNITY_MATRIX_P, float4(posVS, 1));
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
    
    float3 bakedGI;
    float4 shadowCoord;
};
float3 CustomLightHandling(CustomInputData d, Light l)
{
    float atten = lerp(l.shadowAttenuation, 1, smoothstep(20, 30, d.viewDist)) * l.distanceAttenuation;
    float3 radiance = l.color * atten;
    float diffuse = saturate(dot(l.direction, d.normalWS));
    float diffuseGround = saturate(dot(l.direction, d.groundNormalWS));
    float specularDot = saturate(dot(d.normalWS, normalize(l.direction + d.viewDir)));
    float specular = pow(specularDot, d.smoothness) * diffuse;
    float3 phong = ((diffuseGround * 0.5 + diffuse * 0.3) * d.albedo + specular * d.specularColor);
    return phong * radiance;
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

float4 frag(VertexOutput v) : SV_Target
{
#ifdef SHADOW_CASTER_PASS
    return 0;
#else
    CustomInputData d = (CustomInputData) 0;
    d.normalWS = normalize(v.normalWS);
    d.groundNormalWS = normalize(v.groundNormalWS);
    d.positionWS = v.positionWS;
    d.shadowCoord = CalculateShadowCoord(v.positionWS, v.positionCS);
    d.viewDir = normalize(_WorldSpaceCameraPos - v.positionWS);
    d.viewDist = length(_WorldSpaceCameraPos - v.positionWS);
    d.smoothness = exp2(_SpecularTightness * 10 + 1);
    d.albedo = lerp(_BotColor, lerp(_TopColor, _VariantTopColor, saturate(v.height + _ClumpTopThreshold * 2 - 1)), v.uv.y);
    d.specularColor = _SpecularColor;
    d.bakedGI = v.bakedGI;

    float3 finalColor = CustomCombineLight(d) ;
#if _DEBUG_OFF
       return finalColor.xyzz;
    return  d.normalWS.xyzz;
#elif _DEBUG_CHUNKID
        return _ChunkColor.xyzz;
#elif _DEBUG_LOD
        return _LOD_Color.xyzz;
#elif _DEBUG_CLUMPCELL
        return v.clumpInfo.wwzz;
#elif _DEBUG_GLOBALWIND
        return v.debug;
#elif _DEBUG_HASH
        return v.debug.wwww;
#else 
    return d.albedo.xyzz;
#endif
#endif
   
}
#endif