Shader "Custom/S_TransparentTest"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 0.5
        _TopColor("TopColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline"}
        Pass 
        {
            Name "Unlit"
            Cull off
            zWrite off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 5.0


            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
            };
            
            struct Input {
                float3 positionWS : POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Alpha;
            float4 _TopColor;

            VertexOutput vert(Input i)
            {
                VertexOutput o = (VertexOutput)0;
                o.positionWS = i.positionWS;
                
                o.positionCS = mul(UNITY_MATRIX_MVP, float4(i.positionWS, 1));

                return o;
            }
            
            half4 frag(VertexOutput v) : SV_Target
            {
                return  half4 (_TopColor.xyz,_Alpha);
            }
            ENDHLSL
        }
    }
}
