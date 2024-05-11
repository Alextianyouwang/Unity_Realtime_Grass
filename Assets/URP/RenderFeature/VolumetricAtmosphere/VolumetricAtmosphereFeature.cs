using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class VolumetricAtmosphereFeature : ScriptableRendererFeature
{
    public bool showInSceneView = true;
    public RenderPassEvent _event = RenderPassEvent.AfterRenderingTransparents;

    private VolumetricAtmospherePass _volumePass;
    private Material _blitMat;

    private ComputeShader _baker;
    private RenderTexture _opticalDepthTex;
    public enum PrebakedTextureQuality {Low128,Medium256,High512,Ultra1024,Realtime}
    public PrebakedTextureQuality PrebakedTextureQualitySetting = PrebakedTextureQuality.High512;
    private int resolusion = 512;
    public override void Create()
    {
        if (_blitMat == null) 
            _blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/S_Atmosphere"));
        _baker = (ComputeShader)Resources.Load("CS/CS_VA_LookuptableBaker");
        CreateRenderRT();
        _volumePass = new VolumetricAtmospherePass(name);
        _volumePass.renderPassEvent = _event;
    }
  
    private void CreateRenderRT() 
    {
        switch (PrebakedTextureQualitySetting)
        {
            case PrebakedTextureQuality.Low128:
                resolusion = 128;
                break;
            case PrebakedTextureQuality.Medium256:
                resolusion = 256;
                break;
            case PrebakedTextureQuality.High512:
                resolusion = 512;
                break;
            case PrebakedTextureQuality.Ultra1024:
                resolusion = 1024;
                break;
            case PrebakedTextureQuality.Realtime:
                resolusion = 1;
                break;
            default:
                resolusion = 512;
                break;
        }
        _opticalDepthTex = new RenderTexture(resolusion, resolusion, 0, RenderTextureFormat.ARGB64, 0);
        _opticalDepthTex.filterMode = FilterMode.Point;
        _opticalDepthTex.enableRandomWrite = true;
        _opticalDepthTex.format = RenderTextureFormat.ARGBFloat;

    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!ReadyToEnqueue(renderingData)) return;
        renderer.EnqueuePass(_volumePass);

      
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (!ReadyToEnqueue(renderingData)) return;
        _volumePass.SetData(renderer.cameraColorTargetHandle,_baker, _opticalDepthTex,_blitMat,PrebakedTextureQualitySetting == PrebakedTextureQuality.Realtime);
    }
    bool ReadyToEnqueue(RenderingData renderingData) 
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return false;
        if (!showInSceneView && cameraType == CameraType.SceneView) return false;
        if (!_baker) return false;
        if (!_blitMat) return false;
        return true;
    }
    protected override void Dispose(bool disposing)
    {
        _volumePass.Dispose();
        if (!Application.isPlaying) 
        {
            CoreUtils.Destroy(_blitMat);
            if (_opticalDepthTex != null)
                _opticalDepthTex.Release();
        }
    }
}