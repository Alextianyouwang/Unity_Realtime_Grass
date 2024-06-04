Shader "Procedural/S_Tile_PBR"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _Normal("Normal", 2D) = "bump" {}
        _MasterScale("MasterScale",Range(0,1)) = 1
    }
    SubShader
    {
         Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalRenderPipeline"}

         Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            Cull back
            HLSLPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT


            #include "HL_Tile_PBR.hlsl"

            ENDHLSL
        }

      //         Pass
      //{
      //    Name "ShadowCaster"
      //    Tags {"LightMode" = "ShadowCaster"}
      //    ColorMask 0
      //    HLSLPROGRAM
      //
      //    #pragma vertex vert
      //    #pragma fragment frag
      //    #pragma target 2.0
      //    #define SHADOW_CASTER_PASS
      //
      //     #include "HL_Tile_PBR.hlsl"
      //    ENDHLSL
      //}
    }

}
