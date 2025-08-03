#ifndef MESSPROCIND_PBR_INCLUDE
#define MESSPROCIND_PBR_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../INCLUDE/HL_GraphicsHelper.hlsl"
#include "../INCLUDE/HL_Noise.hlsl"
#include "../INCLUDE/HL_ShadowHelper.hlsl"

struct SpawnData
{
    float3 positionOS;
    float3 normalOS;
    float3 color;
    float2 uv;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
StructuredBuffer<uint> _IndexBuffer;

struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float4 tangentWS : TEXCOORD3;
    float3 positionWS : TEXCOORD4;
    float3 bakedGI : TEXCOORD8;
    float3 color : TEXCOORD9;
};

TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);
TEXTURE2D( _Normal);SAMPLER (sampler_Normal);
float4 _MainTex_ST;
float _NormalScale;
float4 _Tint;
float _Smoothness;
float _Metallic;

VertexOutput vert(uint vertexID : SV_VertexID)
{
    VertexOutput o;
    SpawnData i = _SpawnBuffer[_IndexBuffer[vertexID]];
    
    // Apply Transform
    float3 posWS = i.positionOS;
    float3 normalWS = i.normalOS;
    
    ////////////////////////////////////////////////
    // GI
    float2 lightmapUV = 0;
    float3 vertexSH;
    OUTPUT_SH(normalWS, vertexSH);
    ////////////////////////////////////////////////

    o.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, normalWS);
    o.uv = TRANSFORM_TEX(i.uv, _MainTex);
    o.positionWS = posWS;
    o.normalWS = normalWS;
    o.tangentWS = float4(1, 0, 0, 1); // Default tangent
    o.color = i.color;

    #ifdef SHADOW_CASTER_PASS
        o.positionCS = CalculatePositionCSWithShadowCasterLogic(posWS, normalWS);
    #else
        o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
    #endif
    return o;
   
}


float4 frag(VertexOutput v, bool frontFace : SV_IsFrontFace) : SV_Target
{
    float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv);
    albedo.xyz *= v.color; // Apply vertex color

    float3 normalWS = normalize(v.normalWS);
    float3 tangentWS = normalize(v.tangentWS).xyz;
    float3 bitangentWS = cross(normalWS, tangentWS);
    float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, v.uv), _NormalScale);
  

    float sgn = v.tangentWS.w; // should be either +1 or -1
    float3 bitangent = sgn * cross(v.normalWS.xyz, v.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(v.tangentWS.xyz, bitangent.xyz, v.normalWS.xyz);
    normalWS = mul(normalTS, tangentToWorld);
   
    float3 posNDS = v.positionCS.xyz / v.positionCS.w;
    float2 uvSS = posNDS.xy / 2 + 0.5;
    
    InputData data = (InputData) 0;
    
    data.positionWS = v.positionWS;
    data.positionCS = v.positionCS;
    data.normalWS = normalWS;
    data.viewDirectionWS = normalize(_WorldSpaceCameraPos - v.positionWS);
    data.shadowCoord = CalculateShadowCoord(v.positionWS, v.positionCS);
    data.fogCoord = 0;
    data.vertexLighting = 0;
    data.bakedGI = v.bakedGI;
    data.normalizedScreenSpaceUV = uvSS;
    data.shadowMask = 0;
    data.tangentToWorld = tangentToWorld;
    
    SurfaceData surf = (SurfaceData) 0;
    
    surf.albedo = albedo.xyz * _Tint.xyz;
    surf.specular = 0;
    surf.metallic = _Metallic;
    surf.smoothness = _Smoothness;
    surf.normalTS = normalTS;
    surf.emission = 0;
    surf.occlusion = 1;
    surf.alpha = albedo.w;
    surf.clearCoatMask = 0;
    surf.clearCoatSmoothness = 0;

    float4 finalColor = UniversalFragmentPBR(data, surf);
    return finalColor;

   
}

#endif 