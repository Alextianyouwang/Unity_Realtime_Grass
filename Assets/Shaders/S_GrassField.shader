Shader "Procedural/S_GrassField"
{
    Properties
    {
        [KeywordEnum(Off,ChunkID,LOD,ClumpCell,GlobalWind,Hash,Interaction)] _Debug("DebugMode", Float) = 0

   
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _Normal("Normal", 2D) = "bump" {}
         [Header(Shading)][Space]
        _TopColor("TopColor", Color) = (1,1,1,1)
        _BotColor("BotColor", Color) = (0,0,0,1)
        _ClumpTopThreshold("ClumpTopThreshold",Range(0,1)) = 0.1
        _VariantTopColor("ClumpTopColor", Color) = (1,0,0,1)
        [HDR]_SpecularColor("SpecularColor", Color) = (1,1,1,1)
        _SpecularTightness("SpecularTightness",Range(0,1)) = 0.5
        _NormalScale("NormalScale",Range(0,1)) = 0.5

        [Header(Transform)][Space]
        _GrassScale("GrassMasterScale", Range(0,1)) = 1
        _GrassFacingDirection("GrassFacingDirection", Range(0,360)) = 0
        _GrassRandomLength("GrassRandomLength", Range(0,1)) = 0.5
        _GrassRandomFacing("GrassRandomFacing",Range(0,1)) = 0.1


         [Header(Bezier Curve)][Space]
        _GrassTilt("GrassTilt",Float) = 1
        _GrassHeight("GrassHeight",Float) = 1
        _GrassBend("GrassBend",Range(-1,1)) = 0


        [Header(Detial Movement)][Space]
        _GrassWaveAmplitude("GrassWaveAmplitude",Range(0,1)) = 0.1
        _GrassWaveFrequency("GrassWaveFrequency",Range(0,1)) = 0.1
        _GrassWaveSpeed("GrassWaveSpeed",Range(0,1)) = 0.1

        [Header(Clumping)][Space]
        _ClumpEmergeFactor("ClumpEmergeFactor",Range(0,1)) = 0.1
        _ClumpThreshold("ClumpThreshold",Range(0,1)) = 0.1
        _ClumpHeightOffset("ClumpHeightOffset",Range(0,1)) = 0.1
        _ClumpHeightMultiplier("ClumpHeightMultiplier",Range(0,1)) = 0.1


        [Header(Utility)][Space]
        _BladeThickenFactor("ViewSpaceThickenFactor",Range(0,1)) = 0

    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        Pass 
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            Cull off
            HLSLPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
      
            #pragma shader_feature _DEBUG_OFF _DEBUG_CHUNKID _DEBUG_LOD _DEBUG_CLUMPCELL _DEBUG_GLOBALWIND _DEBUG_HASH _DEBUG_INTERACTION
            
            #include "HL_GrassField.hlsl"
            
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
           #pragma target 2.0
           #define SHADOW_CASTER_PASS
           #include "HL_GrassField.hlsl"
           ENDHLSL
       }
    }  
}
