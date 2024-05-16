Shader "Procedural/S_TileStripeFX"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 0.5
        _Tint("Tint", Color) = (1,1,1,1)
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
            #include "../INCLUDE/HL_GraphicsHelper.hlsl"

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float4 color : TEXCOORD3;
                float4 gradient : TEXCOORD4;
                float3 normalWS :TEXCOORD5;
            };
            
              struct Input {
                  float3 positionWS : POSITION;
                  float2 uv : TEXCOORD0;
                  float3 normalOS : NORMAL;
              };

              struct InstanceData {
                  float3 position;
                  float size;
                  int side;
              };
            StructuredBuffer<InstanceData> _InstanceDataBuffer;
            StructuredBuffer<float4> _MaskBuffer;
            float _Alpha;
            float4 _Tint;

            VertexOutput vert(Input i, uint instanceID : SV_InstanceID)
            {
                VertexOutput o = (VertexOutput)0;
                InstanceData data = _InstanceDataBuffer[instanceID];

                float3 posWS = i.positionWS + data.position + float3 (0,0,0.2);
                posWS = ScaleWithCenter(posWS, float3(0.4, 10, 1), data.position);
                posWS = RotateAroundAxis(float4(posWS, 1), float3 (0, 1, 0), 90 * data.side, data.position).xyz;

                float3 normalWS = mul(UNITY_MATRIX_VP, float4(i.normalOS, 0));
                normalWS = RotateAroundAxis(float4(normalWS, 0), float3 (0, 1, 0), 90 * data.side).xyz;
                o.positionWS = posWS;

                o.gradient = float4 (posWS.y - data.position.y, 0, 0, 0);
                
                o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1));

                float4 color = _MaskBuffer[instanceID / 4].xxxx;
                o.normalWS = normalWS;
                o.color = color;
                return o;
            }
            
            float4 frag(VertexOutput v) : SV_Target
            {
                float alpha = v.color.x < 0.5 ? 0 : _Alpha;
            float gradient = 5- v.gradient.x;
            float3 color = gradient * _Tint;
            float3 normalWS = v.normalWS;
            float4 finalColor = float4 (color, alpha * gradient * 0.5);
            //finalColor = float4 (normalWS, alpha);
                return  finalColor;
            }
            ENDHLSL
        }
    }
}
