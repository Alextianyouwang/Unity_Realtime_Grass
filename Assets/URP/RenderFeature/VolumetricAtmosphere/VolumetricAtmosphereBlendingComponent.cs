using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Volumetric Atmosphere With Blending", typeof(UniversalRenderPipeline))]
public class VolumetricAtmosphereBlendingComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter Enabled = new BoolParameter(false, BoolParameter.DisplayType.Checkbox, false);
    [Header("Configuration")]
    public FloatParameter EarthRadius = new FloatParameter(5000f, false);
    public ClampedIntParameter OpticalDepthSamples = new ClampedIntParameter(50, 1, 100, false);
    public ClampedIntParameter InscatteringSamples = new ClampedIntParameter(30, 1, 100, false);
    public Vector3Parameter BlendCenter = new Vector3Parameter(Vector3.zero, false);
    public FloatParameter BlendRaidus = new FloatParameter(100f, false);
    public FloatParameter BlendFalloff = new FloatParameter(10f, false);
    public BoolParameter EnableDistorsion = new BoolParameter(false, BoolParameter.DisplayType.Checkbox, false);
    public BoolParameter EnableMask = new BoolParameter(false, BoolParameter.DisplayType.Checkbox, false);
    [Header("Rayleigh Scattering")]
    public BoolParameter EnableRayleighScattering = new BoolParameter(true, BoolParameter.DisplayType.Checkbox, false);
    public FloatParameter AtmosphereHeight = new FloatParameter(100f, false);
    public ClampedFloatParameter AtmosphereDensityFalloff = new ClampedFloatParameter(1f, 1f, 10f, false);

    public ClampedFloatParameter AtmosphereDensityMultiplier_1 = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AtmosphereInscatteringTint_1 = new ColorParameter(Color.white, true, false, false);
    public ClampedFloatParameter AtmosphereUniformAbsorbsion_1 = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ColorParameter AtmosphereAbsorbsionWeightPerChannel_1 = new ColorParameter(new Color(0.01f, 0.03f, 0.08f), false, false, false);
    public ClampedFloatParameter AtmosphereChannelSplit_1 = new ClampedFloatParameter(1, 0f, 1f, false);

    public ClampedFloatParameter AtmosphereDensityMultiplier_2 = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AtmosphereInscatteringTint_2 = new ColorParameter(Color.white, true, false, false);
    public ClampedFloatParameter AtmosphereUniformAbsorbsion_2 = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ColorParameter AtmosphereAbsorbsionWeightPerChannel_2 = new ColorParameter(new Color(0.01f, 0.03f, 0.08f), false, false, false);
    public ClampedFloatParameter AtmosphereChannelSplit_2 = new ClampedFloatParameter(1, 0f, 1f, false);

    [Header("Mie Scattering")]
    public BoolParameter EnableMieScattering = new BoolParameter(true, BoolParameter.DisplayType.Checkbox, false);
    public FloatParameter AerosolsHeight = new FloatParameter(20f, false);
    public ClampedFloatParameter AerosolsDensityFalloff = new ClampedFloatParameter(1f, 1f, 10f, false);

    public ClampedFloatParameter AerosolsDensityMultiplier_1 = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AerosolsInscatteringTint_1 = new ColorParameter(Color.white, true, false, false);
    public ClampedFloatParameter AerosolsUniformAbsorbsion_1 = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ClampedFloatParameter AerosolsAnistropic_1 = new ClampedFloatParameter(0.7f, 0f, 1f, false);

    public ClampedFloatParameter AerosolsDensityMultiplier_2 = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AerosolsInscatteringTint_2 = new ColorParameter(Color.white, true, false, false);
    public ClampedFloatParameter AerosolsUniformAbsorbsion_2 = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ClampedFloatParameter AerosolsAnistropic_2 = new ClampedFloatParameter(0.7f, 0f, 1f, false);

    [Header("Debug")]
    public BoolParameter VolumePassOnly = new BoolParameter(false, BoolParameter.DisplayType.Checkbox, false);


    public bool IsTileCompatible()
    {
        return true;
    }

    public bool IsActive()
    {
        return Enabled.value;
    }

    public VolumetricAtmosphereBlendingComponent() : base()
    {
        displayName = "Volumetric Atmosphere With Blending";
    }


}
