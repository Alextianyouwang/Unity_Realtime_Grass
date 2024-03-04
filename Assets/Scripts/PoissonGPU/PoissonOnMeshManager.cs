using UnityEngine;

[ExecuteInEditMode]
public class PoissonOnMeshManager : MonoBehaviour
{
    public ComputeShader PoissonSpawner;

    private ComputeBuffer _sourceVerticesBuffer;
    private ComputeBuffer _sourceTrianglesBuffer;
    private ComputeBuffer _disksBuffer;

    private SourceVertex[] _sourceVertices;
    private PoissonDisk[] _spawnPositions;
    private int[] _sourceTriangles;
    private int _triangleCount;

    private Mesh _mesh;

    struct SourceVertex
    {
        public Vector3 positionOS;
        public Vector2 uv;
        public Vector3 normalOS;
    };

    struct PoissonDisk 
    {
        public Vector3 positionWS;
        public float radius;
    }

    private void OnEnable()
    {
        PlaneManager.OnShareMesh += GetMesh;
        GetMeshData(_mesh);
        InitializeShader();
    }
    private void OnDisable()
    {
        PlaneManager.OnShareMesh -= GetMesh;
    }

    private void GetMesh(Mesh m) 
    {
        _mesh = m;
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
        _triangleCount = _sourceTriangles.Length / 3;

        _spawnPositions = new PoissonDisk[_triangleCount];

    }

    private void InitializeShader() 
    {
        _sourceTrianglesBuffer = new ComputeBuffer(_sourceTriangles.Length, sizeof(int));
        _sourceTrianglesBuffer.SetData(_sourceTriangles);
        _sourceVerticesBuffer = new ComputeBuffer(_sourceVertices.Length, sizeof(float) * 8);
        _sourceVerticesBuffer.SetData(_sourceVertices);
        _disksBuffer = new ComputeBuffer(_triangleCount, sizeof(float) * 4, ComputeBufferType.Append);
        _disksBuffer.SetCounterValue(0);

        PoissonSpawner.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        PoissonSpawner.SetInt("_NumTriangles", _triangleCount);
        PoissonSpawner.SetBuffer(0, "_SourceVerticesBuffer", _sourceVerticesBuffer);
        PoissonSpawner.SetBuffer(0, "_SourceTrianglesBuffer", _sourceTrianglesBuffer);
        PoissonSpawner.SetBuffer(0, "_DisksBuffer", _disksBuffer);

        PoissonSpawner.Dispatch(0, Mathf.CeilToInt(_triangleCount / 128), 1, 1);

        _disksBuffer.GetData(_spawnPositions);
    }

    private void OnDrawGizmos()
    {
        if (_spawnPositions == null)
            return;

        foreach (PoissonDisk p in _spawnPositions) 
        {
            Gizmos.DrawSphere(p.positionWS, 0.05f);
        }
    }
}
