Shader "Procedural/S_Simple_IndInst"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct SpawnData
            {
                float3 positionOS;
                float3 normalOS;
                 float3 color;
                float2 uv;
            };
            StructuredBuffer<SpawnData> _SpawnBuffer;
            float4 _Tint;
            float _MasterScale;
            TEXTURE2D( _MainTex);SAMPLER (sampler_MainTex);float4 _MainTex_ST;

            struct VertexInput
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                
            };
            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
            {
                VertexOutput o;

                float3 posOS = v.positionOS;

                SpawnData i = _SpawnBuffer[instanceID];
                float3 posWS = i.positionOS + posOS * _MasterScale ;
                
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                o.uv = i.uv;
                return o;
               
            }

            float4 frag(VertexOutput v, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv);
                return albedo * _Tint;             
            }
            
            ENDHLSL
        }

    }

}
