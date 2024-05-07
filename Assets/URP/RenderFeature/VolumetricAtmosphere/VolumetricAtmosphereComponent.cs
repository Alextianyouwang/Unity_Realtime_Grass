using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenuForRenderPipeline("Custom/Volumetric Atmosphere", typeof(UniversalRenderPipeline))]
public class VolumetricAtmosphereComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter Enabled = new BoolParameter(false, BoolParameter.DisplayType.Checkbox, true);
    [Header("Configuration")]
    public FloatParameter EarthRadius = new FloatParameter(5000f, false);
    public ClampedIntParameter OpticalDepthSamples = new ClampedIntParameter(10, 1, 30, false);
    public ClampedIntParameter InscatteringSamples = new ClampedIntParameter(30, 1, 30, false);

    [Header("Rayleigh Scattering")]
    public FloatParameter AtmosphereHeight = new FloatParameter(100f, false);
    public ClampedFloatParameter AtmosphereDensityFalloff = new ClampedFloatParameter(1f,1f,10f, false);
    public ClampedFloatParameter AtmosphereDensityMultiplier = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AtmosphereInscatteringTint = new ColorParameter(Color.white,true,false, true);
    public ClampedFloatParameter AtmosphereUniformAbsorbsion = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ColorParameter AtmosphereAbsorbsionWeightPerChannel = new ColorParameter(new Color(0.01f, 0.03f, 0.08f), false,false,true);
    public ClampedFloatParameter AtmosphereChannelSplit = new ClampedFloatParameter(1, 0f, 1f, false);



    [Header("Mie Scattering")]
    public FloatParameter AerosolsHeight = new FloatParameter(20f, false);
    public ClampedFloatParameter AerosolsDensityFalloff = new ClampedFloatParameter(1f, 1f, 10f, false);
    public ClampedFloatParameter AerosolsDensityMultiplier = new ClampedFloatParameter(1f, 0f, 3f, false);
    public ColorParameter AerosolsInscatteringTint = new ColorParameter(Color.white, true, false, true);
    public ClampedFloatParameter AerosolsUniformAbsorbsion = new ClampedFloatParameter(0.1f, 0f, 1f, false);
    public ClampedFloatParameter AerosolsAnistropic = new ClampedFloatParameter(0.7f, 0f, 1f, false);


    public bool IsTileCompatible()
    {
        return true;
    }

    public bool IsActive()
    {
        return Enabled.value;
    }
}
