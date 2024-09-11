Shader "GrassUtility/S_FlexTileDebug"
{
    Properties
    {
        [KeywordEnum (Default,Buffer)] _Debug ("Debug View",Float) = 0
        _Alpha ("Alpha", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent"  "RenderPipeline" = "UniversalPipeline"}
        LOD 300

        Pass 
        {
            Name "Unlit"
            ZTest always
            ZWrite off
            Cull off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 2.0


            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _DEBUG_DEFAULT _DEBUG_BUFFER
            
            #include "UnityCG.cginc"

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 color : TEXCOORD3;
            };
            
            struct InstanceData {
                float2 position;
                float size;
                float3 color;
            };
            StructuredBuffer<InstanceData> _InstanceDataBuffer;
            StructuredBuffer<int> _TriangleBuffer;
            StructuredBuffer<float3> _VertBuffer;

            float _Alpha;
            uint _MaxInstance;
            
            VertexOutput vert( uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                VertexOutput o = (VertexOutput)0;

                InstanceData i = _InstanceDataBuffer[instanceID];
                o.positionWS = _VertBuffer[_TriangleBuffer[vertexID]] * i.size + float3 (i.position.x,0,i.position.y);
                o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1));
                #if _DEBUG_DEFAULT
                    o.color = float3 (1,0,0);
                #elif _DEBUG_BUFFER
                    o.color = _InstanceDataBuffer[instanceID].color;
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
        FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
