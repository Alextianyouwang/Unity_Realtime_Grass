
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class CommandBufferTest : MonoBehaviour
{
    public RenderTexture TargetRT;
    private Camera _camera;
    private CommandBuffer _drawBuffer;
    public GameObject TestObj;
    private Material _depthMat;
    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _depthMat = new Material(Shader.Find("Utility/S_GetDepth"));
        _drawBuffer = new CommandBuffer();
    }

    private void OnDisable()
    {
        _drawBuffer.Dispose();
    }
    void LateUpdate()
    {
        if (TargetRT == null)
            return;
        if (TestObj == null)
            return;


        _drawBuffer.SetViewMatrix(_camera.worldToCameraMatrix);
        _drawBuffer.SetProjectionMatrix(_camera.projectionMatrix);
        _drawBuffer.SetRenderTarget(TargetRT);
        _drawBuffer.ClearRenderTarget(true, true, Color.clear);
       
        _drawBuffer.DrawRenderer(TestObj.GetComponent<MeshRenderer>(),TestObj.GetComponent<MeshRenderer>().sharedMaterial,0,3);
 

        Graphics.ExecuteCommandBuffer( _drawBuffer );
        _drawBuffer.Clear();
    }
}
