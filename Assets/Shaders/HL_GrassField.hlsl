#ifndef GRASSFIELD_GRAPHIC_INCLUDE
#define GRASSFIELD_GRAPHIC_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../INCLUDE/HL_GraphicsHelper.hlsl"
#include "../INCLUDE/HL_Noise.hlsl"
#include "../INCLUDE/HL_CustomLighting.hlsl"

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
    float4 clumpInfo : TEXCOORD4;
   
    
};
struct SpawnData
{
    float3 positionWS;
    float hash;
    float4 clumpInfo;
    float density;
    float wind;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
float3 _ChunkColor,_LOD_Color;
TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;
float _Scale, _WindSpeed, _WindFrequency, _WindNoiseAmplitude, _WindDirection, _WindNoiseFrequency,_RandomBendOffset,_WindAmplitude,
_DetailSpeed, _DetailAmplitude, _DetailFrequency,
_HeightRandomnessAmplitude,
_BladeThickenFactor,
_Tilt, _Height, _Bend, _GrassWaveAmplitude, _GrassWaveFrequency, _GrassWaveSpeed,
_ClumpEmergeFactor, _ClumpHeight, _ClumpHeightSmoothness, _GrassRandomFacing;
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


void CalculateGrassCurve(float t, float lengthMult, float offset,float tiltFactor, out float3 pos, out float3 tan)
{
    float2 tiltHeight = float2(_Tilt, _Height) * lengthMult;
    tiltHeight = Rotate2D(tiltHeight, tiltFactor);
    float2 waveDir = normalize(tiltHeight);
    float propg = dot(waveDir, tiltHeight);
    float grassWave = 0;
    float freq = 5 * _GrassWaveFrequency;
    float amplitude = _GrassWaveAmplitude ;
    float speed = _Time.y * _GrassWaveSpeed * 10;
    for (int i = 0; i < 3; i++)
    {

        grassWave += sin(t * freq - speed + offset) * amplitude * lengthMult;
        freq *= 1.2;
        amplitude *= 0.8;
        speed *= 1.4;
        offset *= 78.233;
    }
    
        
    float2 P3 = tiltHeight;
    float2 P2 = tiltHeight / 2 + normalize(float2(-tiltHeight.y, tiltHeight.x)) * _Bend * lengthMult;
    P2 = float2(P2.x, P2.y) + normalize(float2(-P3.y, P3.x)) * grassWave ;
    P3 = float2(P3.x, P3.y) + normalize(float2(-P3.y, P3.x)) * grassWave;
    CubicBezierCurve_Tilt_Bend(float3(0, P2.y, P2.x), float3(0, P3.y, P3.x), t, pos, tan);
}




VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
{
    VertexOutput o;
    ////////////////////////////////////////////////
    // Fetch Input
    float wind = _SpawnBuffer[instanceID].wind;
    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
    float3 spawnPosWS = _SpawnBuffer[instanceID].positionWS;
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
    wind * 0.5 + 0.5;
    CalculateGrassCurve(uv.y,1 + _HeightRandomnessAmplitude * rand, rand * 2, wind * 60, curvePosOS, curveTangentOS);
    float3 curveNormalOS = normalize(cross(float3(-1, 0, 0), curveTangentOS));
    posOS.yz = curvePosOS.yz;
    ////////////////////////////////////////////////
    
    
    ////////////////////////////////////////////////
    // Apply Transform
    float3 posWS = posOS * _Scale + spawnPosWS;
    float3 curvePosWS = curvePosOS * _Scale + spawnPosWS;
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // Apply Clump
    float windAngle = _WindDirection - rand * 50 * _GrassRandomFacing;
    float clumpAngle = degrees(atan2(dirToClump.x,dirToClump.y)) * clumpHash;
    float rotAngle = lerp(windAngle, clumpAngle, _ClumpEmergeFactor) ;
    rotAngle = windAngle + clumpAngle * _ClumpEmergeFactor;
    float scale = 1 + (_ClumpHeight * 5 - distToClump) * _ClumpHeightSmoothness * clumpHash;
    posWS = ScaleWithCenter(posWS, scale, spawnPosWS);
    posWS = RotateAroundAxis(float4(posWS, 1), float3(0,1,0),rotAngle,spawnPosWS).xyz;
    curvePosWS = ScaleWithCenter(curvePosWS, scale, spawnPosWS);
    curvePosWS = RotateAroundAxis(float4(curvePosWS, 1), float3(0, 1, 0), rotAngle, spawnPosWS).xyz;
    float3 normalWS = RotateAroundYInDegrees(float4(curveNormalOS, 0), rotAngle).xyz;
    ////////////////////////////////////////////////
    
    ////////////////////////////////////////////////
    // Apply Clip Space Adjustment
    float offScreenFactor = smoothstep(0.2, 1, 1 - abs(dot(normalWS, normalize(_WorldSpaceCameraPos - posWS))));
    float4 posCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
    float4 curvePosCS = mul(UNITY_MATRIX_VP, float4(curvePosWS, 1));
    float4 normalCS = mul(UNITY_MATRIX_VP, float4(normalWS, 0));
    float2 shiftDist = posCS.xy - curvePosCS.xy;
    float2 projectedFlat = normalize(dot(shiftDist, float2(1, 1)));
    float2 projectedSmooth = clamp(-1, dot(shiftDist, normalCS.xy ) * normalCS.xy * 600, 1);
    //float viewDist = length(_WorldSpaceCameraPos - posWS);
    //float mask = 1 - smoothstep(20, 40, viewDist);
    //float2 shiftFactor = lerp(projectedFlat,projectedSmooth,mask) * _BladeThickenFactor * offScreenFactor * 0.05;
    float2 shiftFactor = projectedSmooth * _BladeThickenFactor * offScreenFactor * 0.05;

    posCS.xy += length(shiftDist) > 0.0001 ?shiftFactor : 0;
    ////////////////////////////////////////////////
    
    
    
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.positionWS = posWS;
    o.normalWS = normalWS;
    o.clumpInfo = _SpawnBuffer[instanceID].clumpInfo;
    o.debug = float4(dirToClump,0, 0);
    o.positionCS = posCS;
    return o;
    
   // #if  _USE_MAINWAVE_ON
   //     float wave = SinWaveWithNoise(spawnPosWS.xz , _WindDirection, _WindNoiseFrequency, _WindNoiseAmplitude, _WindSpeed, _WindFrequency) ;
   //     //wave = _SpawnBuffer[instanceID].wind;
   // #else
   //     float wave = 0;
   // #endif
   // 
   // 
   // #if _USE_DETAIL_ON
   //     float detail = Perlin(Rotate2D(spawnPosWS.xz, _WindDirection * 360) * _DetailFrequency * 10 - _Time.y * _DetailSpeed * 10) ;
   // #else
   //     float detail = 0;
   // #endif
   // 
   // 
   // #if _USE_RANDOM_HEIGHT_ON
   //     float heightPerlin = Perlin((spawnPosWS.xz) * 20)*_HeightRandomnessAmplitude;
   // #else
   //     float heightPerlin  = 0;
   // #endif
    

    //float3 pos = RotateAroundYInDegrees(float4(v.positionOS, 1), rand * 360).xyz;
    //float2 rotatedWindDir = Rotate2D(float2(1, -1), _WindDirection * 360);
    //pos = RotateAroundAxis(float4(pos, 1), float3(rotatedWindDir.x, 0, rotatedWindDir.y),
    //    v.uv.y * ((_Bend * 20 + rand * _RandomBendOffset * 20) + (wave * _WindAmplitude * 20 + (wave / 2 + 0.75) * detail * _DetailAmplitude * 20))).xyz;
    //pos *= _Scale + heightPerlin;

    

}

float4 frag(VertexOutput v) : SV_Target
{
    float4 color = lerp(_BotColor,_TopColor ,v.uv.y);
    float3 normal = normalize(v.normalWS);
    float3 finalColor;
    
   Light mainLight = GetMainLight();
    float nDotl = saturate(dot(mainLight.direction, normal));

#if _DEBUG_OFF
        return color;
        return v.debug.xyzw;
#elif _DEBUG_MAINWAVE
        return v.debug;
#elif _DEBUG_DETAILEDWAVE
        return v.debug.y;
#elif _DEBUG_CHUNKID
        return _ChunkColor.xyzz;
#elif _DEBUG_LOD
        return _LOD_Color.xyzz;
#elif _DEBUG_CLUMPCELL
        return v.clumpInfo.wwzz;
#elif _DEBUG_GLOBALWIND
        return v.debug.w;
#else 

    return color;
#endif
}
#endif