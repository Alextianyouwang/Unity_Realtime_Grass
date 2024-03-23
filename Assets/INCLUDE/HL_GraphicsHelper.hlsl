#ifndef GRAPHIC_HELPER_INCLUDE
#define GRAPHIC_HELPER_INCLUDE

#ifndef UNITY_PI
#define UNITY_PI 3.1415926535
#endif
// github.com/GarrettGunnell/Grass/blob/main/Assets/Shaders/ModelGrass.shader
float4 RotateAroundYInDegrees(float4 vertex, float degrees)
{
    float alpha = degrees * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float4(mul(m, vertex.xz), vertex.yw).xzyw;
}
float4 RotateAroundXInDegrees(float4 vertex, float degrees)
{
    float alpha = degrees * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float4(mul(m, vertex.yz), vertex.xw).zxyw;
}

float4 RotateAroundAxis(float4 vertex, float3 axis, float angle)
{
    float radians = angle * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(radians, sina, cosa);

    // Rodrigues' rotation formula
    float3 rotatedVertex = vertex.xyz * cosa +
                           cross(axis, vertex.xyz) * sina +
                           axis * dot(axis, vertex.xyz) * (1 - cosa);

    return float4(rotatedVertex, vertex.w);
}
float4 RotateAroundAxis(float4 vertex, float3 axis, float angle, float3 center)
{

    float3 translatedVertex = vertex.xyz - center;
    float radians = angle * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(radians, sina, cosa);
    
    float3 rotatedTranslatedVertex = translatedVertex * cosa +
                                     cross(axis, translatedVertex) * sina +
                                     axis * dot(axis, translatedVertex) * (1 - cosa);

    float3 rotatedVertex = rotatedTranslatedVertex + center;
    return float4(rotatedVertex, vertex.w);
}

float2 Rotate2D(float2 uv, float angle)
{
    float alpha = angle * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return mul(m, uv);

}

void CubicBezierCurve(float3 P0, float3 P1, float3 P2, float3 P3, float t, out float3 pos, out float3 tangent)
{
    float t2 = t * t;
    float t3 = t * t * t;
    float4x3 input =
    {
        P0.x, P0.y, P0.z,
        P1.x, P1.y, P1.z,
        P2.x, P2.y, P2.z,
        P3.x, P3.y, P3.z
    };

    float1x4 bernstein =
    {
         1 - 3 * t + 3 * t2 - 3 * t3,
         3 * t - 6 * t2 + 3 * t3,
         3 * t2 - 3 * t3,
         t3
    };

    float1x4 d_bernstein =
    {
        -3 + 6 * t - 9 * t2,
        3 - 12 * t + 9 * t2,
        6 * t - 9 * t2,
        3 * t2
    };
    pos = mul(bernstein, input);
    tangent = mul(d_bernstein, input);
}
void CubicBezierCurve_Tilt_Bend(float3 P2, float3 P3, float t, out float3 pos, out float3 tangent)
{
    float t2 = t * t;
    float t3 = t * t * t;
    float1x2 bernstein =
    {
         3 * t2 - 3 * t3,
         t3
    };

    float2x3 input =
    {
        P2.x, P2.y, P2.z,
        P3.x, P3.y, P3.z
    };


    float1x2 d_bernstein =
    {
        6 * t - 9 * t2,
        3 * t2
    };
    pos = mul(bernstein, input);
    tangent = mul(d_bernstein, input);
}

float3 ScaleWithCenter(float3 pos,float scale, float3 center)
{
    pos -= center;
    pos *= scale;
    pos += center;
    return pos;
}
float3 ProjectOntoPlane(float3 v, float3 planeNormal)
{
    return v - dot(v, normalize(planeNormal)) * planeNormal;
}
#endif