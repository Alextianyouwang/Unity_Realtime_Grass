Shader "Procedural/S_TileDebug"
{
    Properties
    {
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
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            
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

            
            VertexOutput vert( uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                VertexOutput o;
                InstanceData i = _InstanceDataBuffer[instanceID];
                o.positionWS = _VertBuffer[_TriangleBuffer[vertexID]] * i.size + i.position;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1));
                o.color = _InstanceDataBuffer[instanceID].color;
                return o;
            }
            
            half4 frag(VertexOutput v) : SV_Target
            {
                return  half4 (v.color,1);
            }
            ENDHLSL
        }
    }
}
