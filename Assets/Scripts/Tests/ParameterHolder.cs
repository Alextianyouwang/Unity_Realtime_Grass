using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ParameterHolder : MonoBehaviour
{

    public Vector3 SphereMaskCenter;
    public float SphereMaskRadius;
    public float SphereMaskFalloff;

    public VolumeProfile VolumeObject;
    private VolumetricAtmosphereBlendingComponent _setting;

    public WindSimTest WindSimTest;
    private void OnEnable()
    {
        VolumeObject.TryGet(out _setting);

    }
    void Update()
    {
        _setting.BlendCenter.value = SphereMaskCenter;
        _setting.BlendRaidus.value = SphereMaskRadius;
        _setting.BlendFalloff.value = SphereMaskFalloff;

        WindSimTest.Center = SphereMaskCenter;
        WindSimTest.Size = SphereMaskRadius;
    }
}
