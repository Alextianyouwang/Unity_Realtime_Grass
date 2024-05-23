using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[DefaultExecutionOrder (-101)]
public class ParameterHolder : MonoBehaviour
{
    public VolumeProfile VolumeObject;
    private VolumetricAtmosphereBlendingComponent _setting;
    private ChromaticAberration _aberration;
    private DepthOfField _dof;
    public WindSimTest FlowBufferHolder;
    public MaskBufferManager MaskBufferHolder;
    public float AberrationIntensity;
    public float Dof;


    [Header("Spherical Mask")]
    public Vector3 SphereMaskCenter;
    public float SphereMaskRadius;
    public float SphereMaskFalloff;

    public float DistorsionStrength = 0.2f;


    [Header("Stripe FX")]
    public TileStripeFXComponent TileStripeFXComponent;
    public bool ToggleTileStripeFX;

    public int StripeFXSampleSize;
    public int StripeFXSeed;
    [Range (0f,1f)]
    public float StripeFXStep;

    public Material StripeFXMat;
    [Range(0f, 1f)]
    public float StripeAlpha;

    [Header("Atmosphere")]
    [Range(0f,3f)]
    public float AtmosphereDensityMultiplier_1 = 3;
    [ColorUsage(true, true)]
    public Color AtmosphereInscatteringTint_1;
    [Range(0f, 1f)]
    public float AtmosphereUniformAbsorbsion_1;
    public Color AtmosphereAbsorbsionWeightPerChannel_1;
    [Range(0f, 1f)]
    public float AtmosphereChannelSplit_1;

    [Range(0f, 3f)]
    public float AtmosphereDensityMultiplier_2;
    [ColorUsage(true,true)]
    public Color AtmosphereInscatteringTint_2;
    [Range(0f, 1f)]
    public float AtmosphereUniformAbsorbsion_2;
    public Color AtmosphereAbsorbsionWeightPerChannel_2;
    [Range(0f, 1f)]
    public float AtmosphereChannelSplit_2;

    [Range(0f, 3f)]
    public float AerosolsDensityMultiplier_1;
    [ColorUsage(true, true)]
    public Color AerosolsInscatteringTint_1;
    [Range(0f, 1f)]
    public float AerosolsUniformAbsorbsion_1;
    [Range(0f, 1f)]
    public float AerosolsAnistropic_1;
    [Range(0f, 3f)]
    public float AerosolsDensityMultiplier_2;
    [ColorUsage(true, true)]
    public Color AerosolsInscatteringTint_2;
    [Range(0f, 1f)]
    public float AerosolsUniformAbsorbsion_2;
    [Range(0f, 1f)]
    public float AerosolsAnistropic_2;
    private void OnEnable()
    {
        VolumeObject.TryGet(out _setting);
        VolumeObject.TryGet(out _aberration);
        VolumeObject.TryGet(out _dof);

    }
    void LateUpdate()
    {
        _setting.BlendCenter.value = SphereMaskCenter;
        _setting.BlendRaidus.value = SphereMaskRadius;
        _setting.BlendFalloff.value = SphereMaskFalloff;

        _setting.BlendDistortStrength.value = DistorsionStrength;

        _setting.AtmosphereDensityMultiplier_1.value = AtmosphereDensityMultiplier_1;
        _setting.AtmosphereInscatteringTint_1.value = AtmosphereInscatteringTint_1;
        _setting.AtmosphereUniformAbsorbsion_1.value = AtmosphereUniformAbsorbsion_1;
        _setting.AtmosphereAbsorbsionWeightPerChannel_1.value = AtmosphereAbsorbsionWeightPerChannel_1;
        _setting.AtmosphereChannelSplit_1.value = AtmosphereChannelSplit_1;

        _setting.AtmosphereDensityMultiplier_2.value = AtmosphereDensityMultiplier_2;
        _setting.AtmosphereInscatteringTint_2.value = AtmosphereInscatteringTint_2;
        _setting.AtmosphereUniformAbsorbsion_2.value = AtmosphereUniformAbsorbsion_2;
        _setting.AtmosphereAbsorbsionWeightPerChannel_2.value = AtmosphereAbsorbsionWeightPerChannel_2;
        _setting.AtmosphereChannelSplit_2.value = AtmosphereChannelSplit_2;


        _setting.AerosolsDensityMultiplier_1.value = AerosolsDensityMultiplier_1;
        _setting.AerosolsInscatteringTint_1.value = AerosolsInscatteringTint_1;
        _setting.AerosolsUniformAbsorbsion_1.value = AerosolsUniformAbsorbsion_1;
        _setting.AerosolsAnistropic_1.value = AerosolsAnistropic_1;

        _setting.AerosolsDensityMultiplier_2.value = AerosolsDensityMultiplier_2;
        _setting.AerosolsInscatteringTint_2.value = AerosolsInscatteringTint_2;
        _setting.AerosolsUniformAbsorbsion_2.value = AerosolsUniformAbsorbsion_2;
        _setting.AerosolsAnistropic_2.value = AerosolsAnistropic_2;

        _aberration.intensity.value = AberrationIntensity;
        _dof.focusDistance.value = Dof;

        StripeFXMat.SetFloat("_Alpha", StripeAlpha);


        FlowBufferHolder.Center = SphereMaskCenter;
        FlowBufferHolder.Size = SphereMaskRadius;

       MaskBufferHolder.Falloff = SphereMaskFalloff;
       MaskBufferHolder.Center = SphereMaskCenter;
       MaskBufferHolder.Radius = SphereMaskRadius;

        TileStripeFXComponent.Enabled = ToggleTileStripeFX;
        MaskBufferHolder.DownSamplingScale = StripeFXSampleSize;
        MaskBufferHolder.Seed = StripeFXSeed;
        MaskBufferHolder.Threshold = StripeFXStep;
    }
}
