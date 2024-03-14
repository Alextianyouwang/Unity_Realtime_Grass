
using UnityEngine;
[ExecuteInEditMode]
public class GrassField : MonoBehaviour
{
    private ComputeShader _spawnShader;
    private ComputeBuffer _sourceVerticesBuffer_spawn;
    private ComputeBuffer _sourceTrianglesBuffer_spawn;
    private ComputeBuffer _spawnBuffer_spawn;
    private ComputeBuffer _argsBuffer;
    private SourceVertex[] _sourceVertices;
    private SpawnData[] _spawnData;
    private int[] _sourceTriangles;
    private int _quadCount;

    private ComputeShader _cullShader;
    private ComputeBuffer _voteBuffer_cull;
    private ComputeBuffer _scanBuffer_cull;
    private ComputeBuffer _groupScanBufferIn_cull;
    private ComputeBuffer _groupScanBufferOut_cull;
    private ComputeBuffer _compactBuffer_cull;
    private ComputeBuffer _argsBuffer_static;
    private uint[] _args;
    private int _elementCount;
    private int _groupCount;


    private Mesh _mesh;

    public Mesh GrassMesh;
    public Material GrassMaterial;
    public bool UseCulling = true;

    [Range(1, 10)]
    public int Subdivision = 2;

    struct SourceVertex
    {
        public Vector3 positionOS;
        public Vector2 uv;
        public Vector3 normalOS;
    };

    struct SpawnData
    {
        public Vector3 positionWS;
    }
    private void OnEnable()
    {

        _spawnShader = (ComputeShader)Resources.Load("CS_SpawnOnMesh");
        _cullShader = (ComputeShader)Resources.Load("CS_GrassCulling");
        TryGetMesh();
        SpawnPoint();
    }
    private void OnDisable()
    {
        _argsBuffer?.Release();

        _sourceTrianglesBuffer_spawn?.Release();
        _sourceVerticesBuffer_spawn?.Release();
        _spawnBuffer_spawn?.Release();

        _voteBuffer_cull?.Release();
        _scanBuffer_cull?.Release();
        _groupScanBufferIn_cull?.Release();
        _groupScanBufferOut_cull?.Release();
        _compactBuffer_cull?.Release();
        _argsBuffer_static?.Release();
    }


    private void TryGetMesh()
    {
        PlaneManager p = GetComponent<PlaneManager>();
        if (p != null)
            _mesh = p.PlaneMesh;
    }
    private void SpawnPoint()
    {
        if (_mesh == null)
        {
            print("Terrain mesh reference is null, try to re-enable terrain manager");
            return;
        }
        GetMeshData(_mesh);
        InitializeShader_spawn();
        InitializeShader_cull();
    }
    private void GetMeshData(Mesh mesh)
    {

        _sourceVertices = new SourceVertex[mesh.vertices.Length];
        for (int i = 0; i < _sourceVertices.Length; i++)
        {
            _sourceVertices[i] = new SourceVertex
            {
                positionOS = mesh.vertices[i],
                uv = mesh.uv[i],
                normalOS = mesh.normals[i]
            };
        }
        _sourceTriangles = mesh.triangles;
        _quadCount = _sourceTriangles.Length / 6;

        _spawnData = new SpawnData[_quadCount * Subdivision * Subdivision];

    }
    private void InitializeShader_spawn()
    {
        _sourceTrianglesBuffer_spawn = new ComputeBuffer(_sourceTriangles.Length, sizeof(int));
        _sourceTrianglesBuffer_spawn.SetData(_sourceTriangles);
        _sourceVerticesBuffer_spawn = new ComputeBuffer(_sourceVertices.Length, sizeof(float) * 8);
        _sourceVerticesBuffer_spawn.SetData(_sourceVertices);
        _spawnBuffer_spawn = new ComputeBuffer(_spawnData.Length, sizeof(float) * 3, ComputeBufferType.Append);
        _spawnBuffer_spawn.SetCounterValue(0);

        _spawnShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _spawnShader.SetInt("_NumQuad", _quadCount);
        _spawnShader.SetInt("_Subdivisions", Subdivision);

        _spawnShader.SetBuffer(0, "_SourceVerticesBuffer", _sourceVerticesBuffer_spawn);
        _spawnShader.SetBuffer(0, "_SourceTrianglesBuffer", _sourceTrianglesBuffer_spawn);
        _spawnShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer_spawn);
        if (GrassMaterial)
            GrassMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer_spawn);

        _spawnShader.Dispatch(0, Mathf.CeilToInt(_quadCount / 128f), 1, 1);
    }

    private void InitializeShader_cull() 
    {
        _elementCount =Utility.CeilToNearestPowerOf2(_spawnData.Length);
        _groupCount = Utility.CeilToNearestPowerOf2(_elementCount / 128);
        if (GrassMesh)
        {
            _args = new uint[] {
            GrassMesh.GetIndexCount(0),
            (uint)_spawnData.Length,
            GrassMesh.GetIndexStart(0),
            GrassMesh.GetBaseVertex(0),
            0
        };
            _argsBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(_args);
            _argsBuffer_static = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
            _argsBuffer_static.SetData(_args);
        }

        /* Procedural */
        _voteBuffer_cull = new ComputeBuffer(_spawnData.Length, sizeof(uint));
        /* Procedural */
        _scanBuffer_cull = new ComputeBuffer(_elementCount, sizeof(uint));
        /* Procedural */
        _groupScanBufferIn_cull = new ComputeBuffer(_groupCount, sizeof(uint));
        /* Procedural */
        _groupScanBufferOut_cull = new ComputeBuffer(_groupCount, sizeof(uint));
        /* Procedural */
        _compactBuffer_cull = new ComputeBuffer(_spawnData.Length, sizeof(float) * 3);


        _cullShader.SetInt("_InstanceCount", _spawnData.Length);

        _cullShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer_spawn);
        _cullShader.SetBuffer(0, "_VoteBuffer", _voteBuffer_cull);

        _cullShader.SetBuffer(1, "_ScanBuffer", _scanBuffer_cull);
        _cullShader.SetBuffer(1, "_GroupScanBufferIn", _groupScanBufferIn_cull);
        _cullShader.SetBuffer(1, "_VoteBuffer", _voteBuffer_cull);

        _cullShader.SetBuffer(2, "_GroupScanBufferIn", _groupScanBufferIn_cull);
        _cullShader.SetBuffer(2, "_GroupScanBufferOut", _groupScanBufferOut_cull);

        _cullShader.SetBuffer(3, "_ScanBuffer", _scanBuffer_cull);
        _cullShader.SetBuffer(3, "_GroupScanBufferOut", _groupScanBufferOut_cull);
        _cullShader.SetBuffer(3, "_VoteBuffer", _voteBuffer_cull);
        _cullShader.SetBuffer(3, "_CompactBuffer", _compactBuffer_cull);
        _cullShader.SetBuffer(3, "_SpawnBuffer", _spawnBuffer_spawn);
        _cullShader.SetBuffer(3, "_ArgsBuffer", _argsBuffer);

        _cullShader.SetBuffer(4, "_ArgsBuffer", _argsBuffer);
        print($"Budget Useage: {_spawnData.Length / 262144f * 100}%");

    }

    private void Update()
    {
        if (
            GrassMesh == null
            || GrassMaterial == null
            || _argsBuffer == null
            || _cullShader == null
            )
            return;

        if (UseCulling) 
        {
            _cullShader.SetMatrix("_Camera_V", Camera.main.transform.worldToLocalMatrix);
            _cullShader.SetMatrix("_Camera_P", Camera.main.projectionMatrix);
            _cullShader.Dispatch(4, 1, 1, 1);
            _cullShader.Dispatch(0, Mathf.CeilToInt(_spawnData.Length / 128f), 1, 1);
            _cullShader.Dispatch(1, _groupCount, 1, 1);
            _cullShader.Dispatch(2, 1, 1, 1);
            _cullShader.Dispatch(3, Mathf.CeilToInt(_spawnData.Length / 128f), 1, 1);
        }
       

        GrassMaterial.SetBuffer("_SpawnBuffer", UseCulling? _compactBuffer_cull :_spawnBuffer_spawn);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(GrassMesh, 0, GrassMaterial, bounds, UseCulling? _argsBuffer : _argsBuffer_static,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
    }
}
