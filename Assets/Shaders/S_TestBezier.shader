Shader "Utility/S_TestBezier"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _P0("Control 0", Vector) = (0,0,0,0)
        _P1("Control 1", Vector) = (0,0,0,0)
        _P2("Control 2", Vector) = (0,0,0,0)
        _P3("Control 3", Vector) = (0,0,0,0)

        _WaveMagnitude("Wave Magnitude", Range(0,1)) = 0.5
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
            #include "../INCLUDE/HL_GraphicsHelper.hlsl"
  
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
                float4 debug : TEXCOORD3;
            };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_ST;

           
            float3 _P0;
            float3 _P1;
            float3 _P2;
            float3 _P3;
            float _WaveMagnitude;

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.positionOS = v.positionOS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
             
                float2 waveDir = normalize(float2(_P3.z,_P3.y));
                float propg =  dot(waveDir, float2(_P3.z, _P3.y));
                float wave = sin(o.uv.y* 4 - _Time.y * 5);
                float2 P2 = float2(_P2.z, _P2.y) + normalize(float2(-_P3.y, _P3.z)) * (wave) * _WaveMagnitude * o.uv.y;
                float2 P3 = float2(_P3.z, _P3.y)+ normalize(float2(-_P3.y, _P3.z)) * (wave)*_WaveMagnitude * o.uv.y * 2;
                o.debug = wave;

                float3 curvePos = 0;
                float3 curveTan = 0;
                CubicBezierCurve_Tilt_Bend(float3(0,P2.y,P2.x), float3(0, P3.y , P3.x ), o.uv.y, curvePos, curveTan);
                float3 curveNormal = normalize(cross(float3(-1, 0, 0), curveTan));

                o.positionCS = TransformObjectToHClip(float3 (v.positionOS.x, curvePos.y, curvePos.z));
                o.normalWS = TransformObjectToWorldNormal(curveNormal);
                
                return o;
            }

            float4 frag(VertexOutput v) : SV_Target
            {
                return v.debug;
                return v.normalWS.xyzz;
            }
            ENDHLSL
        }
    }
}
