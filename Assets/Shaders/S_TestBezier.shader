Shader "Utility/S_TestBezier"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _P0("Control 0", Vector) = (0,0,0,0)
        _P1("Control 1", Vector) = (0,0,0,0)
        _P2("Control 2", Vector) = (0,0,0,0)
        _P3("Control 3", Vector) = (0,0,0,0)
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

           
            float3 _P0;
            float3 _P1;
            float3 _P2;
            float3 _P3;

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float t = v.positionOS.y;
                float t2 = t * t;
                float t3 = t * t * t;

                float4 factor = float4(1, t, t * t, t * t * t);
                const float4x4 character = {
                1,0,0,0,
                -3,3,0,0,
                3,-6,3,0,
                -1,3,-3,1
                };
          
                float4x3 input =
                {
                    _P0.x,_P0.y,_P0.z,
                    _P1.x,_P1.y,_P1.z,
                    _P2.x,_P2.y,_P2.z,
                    _P3.x,_P3.y,_P3.z
                };

                float1x4 bernstein =
                {
                    1 - 3 * t + 3 * t2 - 3 * t3,
                    3 * t - 6 * t2 + 3 * t3,
                    3 * t2 - 3 * t3,
                    t3
                };

                float1x4 d_bernstein = {
                    -3 + 6 * t - 9 * t2, 
                    3 - 12 * t + 9 * t2,
                    6 * t - 9 * t2,
                    3 * t2
                };
                float3 curvePos = mul (bernstein, input);
                float3 curveTan = mul (d_bernstein, input);
                float3 curveNormal = normalize( cross(float3(-1, 0, 0), curveTan));

                o.positionCS = TransformObjectToHClip(float3 (v.positionOS.x, curvePos.y, curvePos.z));
                o.normalWS = TransformObjectToWorldNormal(curveNormal);
                
                return o;
            }

            float4 frag(VertexOutput v) : SV_Target
            {
                return v.normalWS.xyzz;
                return v.uv.xyyy;
            }
            ENDHLSL
        }
    }
}
