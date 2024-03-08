Shader "Procedural/S_GrassField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct VertexInput
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            
            };
            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            StructuredBuffer<float3> _SpawnBuffer;
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_ST;
            
            VertexOutput vert(VertexInput v, uint instanceID : SV_INSTANCEID)
            {
                VertexOutput o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.positionWS = v.positionOS + _SpawnBuffer[instanceID];
                o.positionCS = TransformObjectToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            
            float4 frag(VertexOutput v) : SV_Target
            {
                float4 color = float4 (v.normalWS,1);
                return color;
            }
            ENDHLSL
        }
    }
}
