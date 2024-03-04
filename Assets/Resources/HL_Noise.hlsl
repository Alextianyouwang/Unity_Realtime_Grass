#ifndef NOISE
#define NOISE

float nrand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}
float3 RandomPointInTriangle(float3 a, float3 b, float3 c, float randSeed)
{

	float r1 = nrand((float)randSeed * 0.002323);
	float r2 = nrand((float)randSeed * 0.0032676);

	if (r1 + r2 >= 1)
	{
		r1 = 1 - r1;
		r2 = 1 - r2;
	}
	float r3 = 1 - r1 - r2;
	float3 randomPoint = r1 * a + r2 * b + r3 * c;
	return randomPoint;
}
float3 GetTriangleCenter(float3 a, float3 b, float3 c)
{
	return (a + b + c) / 3;
}
float3 GetQuadCenter(float3 a, float3 b, float3 c, float3 d)
{
	return (a + b + c + d) / 4;
}

#endif