Shader "Procedural/S_MeshIndirectSimple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", Range(0,1)) = 1
        [Toggle(_USE_CHUNKID_ON)]_USE_CHUNKID_ON("Show ChunkID Color", Float) = 0
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        Pass 
        {
            Name "Unlit"
            Cull off
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _USE_CHUNKID_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct VertexInput
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            StructuredBuffer<float3> _SpawnBuffer;
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_ST;
            float _Scale;
            float3 _ChunkColor;
            
            VertexOutput vert(VertexInput v, uint instanceID : SV_InstanceID)
            {
                VertexOutput o;
                o.positionWS = v.positionOS * _Scale + _SpawnBuffer[instanceID];
                o.positionCS = TransformObjectToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            
            float4 frag(VertexOutput v) : SV_Target
            {
#ifdef _USE_CHUNKID_ON
                return float4 (_ChunkColor, 1);
#else
                return float4 (v.normalWS / 2 + 0.5, 1);
#endif
            }
            ENDHLSL
        }
    }

}
