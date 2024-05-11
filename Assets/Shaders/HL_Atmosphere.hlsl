#ifndef ATMOSPHERE_INCLUDE
#define ATMOSPHERE_INCLUDE
uniform float4 _BlitScaleBias;
sampler2D _CameraOpaqueTexture, _CameraDepthTexture,_OpticalDepthTexture;
float _Camera_Near, _Camera_Far, _EarthRadius;
uint _NumInScatteringSample, _NumOpticalDepthSample;

float _Rs_Absorbsion, _Rs_Thickness, _Rs_DensityFalloff, _Rs_DensityMultiplier, _Rs_ChannelSplit;
float4 _Rs_ScatterWeight, _Rs_InsColor;

float _Ms_Absorbsion, _Ms_Thickness, _Ms_DensityFalloff, _Ms_DensityMultiplier, _Ms_Anisotropic;
float4 _Ms_InsColor;

bool _VolumeOnly = 0;

#include "../INCLUDE/HL_AtmosphereHelper.hlsl"


struct appdata
{
    float4 positionOS : POSITION;
    uint vertexID : SV_VertexID;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    float3 viewDir : TEXCOORD1;
};


v2f vert(appdata input)
{
    v2f output;

#if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv = input.uv;
#else
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
#endif

    output.positionCS = pos;
    output.uv = uv;
    float3 viewVector = mul(unity_CameraInvProjection, float4(uv.xy * 2 - 1, 0, -1)).xyz;
    output.viewDir = mul(unity_CameraToWorld, float4(viewVector, 0)).xyz;
    return output;
}
inline float LinearEyeDepth(float depth)
{
    // Reversed Z
    depth = 1 - depth;
    float x = 1 - _Camera_Far / _Camera_Near;
    float y = _Camera_Far / _Camera_Near;
    float z = x / _Camera_Far;
    float w = y / _Camera_Far;
    return 1.0 / (z * depth + w);
}

float4 OpticalDepthBaked(float3 rayOrigin, float3 rayDir)
{
    float3 relativeDistToCenter = rayOrigin - float3(0, -_EarthRadius, 0);
    float distAboveGround = length(relativeDistToCenter) - _EarthRadius;
    float height01 = distAboveGround / _Rs_Thickness;
    float zenithAngle = dot(normalize(relativeDistToCenter), rayDir);
    float angle01 = 1 - (zenithAngle * 0.5 + 0.5);
    float4 opticalDepth = tex2D(_OpticalDepthTexture, float2(angle01,height01));
    return opticalDepth;
}

void AtmosphereicScattering(float3 rayOrigin, float3 rayDir, float3 sunDir, float distance,
 out float3 inscatteredLight, out float3 transmittance)
{
    float stepSize = distance / (_NumInScatteringSample - 1);
    float3 samplePos = rayOrigin;
#if _USE_RAYLEIGH
    float rs_phase = PhaseFunction(dot(sunDir, rayDir), 0);
#else
    float rs_phase = 0;
#endif
#if _USE_MIE
    float ms_phase = PhaseFunction(dot(sunDir, rayDir), _Ms_Anisotropic);
#else
    float ms_phase = 0;
#endif
    float3 rs_scatteringWeight = lerp(length(_Rs_ScatterWeight.xyz) / 3, _Rs_ScatterWeight.xyz, _Rs_ChannelSplit) * _Rs_Absorbsion;
    float ms_scatteringWeight = _Ms_Absorbsion / 100;
    float rs_viewRayOpticalDepth = 0;
    float ms_viewRayOpticalDepth = 0;
    float3 rs_inscatterLight = 0;
    float3 ms_inscatterLight = 0;
    for (uint i = 0; i < _NumInScatteringSample; i++)
    {
#if _USE_REALTIME
         float3 earthCenter = float3(0, -_EarthRadius, 0);
        float sunRayLength = RaySphere(float3(0, -_EarthRadius, 0), _EarthRadius + _Rs_Thickness, samplePos, sunDir).y;
#else
        float4 opticalDepthData = OpticalDepthBaked(samplePos, sunDir);
#endif
        
#if _USE_RAYLEIGH
    #if _USE_REALTIME
        float rs_localDensity = LocalDensity(samplePos,earthCenter, _Rs_Thickness, _Rs_DensityFalloff) *stepSize * _Rs_DensityMultiplier;
        float rs_sunRayOpticalDepth = OpticalDepth(samplePos, sunDir, sunRayLength,earthCenter , _Rs_Thickness,_Rs_DensityFalloff) * _Rs_DensityMultiplier;
    #else
        float rs_localDensity = opticalDepthData.y * stepSize* _Rs_DensityMultiplier;
        float rs_sunRayOpticalDepth = opticalDepthData.x;
    #endif
        rs_viewRayOpticalDepth += rs_localDensity;
        float3 rs_tau = (rs_viewRayOpticalDepth + rs_sunRayOpticalDepth) * rs_scatteringWeight;
#else
        float3 rs_tau = 0;
        float rs_localDensity = 0;
#endif
#if _USE_MIE
    #if _USE_REALTIME
        float ms_localDensity = LocalDensity(samplePos, earthCenter,_Ms_Thickness, _Ms_DensityFalloff) * stepSize * _Ms_DensityMultiplier;
        float ms_sunRayOpticalDepth = OpticalDepth(samplePos, sunDir, sunRayLength,earthCenter, _Ms_Thickness, _Ms_DensityFalloff) *_Ms_DensityMultiplier;
    #else
        float ms_localDensity = opticalDepthData.w * stepSize * _Ms_DensityMultiplier;
        float ms_sunRayOpticalDepth =  opticalDepthData.z;
    #endif
        ms_viewRayOpticalDepth += ms_localDensity;
        float3 ms_tau = (ms_sunRayOpticalDepth + ms_viewRayOpticalDepth) * ms_scatteringWeight;
#else
        float3 ms_tau = 0;
        float ms_localDensity = 0;
#endif
        
        float3 totalTransmittance = exp(-rs_tau - ms_tau);
        
        rs_inscatterLight += totalTransmittance * rs_localDensity;
        ms_inscatterLight += totalTransmittance * ms_localDensity;
        samplePos += rayDir * stepSize;
    }
    rs_inscatterLight *=  rs_scatteringWeight * _Rs_InsColor.xyz;
    ms_inscatterLight *= ms_phase * ms_scatteringWeight * _Ms_InsColor.xyz;
    transmittance = exp(-rs_viewRayOpticalDepth * rs_scatteringWeight - ms_viewRayOpticalDepth * ms_scatteringWeight);
    inscatteredLight = ms_inscatterLight + rs_inscatterLight;
}

float4 frag(v2f i) : SV_Target
{
    float3 rayOrigin = _WorldSpaceCameraPos;
    float3 rayDir = normalize(i.viewDir); 
    float4 col = tex2D(_CameraOpaqueTexture, i.uv);

    float3 forward = mul((float3x3) unity_CameraToWorld, float3(0, 0, 1));
    float sceneDepthNonLinear = tex2D(_CameraDepthTexture, i.uv).x;
    float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) /dot(rayDir, forward);

    Light mainLight = GetMainLight();

    float2 hitInfo = RaySphere(float3(0, -_EarthRadius, 0), _EarthRadius + _Rs_Thickness, rayOrigin, rayDir);
    float distThroughVolume = min(hitInfo.y, max(sceneDepth - hitInfo.x, 0));
    float3 marchStart = rayOrigin + rayDir * (hitInfo.x+ 0.01);
    float3 inscatteredLight;
    float3 transmittance;
    AtmosphereicScattering(marchStart, rayDir, mainLight.direction, distThroughVolume, inscatteredLight,transmittance);

    float3 finalCol = _VolumeOnly ? inscatteredLight : inscatteredLight + transmittance * col.xyz;
    return finalCol.xyzz;
}
#endif