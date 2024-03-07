Shader "Procedural/S_GrassField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Range(0,1)) = 1
        _Bend("Bend", Range(0,1)) = 0
        _TopColor("TopColor", Color) = (1,1,1,1)
        _BotColor("BotColor", Color) = (0,0,0,1)
        _WindSpeed("WindSpeed", Range(0,1)) = 0.5
        _WindFrequency("WindFrequency", Range(0,1)) = 0.5
        _WindDirection("WindDirection", Range(0,1)) = 0
        _WindNoiseFrequency("WindNoiseFrequency", Range(0,1)) = 0.5
        _WindNoiseWeight("WindNoiseWeight", Range(0,1)) = 0.5
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
            
            #include "HL_GrassField.hlsl"
            
            ENDHLSL
        }
    }
}
