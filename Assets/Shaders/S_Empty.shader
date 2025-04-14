Shader "Procedural/S_Empty"
{
    Properties
    {
        // No properties yet
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
              Name "DepthOnly"
            ZWrite Off
            ZTest Always
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 positionWS = float4(IN.positionOS, 1.0);
                OUT.positionCS = TransformObjectToHClip(positionWS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                clip(-1);
                return float4(1, 0, 0, 1); 
            }

            ENDHLSL
        }
    }
}