using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[DefaultExecutionOrder (-101)]
public class ParameterHolder : MonoBehaviour
{

    public Vector3 SphereMaskCenter;
    public float SphereMaskRadius;
    public float SphereMaskFalloff;

    public VolumeProfile VolumeObject;
    private VolumetricAtmosphereBlendingComponent _setting;

    public WindSimTest FlowBufferHolder;
    public MaskBufferManager MaskBufferHolder;
    private void OnEnable()
    {
        VolumeObject.TryGet(out _setting);

    }
    void LateUpdate()
    {
        _setting.BlendCenter.value = SphereMaskCenter;
        _setting.BlendRaidus.value = SphereMaskRadius;
        _setting.BlendFalloff.value = SphereMaskFalloff;

        FlowBufferHolder.Center = SphereMaskCenter;
        FlowBufferHolder.Size = SphereMaskRadius;

       MaskBufferHolder.Falloff = SphereMaskFalloff;
       MaskBufferHolder.Center = SphereMaskCenter;
       MaskBufferHolder.Radius = SphereMaskRadius;
    }
}
