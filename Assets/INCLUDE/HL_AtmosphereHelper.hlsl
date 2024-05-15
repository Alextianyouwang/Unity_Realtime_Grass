#ifndef ATMOSPHERE_HELPER_INCLUDED
#define ATMOSPHERE_HELPER_INCLUDED
#define MAX_DISTANCE 10000

//developer.nvidia.com/gpugems/gpugems2/part-ii-shading-lighting-and-shadows/chapter-16-accurate-atmospheric-scattering
float PhaseFunction(float costheta, float g)
{
    float g2 = g * g;
    float symmetry = (3 * (1 - g2)) / (2 * (2 + g2));
    return (1 + costheta * costheta) / pow(abs(1 + g2 - 2 * g * costheta), 1.5);

}

float2 RaySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir)
{
    float3 offset = rayOrigin - sphereCentre;
    float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
    float b = 2 * dot(offset, rayDir);
    float c = dot(offset, offset) - sphereRadius * sphereRadius;
    float d = b * b - 4 * a * c; // Discriminant from quadratic formula

		// Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
    if (d > 0)
    {
        float s = sqrt(d);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToSphereFar = (-b + s) / (2 * a);

			// Ignore intersections that occur behind the ray
        if (dstToSphereFar >= 0)
        {
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
        }
    }
		// Ray did not intersect sphere
    return float2(MAX_DISTANCE, 0);
}

float LocalDensity(float3 pos, float3 offset,float relativeHeight, float falloff)
{
    float distToCenter = length(pos - offset);
    float heightAboveSurface = max(distToCenter - _EarthRadius, 0);
    float height01 = clamp(heightAboveSurface / relativeHeight, 0, 1);
    return exp(-height01 * falloff) * (1 - height01);

}
float OpticalDepth(float3 rayOrigin, float3 rayDir, float rayLength, float3 offset, float relativeHeight, float falloff)
{
    float3 densitySamplePoint = rayOrigin;
    float stepSize = rayLength / (_NumOpticalDepthSample - 1);
    float opticalDepth = 0;

    for (uint i = 0; i < _NumOpticalDepthSample; i++)
    {
        float localDensity = LocalDensity(densitySamplePoint, offset, relativeHeight, falloff);
        opticalDepth += localDensity * stepSize;
        densitySamplePoint += rayDir * stepSize;
    }
    return opticalDepth;
}



float SphereMask(float3 center, float radius, float falloff, float3 position, out float ring )
{
    float mask0 = smoothstep(radius - falloff, radius, distance(position, center));
    float mask1 = smoothstep(radius, radius + falloff, distance(position, center));
    ring = mask0 - mask1;
    return mask0;

}

#endif