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
    // Convert angle from degrees to radians
    float radians = angle * UNITY_PI / 180.0;

    // Compute sine and cosine of the angle
    float sina, cosa;
    sincos(radians, sina, cosa);

    // Rodrigues' rotation formula
    float3 rotatedVertex = vertex.xyz * cosa +
                           cross(axis, vertex.xyz) * sina +
                           axis * dot(axis, vertex.xyz) * (1 - cosa);

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
#endif