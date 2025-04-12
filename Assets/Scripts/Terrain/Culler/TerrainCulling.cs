using UnityEngine;
[ExecuteInEditMode]
public class TerrainCulling : MonoBehaviour
{
    [SerializeField] private ComputeShader _terrainCullCompute;
    [SerializeField] private Mesh _targetMesh;
    [SerializeField] private Material _visualizationMaterial;
    [SerializeField] private Mesh _visualizationMesh;

    private MeshVertex[] _meshVertexArray;
    private int[] _meshTriangleArray;

    private uint _vertCount;

    private bool _properlySetup = false;
    private ComputeBuffer _cb_vertexVisualization;
    private ComputeBuffer _cb_vertexVisualization_args;
    private MaterialPropertyBlock _mpb_visual;


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
        _cb_vertexVisualization = new ComputeBuffer(_targetMesh.vertexCount, sizeof(float) * 8);
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

        _targetMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        GraphicsBuffer vb = _targetMesh.GetVertexBuffer(0);
        _terrainCullCompute.SetBuffer(0, "_TargetMeshRawBuffer", vb);
        vb.Dispose();
        _terrainCullCompute.SetBuffer(0, "_SpawnBuffer", _cb_vertexVisualization);
        _terrainCullCompute.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _terrainCullCompute.Dispatch(0, Mathf.CeilToInt(_targetMesh.vertexCount / 128f), 1, 1);

        _mpb_visual = new MaterialPropertyBlock();
        _mpb_visual.SetBuffer("_SpawnBuffer", _cb_vertexVisualization);
    }
    private void DrawContent()
    {
        if (!_properlySetup)
            return;

 
       Graphics.DrawMeshInstancedIndirect(_visualizationMesh, 0, _visualizationMaterial, new Bounds(Vector3.zero, Vector3.one * 10000), _cb_vertexVisualization_args, 0, _mpb_visual);
    }

    private void DisposeResources() 
    {
        _cb_vertexVisualization.Dispose();
        _cb_vertexVisualization = null;
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
