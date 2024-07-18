#ifndef FOLIAGE_LIGHTING_MODEL_INCLUDE
#define FOLIAGE_LIGHTING_MODEL_INCLUDE


struct CustomInputData
{
    float3 normalWS;
    float3 groundNormalWS;
    float3 positionWS;
    float3 viewDir;
    float viewDist;
    
    float3 albedo;
    float3 specularColor;
    float smoothness;
    
    float3 sss;
    float sssTightness;

    float3 bakedGI;
    float4 shadowCoord;
};

void FastSSS_float(float3 ViewDir, float3 LightDir, float3 WorldNormal, float3 LightColor, float Flood, float Power, out float3 sss)
{
    const float3 LAddN = LightDir + WorldNormal;
    sss = saturate(pow(saturate(dot(-LAddN, -LAddN * Flood + ViewDir)), Power)) * LightColor;
    
}
float3 CustomLightHandling(CustomInputData d, Light l)
{
    // Shadow in Project Setting set to 30 meters
    float atten = lerp(l.shadowAttenuation, 1, smoothstep(20, 30, d.viewDist)) * l.distanceAttenuation;
    float3 radiance = l.color * atten;
    float diffuse = saturate(dot(l.direction, d.normalWS));
    float diffuseGround = saturate(dot(l.direction, d.groundNormalWS));
    float3 sss = 0;
    FastSSS_float(d.viewDir, l.direction, d.groundNormalWS, l.color, 0, d.sssTightness, sss);
    float3 lv_dir = normalize(l.direction + d.viewDir);
    float specularDot = saturate(dot(d.normalWS, lv_dir));
    float specularDotGround = saturate(dot(d.groundNormalWS, lv_dir));
    float specularBlend = lerp(specularDot, specularDotGround * 0.99, smoothstep(20, 120, d.viewDist));
    float diffuseBlend = diffuseGround * 1 + diffuse * 0.5;
    float specular = pow(abs(specularBlend), d.smoothness) * diffuseBlend;
    float3 phong = saturate(diffuseBlend * d.albedo + specular * d.specularColor);
    return phong * radiance + sss * d.sss * smoothstep(-0.2, 1, diffuseGround) * atten;
}
float3 CustomCombineLight(CustomInputData d)
{
    Light mainLight = GetMainLight(d.shadowCoord);
    MixRealtimeAndBakedGI(mainLight, d.normalWS, d.bakedGI);
    float3 color = d.bakedGI * d.albedo;
    color += CustomLightHandling(d, mainLight);
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++)
        color += CustomLightHandling(d, GetAdditionalLight(lightI, d.positionWS, d.shadowCoord));
    return color;
}


float3 CustomLightHandling_SSS(CustomInputData d, Light l)
{
    float atten = lerp(l.shadowAttenuation, 1, smoothstep(20, 30, d.viewDist)) * l.distanceAttenuation;
    float diffuseGround = saturate(dot(l.direction, d.groundNormalWS));
    float3 sss = 0;
    FastSSS_float(d.viewDir, l.direction, d.groundNormalWS, l.color, 0, d.sssTightness, sss);
    return saturate(sss * d.sss * atten);
}
float3 CustomCombineLight_SSS(CustomInputData d)
{
    Light mainLight = GetMainLight(d.shadowCoord);
    float3 color =  d.albedo;
    color += CustomLightHandling_SSS(d, mainLight);
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++)
        color += CustomLightHandling_SSS(d, GetAdditionalLight(lightI, d.positionWS, d.shadowCoord));
    return color;
}

#endif