using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class TerrainCulling : MonoBehaviour
{
    [SerializeField] private ComputeShader _terrainCullCompute;
    [SerializeField] private Mesh _targetMesh;
    [SerializeField] private Material _visualizationMaterial;
    [SerializeField] private Mesh _visualizationMesh;

    [SerializeField] private Material _terrainRenderMaterial;
    [SerializeField] private Material _terrainDepthOnly;

    private int[] _meshTriangleArray;

    private uint _vertCount;
    private uint _triCount;


    private int _elementCount;
    private int _groupCount;

    private bool _properlySetup = false;
    private ComputeBuffer _cb_vertexBuffer;
    private ComputeBuffer _cb_vertexVisualization_args;
    private ComputeBuffer _cb_finalRender_args;
    private ComputeBuffer _cb_finalindexBuffer;
    private ComputeBuffer _cb_triangleVote;
    private ComputeBuffer _cb_triangleScanBuffer;
    private ComputeBuffer _cb_triangleGroupScanInBuffer;
    private ComputeBuffer _cb_triangleGroupScanOutBuffer;
    private ComputeBuffer _cb_triangleCompactBuffer;
    private MaterialPropertyBlock _mpb_visual;
    private MaterialPropertyBlock _mpb_finalRender;

    public RenderTexture _depthPrePass;

    private void ReferenceCheck() 
    {
        if (_targetMesh == null)
            return;
        if (_visualizationMesh == null)
            return;
        if (_visualizationMaterial == null)
            return;
        if (_terrainCullCompute == null)
            return;
        if (_terrainRenderMaterial == null)
            return;
        if (_terrainDepthOnly == null)
            return;

        _properlySetup = true;
    }
    private void PrepareBuffer() 
    {
        if (!_properlySetup)
            return;

        _depthPrePass = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 24, RenderTextureFormat.RFloat);
        _depthPrePass.enableRandomWrite = true;
        _depthPrePass.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
        _triCount = (uint)_targetMesh.triangles.Length;

        _elementCount = Utility.CeilToNearestPowerOf2((int)_triCount);
        _groupCount = _elementCount / 512;
        _cb_vertexBuffer = new ComputeBuffer(_targetMesh.vertexCount, sizeof(float) * 11);
        _cb_vertexVisualization_args = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        _cb_finalindexBuffer = new ComputeBuffer((int)_triCount, sizeof(float));

        _cb_triangleVote = new ComputeBuffer(_elementCount, sizeof(uint));
        _cb_triangleScanBuffer = new ComputeBuffer(_elementCount, sizeof(int));
        _cb_triangleGroupScanInBuffer = new ComputeBuffer(_groupCount, sizeof(int));
        _cb_triangleGroupScanOutBuffer = new ComputeBuffer(_groupCount, sizeof(int));
        _cb_triangleCompactBuffer = new ComputeBuffer((int)_triCount, sizeof(int));
        uint[] _args_arry = new uint[]
        {
            _visualizationMesh.GetIndexCount(0),
            (uint) _targetMesh.vertexCount,
            _visualizationMesh.GetIndexStart(0),
            _visualizationMesh.GetBaseVertex(0),
            0
        };
        _cb_vertexVisualization_args.SetData(_args_arry);


        _cb_finalRender_args = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] _args = new uint[] {
            _triCount,
            1,
            0,
            0,
            0
        };
        _cb_finalRender_args.SetData(_args);

        _targetMesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
        GraphicsBuffer vb = _targetMesh.GetVertexBuffer(0);
        _terrainCullCompute.SetBuffer(0, "_TargetMeshRawVertexBuffer", vb);
        _terrainCullCompute.SetBuffer(2, "_TargetMeshRawVertexBuffer", vb);
        vb.Dispose();

        _targetMesh.indexBufferTarget |=  GraphicsBuffer.Target.Index | GraphicsBuffer.Target.Structured;
        GraphicsBuffer ib = _targetMesh.GetIndexBuffer();

        _terrainCullCompute.SetInt("_TriCount",(int)_triCount);
        _terrainCullCompute.SetInt("_Use16Bit",_targetMesh.indexFormat == IndexFormat.UInt16? 1 : 0);

        _terrainCullCompute.SetBuffer(1, "_TargetMeshIndexBuffer", ib);
        _terrainCullCompute.SetBuffer(1, "_IndexBuffer",_cb_finalindexBuffer);
        _terrainCullCompute.Dispatch(1, Mathf.CeilToInt(_triCount  / 128f ), 1, 1);

        ib.Dispose();

        _terrainCullCompute.SetBuffer(2, "_IndexBuffer", _cb_finalindexBuffer);
        _terrainCullCompute.SetBuffer(2, "_SpawnBuffer", _cb_vertexBuffer);

        _terrainCullCompute.SetBuffer(0, "_SpawnBuffer", _cb_vertexBuffer);
        _terrainCullCompute.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _terrainCullCompute.Dispatch(0, Mathf.CeilToInt(_targetMesh.vertexCount / 128f), 1, 1);

        _terrainCullCompute.SetBuffer(2, "_VoteBuffer", _cb_triangleVote);

        _terrainCullCompute.SetBuffer(3, "_VoteBuffer", _cb_triangleVote);
        _terrainCullCompute.SetBuffer(3, "_ScanBuffer", _cb_triangleScanBuffer);
        _terrainCullCompute.SetBuffer(3, "_GroupScanBufferIn", _cb_triangleGroupScanInBuffer);

        _terrainCullCompute.SetBuffer(4, "_GroupScanBufferIn", _cb_triangleGroupScanInBuffer);
        _terrainCullCompute.SetBuffer(4, "_GroupScanBufferOut", _cb_triangleGroupScanOutBuffer);

        _terrainCullCompute.SetBuffer(5, "_VoteBuffer", _cb_triangleVote);
        _terrainCullCompute.SetBuffer(5, "_ScanBuffer", _cb_triangleScanBuffer);
        _terrainCullCompute.SetBuffer(5, "_GroupScanBufferOut", _cb_triangleGroupScanOutBuffer);
        _terrainCullCompute.SetBuffer(5, "_CompactIndexBuffer", _cb_triangleCompactBuffer);
        _terrainCullCompute.SetBuffer(5, "_IndexBuffer", _cb_finalindexBuffer);
        _terrainCullCompute.SetBuffer(5, "_ArgsBuffer", _cb_finalRender_args);



        _terrainCullCompute.SetBuffer(6, "_ArgsBuffer", _cb_finalRender_args);




        _mpb_visual = new MaterialPropertyBlock();
        _mpb_visual.SetBuffer("_SpawnBuffer", _cb_vertexBuffer);

        _mpb_finalRender = new MaterialPropertyBlock();
        _mpb_finalRender.SetBuffer("_IndexBuffer", _cb_triangleCompactBuffer);
        _mpb_finalRender.SetBuffer("_SpawnBuffer", _cb_vertexBuffer);
    }
    private void DrawContent()
    {
        if (!_properlySetup)
            return;
        _terrainCullCompute.SetVector("_CameraPos", Camera.main.transform.position);
        _terrainCullCompute.SetTexture(2, "_HiZTexture", _depthPrePass);

        _terrainCullCompute.SetFloat("_Camera_Near", Camera.main.nearClipPlane);
        _terrainCullCompute.SetFloat("_Camera_Far", Camera.main.farClipPlane);

        _terrainCullCompute.SetMatrix("_Camera_V", Camera.main.worldToCameraMatrix);
        _terrainCullCompute.SetMatrix("_Camera_P", Camera.main.projectionMatrix);

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Vector4[] planeVectors = new Vector4[6];
        for (int i = 0; i < 6; i++)
        {
            Plane p = frustumPlanes[i];
            planeVectors[i] = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        }

        _terrainCullCompute.SetVectorArray("_FrustumPlanes", planeVectors);

        CommandBuffer cb = new CommandBuffer();
        cb.name = "Terrain GPU Cull Command";
        cb.SetRenderTarget(_depthPrePass);
        cb.ClearRenderTarget(true, true, Color.clear);
        cb.SetViewMatrix(Camera.main.worldToCameraMatrix);
        cb.SetProjectionMatrix(Camera.main.projectionMatrix);
        cb.DrawMesh(_targetMesh, transform.localToWorldMatrix, _terrainDepthOnly);
 
        cb.DispatchCompute(_terrainCullCompute, 6, 1, 1, 1);
        cb.DispatchCompute(_terrainCullCompute, 2, Mathf.CeilToInt((_triCount / 3) / 128f), 1, 1);
        cb.DispatchCompute(_terrainCullCompute, 3, _groupCount, 1, 1);
        cb.DispatchCompute(_terrainCullCompute, 4, 1, 1, 1);
        cb.DispatchCompute(_terrainCullCompute, 5, Mathf.CeilToInt((_triCount) / 512), 1, 1);

        Graphics.ExecuteCommandBuffer(cb);
        cb.Release();

        Graphics.DrawMeshInstancedIndirect(_visualizationMesh, 0, _visualizationMaterial, new Bounds(Vector3.zero, Vector3.one * 10000), _cb_vertexVisualization_args, 0, _mpb_visual);
        Graphics.DrawProceduralIndirect(_terrainRenderMaterial, new Bounds(Vector3.zero, Vector3.one * 10000),MeshTopology.Triangles, _cb_finalRender_args, 0, null,_mpb_finalRender);
    }

    private void DisposeResources() 
    {
        _cb_vertexBuffer.Dispose();
        _cb_vertexBuffer = null;
        _cb_finalRender_args.Dispose();
        _cb_finalRender_args = null;
        _cb_vertexVisualization_args?.Dispose();
        _cb_vertexVisualization_args = null;
        _cb_triangleVote?.Dispose();
        _cb_triangleVote = null;
        _cb_triangleScanBuffer?.Dispose(); 
        _cb_triangleScanBuffer = null;
        _cb_triangleGroupScanInBuffer?.Dispose();
        _cb_triangleGroupScanInBuffer = null;
        _cb_triangleGroupScanOutBuffer?.Dispose();
        _cb_triangleGroupScanOutBuffer = null;
        _cb_triangleCompactBuffer?.Dispose();  
        _cb_triangleCompactBuffer = null;
        _cb_finalindexBuffer?.Dispose();
        _cb_finalindexBuffer = null;

        _depthPrePass.Release();
        _depthPrePass = null;
    }

    private void OnEnable()
    {
         ReferenceCheck();
         PrepareBuffer();
    }
    private void OnDisable()
    {
        DisposeResources();
    }
    private void LateUpdate()
    {
        DrawContent();
    }

}
