using UnityEngine;
[ExecuteInEditMode]
public class TerrainCulling : MonoBehaviour
{
    [SerializeField] private ComputeShader _terrainCullCompute;
    [SerializeField] private Mesh _targetMesh;
    [SerializeField] private Material _visualizationMaterial;
    [SerializeField] private Mesh _visualizationMesh;

    [SerializeField] private Material _terrainRenderMaterial;

    private MeshVertex[] _meshVertexArray;
    private int[] _meshTriangleArray;

    private uint _vertCount;

    private bool _properlySetup = false;
    private ComputeBuffer _cb_vertexBuffer;
    private ComputeBuffer _cb_vertexVisualization_args;
    private ComputeBuffer _cb_finalRender_args;
    private GraphicsBuffer _gb_indexBuffer;
    private MaterialPropertyBlock _mpb_visual;
    private MaterialPropertyBlock _mpb_finalRender;


    public struct MeshVertex
    {
        public Vector3 PosOS;
        public Vector3 NormalOS;
        public Vector2 UV;
    }

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

        _properlySetup = true;
    }
    private void PrepareBuffer() 
    {
        if (!_properlySetup)
            return;
        _cb_vertexBuffer = new ComputeBuffer(_targetMesh.vertexCount, sizeof(float) * 8);
        _cb_vertexVisualization_args = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] _args_arry = new uint[]
        {
            _visualizationMesh.GetIndexCount(0),
            (uint) _targetMesh.vertexCount,
            _visualizationMesh.GetIndexStart(0),
            _visualizationMesh.GetBaseVertex(0),
            0
        };
        _cb_vertexVisualization_args.SetData(_args_arry);

        _cb_finalRender_args = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] _args = new uint[] {
            (uint)_targetMesh.triangles.Length,
            1,
            0,
            0,
        };
        _cb_finalRender_args.SetData(_args);

        _targetMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        _targetMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        GraphicsBuffer vb = _targetMesh.GetVertexBuffer(0);
        _gb_indexBuffer = _targetMesh.GetIndexBuffer();
        _terrainCullCompute.SetBuffer(0, "_TargetMeshRawVertexBuffer", vb);
        vb.Dispose();
        _terrainCullCompute.SetBuffer(0, "_SpawnBuffer", _cb_vertexBuffer);
        _terrainCullCompute.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _terrainCullCompute.Dispatch(0, Mathf.CeilToInt(_targetMesh.vertexCount / 128f), 1, 1);

        _mpb_visual = new MaterialPropertyBlock();
        _mpb_visual.SetBuffer("_SpawnBuffer", _cb_vertexBuffer);

        _mpb_finalRender = new MaterialPropertyBlock();
        _mpb_finalRender.SetBuffer("_IndexBuffer", _gb_indexBuffer);
        _mpb_finalRender.SetBuffer("_SpawnBuffer", _cb_vertexBuffer);
    }
    private void DrawContent()
    {
        if (!_properlySetup)
            return;

 
        Graphics.DrawMeshInstancedIndirect(_visualizationMesh, 0, _visualizationMaterial, new Bounds(Vector3.zero, Vector3.one * 10000), _cb_vertexVisualization_args, 0, _mpb_visual);
        Graphics.DrawProceduralIndirect(_terrainRenderMaterial, new Bounds(Vector3.zero, Vector3.one * 10000),MeshTopology.Triangles, _cb_finalRender_args, 0, null,_mpb_finalRender);
    }

    private void DisposeResources() 
    {
        _cb_vertexBuffer.Dispose();
        _cb_vertexBuffer = null;
        _gb_indexBuffer.Dispose();
        _gb_indexBuffer = null;
        _cb_finalRender_args.Dispose();
        _cb_finalRender_args = null;
        _cb_vertexVisualization_args?.Dispose();
        _cb_vertexVisualization_args = null;
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
