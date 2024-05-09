Shader "Procedural/S_PampasField"
{
    Properties
    {

        _MainTex ("Texture", 2D) = "white" {}
        _Normal("Normal", 2D) = "bump" {}
        _NormalScale("NormalScale",Range(0,3)) = 1
        _MasterScale("MasterScale",Range(0,1)) = 1
        _RandomScale("RandomScale",Range(0,1)) = 1
        _RandomFacing("RandomFacing",Range(0,1)) = 1
        _ClumpTightness("ClumpTightness",Range(0,1)) = 0.5
        _SSSColor("SSSColor", Color) = (0,0,0,1)
        _SSSTightness("SSSTightness",Range(0,1)) = 0.1

        _GrassWaveAmplitude("GrassWaveAmplitude",Float) = 1
        _GrassWaveFrequency("GrassWaveFrequency",Float) = 1
        _GrassWaveSpeed("GrassWaveSpeed",Float) = 1

    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalRenderPipeline"}
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

            
            #include "HL_Pampas.hlsl"
            
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
           #include "HL_Pampas.hlsl"
           ENDHLSL
       }
    }  
}
