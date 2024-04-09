Shader "Utility/S_TestOpaqueDepthTexture"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque""RenderPipeline" = "UniversalRenderPipeline"}
        Pass 
        {
            Name "Unlit"
            Cull off
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"  
            struct VertexInput
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_ST;
            TEXTURE2D(_HiZTexture); SAMPLER(sampler_HiZTexture); float4 _HiZTexture_ST;

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(VertexOutput v) : SV_Target
            {
                float4 normal = SAMPLE_TEXTURE2D(_CameraNormalsTexture,sampler_CameraNormalsTexture,v.uv);
                float4 depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,v.uv);
                float4 color = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,v.uv);
                float4 custom = SAMPLE_TEXTURE2D(_HiZTexture,sampler_HiZTexture,v.uv);
                return custom;
            }
            ENDHLSL
        }
    }
}
