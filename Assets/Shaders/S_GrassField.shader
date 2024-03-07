Shader "Procedural/S_GrassField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Range(0,1)) = 1
        _Bend("Bend", Range(0,1)) = 0
        _RandomBendOffset("RandomBendOffset", Range(0,1)) = 0
        _TopColor("TopColor", Color) = (1,1,1,1)
        _BotColor("BotColor", Color) = (0,0,0,1)
        [Toggle(_USE_MAINWAVE_ON)]_USE_MAINWAVE_ON ("Use_Main_Wave", Float) = 1
        _WindSpeed("WindSpeed", Range(0,1)) = 0.5
        _WindFrequency("WindFrequency", Range(0,1)) = 0.5
        _WindDirection("WindDirection", Range(0,1)) = 0
        _WindAmplitude("WindAmplitude", Range(0,1)) = 0.5
        _WindNoiseFrequency("WindNoiseFrequency", Range(0,1)) = 0.5
        _WindNoiseAmplitude("WindNoiseAmplitude", Range(0,1)) = 0.5
        [Toggle(_USE_DETAIL_ON)]_USE_DETAIL_ON("Use_Main_Wave", Float) = 1
        _DetailSpeed("DetailSpeed", Range(0,1)) = 0.5
        _DetailFrequency("DetailFrequency", Range(0,1)) = 0.5
        _DetailAmplitude("DetailAmplitude", Range(0,1)) = 0.5

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

            #pragma shader_feature _ _USE_MAINWAVE_ON
            #pragma shader_feature _ _USE_DETAIL_ON
            
            #include "HL_GrassField.hlsl"
            
            ENDHLSL
        }
    }
}
