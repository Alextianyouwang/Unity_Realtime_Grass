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
                float3 positionOS : TEXCOORD2;
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
                o.positionOS = v.positionOS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float t = v.uv.y;
                float t2 = t * t;
                float t3 = t * t * t;
          
                float4x3 input_4 =
                {
                    _P0.x,_P0.y,_P0.z,
                    _P1.x,_P1.y,_P1.z,
                    _P2.x,_P2.y,_P2.z,
                    _P3.x,_P3.y,_P3.z
                };
          
                float1x4 bernstein_4 =
                {
                    1 - 3 * t + 3 * t2 - 3 * t3,
                    3 * t - 6 * t2 + 3 * t3,
                    3 * t2 - 3 * t3,
                    t3
                };

                float1x4 d_bernstein_4 = {
                    -3 + 6 * t - 9 * t2, 
                    3 - 12 * t + 9 * t2,
                    6 * t - 9 * t2,
                    3 * t2
                };

                float3x3 input_3 =
                {
                    _P1.x,_P1.y,_P1.z,
                    _P2.x,_P2.y,_P2.z,
                    _P3.x,_P3.y,_P3.z
                };

                float1x3 bernstein_3 =
                {
                    3 * t - 6 * t2 + 3 * t3,
                    3 * t2 - 3 * t3,
                    t3
                };

                float1x3 d_bernstein_3 = {
                    3 - 12 * t + 9 * t2,
                    6 * t - 9 * t2,
                    3 * t2
                };

                float2x3 input_2 =
                {
                    _P2.x,_P2.y,_P2.z,
                    _P3.x,_P3.y,_P3.z
                };

                float1x2 bernstein_2 =
                {
                    3 * t2 - 3 * t3,
                    t3
                };

                float1x2 d_bernstein_2 = {
                    6 * t - 9 * t2,
                    3 * t2
                };
                float3 curvePos = mul (bernstein_2, input_2);
                float3 curveTan = mul(d_bernstein_2, input_2);
                float3 curveNormal = normalize(cross(float3(-1, 0, 0), curveTan));
                o.positionCS = TransformObjectToHClip(float3 (v.positionOS.x, curvePos.y, curvePos.z));
                o.normalWS = TransformObjectToWorldNormal(curveNormal);
                
                return o;
            }

            float4 frag(VertexOutput v) : SV_Target
            {
      
                return v.normalWS.xyzz;
            }
            ENDHLSL
        }
    }
}
