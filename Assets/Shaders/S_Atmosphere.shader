Shader "Hidden/S_Atmosphere"
{
    Properties
    {
    }
    SubShader
    {
        Tags{"RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "BlitAtmosphere"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

#pragma shader_feature_local _ _USE_MIE
#pragma shader_feature_local _ _USE_RAYLEIGH
#pragma shader_feature_local _ _USE_REALTIME

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "HL_Atmosphere.hlsl"


            ENDHLSL
        }

        Pass 
        {
            Name "BlitDistortedVolume"


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
       

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "HL_VolumeDistorsion.hlsl"
            ENDHLSL
        }
    }
}
