Shader "Utility/S_DepthOnly"
{
    Properties
    {
    }
    SubShader
    {
         Tags {"RenderType" = "Opaque"}
        Pass
        {
            Name "DepthOnly"
            Cull back
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 vert(float3 positionOS : POSITION) : SV_POSITION
            {
                return mul(UNITY_MATRIX_MVP,float4(positionOS,1));
            }

            float4 frag(float4 vert : SV_POSITION, out float outDepth : SV_Depth) : SV_Target
            {
                outDepth = vert.z;
                return vert.z;
            }
            ENDHLSL
        }
    }
}
