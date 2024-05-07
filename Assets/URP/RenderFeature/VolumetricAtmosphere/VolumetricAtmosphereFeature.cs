using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class VolumetricAtmosphereFeature : ScriptableRendererFeature
{
    public bool showInSceneView = true;
    public RenderPassEvent _event = RenderPassEvent.AfterRenderingTransparents;

    private VolumetricAtmospherePass _volumePass;
    private Material _blitMat;
    public override void Create()
    {
        if (_blitMat == null) 
            _blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/S_Atmosphere"));

        _volumePass = new VolumetricAtmospherePass( name, _blitMat);
        _volumePass.renderPassEvent = _event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return; 
        if (!showInSceneView && cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(_volumePass);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return;
        if (!showInSceneView && cameraType == CameraType.SceneView) return;
        _volumePass.ConfigureInput(ScriptableRenderPassInput.Color);
        _volumePass.SetTarget(renderer.cameraColorTargetHandle);
    }
    protected override void Dispose(bool disposing)
    {
        _volumePass.Dispose();
        if (!Application.isPlaying)
            CoreUtils.Destroy(_blitMat);
    }
}