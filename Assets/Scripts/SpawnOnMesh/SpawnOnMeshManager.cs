using UnityEngine;
[ExecuteInEditMode]
public class SpawnOnMeshManager : MonoBehaviour
{
    private ComputeShader _spawnShader;

    private ComputeBuffer _sourceVerticesBuffer;
    private ComputeBuffer _sourceTrianglesBuffer;
    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _argsBuffer;

    private SourceVertex[] _sourceVertices;
    private SpawnData[] _spawnPositions;
    private int[] _sourceTriangles;
    private int _quadCount;

    private Mesh _mesh;

    public Mesh GrassBlade;
    public Material GrassMaterial;

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
        public float radius;
    }

    private void OnEnable()
    {
        PlaneManager.OnShareMesh += GetMesh;
        _spawnShader = (ComputeShader)Resources.Load("CS_SpawnOnMesh");
        SpawnPoint();
    }
    private void OnDisable()
    {
        PlaneManager.OnShareMesh -= GetMesh;

        _sourceTrianglesBuffer?.Release();
        _sourceVerticesBuffer?.Release();
        _spawnBuffer?.Release();
        _argsBuffer?.Release();
    }

    private void GetMesh(Mesh m) 
    {
        _mesh = m;
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
        _spawnBuffer = new ComputeBuffer(_spawnPositions.Length, sizeof(float) * 4, ComputeBufferType.Append);
        _spawnBuffer.SetCounterValue(0);

        if (GrassBlade) 
        {
            uint[] args = new uint[] {
            GrassBlade.GetIndexCount(0),
            (uint)_spawnPositions.Length,
            GrassBlade.GetIndexStart(0),
            GrassBlade.GetBaseVertex(0),
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
        _spawnShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        if (GrassMaterial)
            GrassMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer);

        _spawnShader.Dispatch(0, Mathf.CeilToInt(_quadCount / 128f), 1, 1);
        _spawnBuffer.GetData(_spawnPositions);
    }

    private void Update()
    {
        if (
            GrassBlade == null
            || GrassMaterial == null
            || _argsBuffer == null
            )
            return;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(GrassBlade, 0, GrassMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
    }

}
