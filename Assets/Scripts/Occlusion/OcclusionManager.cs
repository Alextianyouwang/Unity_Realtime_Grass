using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class OcclusionManager : MonoBehaviour
{
    public Camera RenderCam;
    public Renderer[] Occluders;

    private RenderTexture _zTex;
    private Material _zMat;
    private Mesh _occluder;

    public void LateUpdate()
    {
        DrawOcclusionTexture();
    }
    private void OnEnable()
    {
        InitializeOcclusionSetup();
        TileGrandCluster.OnRequestOcclusionTexture += GetOcclusionTexture;
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(_zTex);
        TileGrandCluster.OnRequestOcclusionTexture -= GetOcclusionTexture;
    }

    public RenderTexture GetOcclusionTexture() 
    {
        return _zTex;
    }
    void InitializeOcclusionSetup()
    {
        if (RenderCam == null)
            return;
        if (Occluders == null)
            return;

        _zTex = RenderTexture.GetTemporary(RenderCam.pixelWidth, RenderCam.pixelHeight, 32, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        _zTex.Create();
        _zMat = new Material(Shader.Find("Utility/S_DepthOnly"));
        _occluder = Utility.CombineMeshes(Occluders.Select(x => x.gameObject).ToArray());
    }
    public void DrawOcclusionTexture()
    {
        if (RenderCam == null)
            return;
        if (_occluder == null)
            return;

        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "DrawOccluderDepth";
        cmd.SetViewMatrix(RenderCam.worldToCameraMatrix);
        cmd.SetProjectionMatrix(RenderCam.projectionMatrix);
        cmd.SetRenderTarget(_zTex);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(_occluder, Matrix4x4.identity, _zMat);
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}
