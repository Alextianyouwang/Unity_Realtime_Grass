using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-99)]
public class SpawnOnMeshManager : MonoBehaviour
{
    private ComputeShader _spawnShader;

    private ComputeBuffer _sourceVerticesBuffer;
    private ComputeBuffer _sourceTrianglesBuffer;
    private ComputeBuffer _argsBuffer;
    public ComputeBuffer SpawnBuffer;

    private SourceVertex[] _sourceVertices;
    private SpawnData[] _spawnPositions;
    private int[] _sourceTriangles;
    private int _quadCount;

    private Mesh _mesh;

    public Mesh TestMesh;
    public Material RenderingMaterial;

    [Range (1,10)]
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
        TryGetMesh();
        SpawnPoint();
    }
    private void OnDisable()
    {

        _sourceTrianglesBuffer?.Release();
        _sourceVerticesBuffer?.Release();
        SpawnBuffer?.Release();
        _argsBuffer?.Release();
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
        InitializeShader();
    }

    private void GetMeshData(Mesh mesh) 
    {
       
        _sourceVertices = new SourceVertex[mesh.vertices.Length];
        for (int i = 0; i < _sourceVertices.Length; i++) 
        {
            _sourceVertices[i] = new SourceVertex {
                positionOS = mesh.vertices[i],
                uv = mesh.uv[i],
                normalOS = mesh.normals[i]
            };
        }
        _sourceTriangles = mesh.triangles;
        _quadCount = _sourceTriangles.Length / 6;

        _spawnPositions = new SpawnData[_quadCount * Subdivision * Subdivision];

    }

    private void InitializeShader() 
    {
        _sourceTrianglesBuffer = new ComputeBuffer(_sourceTriangles.Length, sizeof(int));
        _sourceTrianglesBuffer.SetData(_sourceTriangles);
        _sourceVerticesBuffer = new ComputeBuffer(_sourceVertices.Length, sizeof(float) * 8);
        _sourceVerticesBuffer.SetData(_sourceVertices);
        SpawnBuffer = new ComputeBuffer(_spawnPositions.Length, sizeof(float) * 3, ComputeBufferType.Append);
        SpawnBuffer.SetCounterValue(0);

        if (TestMesh) 
        {
            uint[] args = new uint[] {
            TestMesh.GetIndexCount(0),
            (uint)_spawnPositions.Length,
            TestMesh.GetIndexStart(0),
            TestMesh.GetBaseVertex(0),
            0
        };
            _argsBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);
        }
     
 

        _spawnShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _spawnShader.SetInt("_NumTriangles", _quadCount);
        _spawnShader.SetInt("_Subdivisions", Subdivision);

        _spawnShader.SetBuffer(0, "_SourceVerticesBuffer", _sourceVerticesBuffer);
        _spawnShader.SetBuffer(0, "_SourceTrianglesBuffer", _sourceTrianglesBuffer);
        _spawnShader.SetBuffer(0, "_SpawnBuffer", SpawnBuffer);
        if (RenderingMaterial)
            RenderingMaterial.SetBuffer("_SpawnBuffer", SpawnBuffer);

        _spawnShader.Dispatch(0, Mathf.CeilToInt(_quadCount / 128f), 1, 1);
        SpawnBuffer.GetData(_spawnPositions);
    }

    private void Update()
    {
        if (
            TestMesh == null
            || RenderingMaterial == null
            || _argsBuffer == null
            )
            return;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(TestMesh, 0, RenderingMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
    }

}
