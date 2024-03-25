Shader "Procedural/S_TileDebug"
{
    Properties
    {
        [KeywordEnum (RandomID,Noise)] _Debug ("Debug View",Float) = 0
        _Alpha ("Alpha", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline"}
        Pass 
        {
            Name "Unlit"
            Cull off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 5.0


            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ _DEBUG_RANDOMID _DEBUG_NOISE
            
            #include "UnityCG.cginc"

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 color : TEXCOORD3;
            };
            
            struct InstanceData {
                float3 position;
                float3 color;
                float size;
            };
            StructuredBuffer<InstanceData> _InstanceDataBuffer;
            StructuredBuffer<int> _TriangleBuffer;
            StructuredBuffer<float3> _VertBuffer;
            StructuredBuffer<float> _NoiseBuffer;
            float _Alpha;
            int _TileDimension;

            
            VertexOutput vert( uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                VertexOutput o;
                InstanceData i = _InstanceDataBuffer[instanceID];
                o.positionWS = _VertBuffer[_TriangleBuffer[vertexID]] * i.size + i.position;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1));
#if _DEBUG_RANDOMID
                o.color = _InstanceDataBuffer[instanceID].color;
#elif _DEBUG_NOISE
                o.color = lerp(float4(0, 1,0,0), float4(1, 0,0,0), _NoiseBuffer[instanceID] + 0.5);
#endif

                return o;
            }
            
            half4 frag(VertexOutput v) : SV_Target
            {
                return  half4 (v.color,_Alpha);
            }
            ENDHLSL
        }
    }
}
