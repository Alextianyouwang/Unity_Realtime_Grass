Shader "Procedural/S_MessProcInd_PBR"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal("Normal", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0, 3)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0
    }
    SubShader
    {
         Tags {"RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline"}

         Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            Cull back
            HLSLPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma require geometry

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #include "HL_MessProcInd_PBR.hlsl"

            ENDHLSL
        }

         Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}
            ColorMask 0
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma require geometry
            #pragma target 2.0
            #define SHADOW_CASTER_PASS

             #include "HL_MessProcInd_PBR.hlsl"
            ENDHLSL
        }
    }

} 