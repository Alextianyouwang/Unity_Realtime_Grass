Shader "Procedural/S_GrassField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Range(0,1)) = 1
        _Bend("Bend", Range(0,1)) = 0
        _TopColor("_TopColor", Color) = (1,1,1,1)
        _BotColor("_BotColor", Color) = (0,0,0,1)
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
