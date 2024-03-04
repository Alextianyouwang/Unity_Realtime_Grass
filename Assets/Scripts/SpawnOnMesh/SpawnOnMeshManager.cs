using UnityEngine;

[ExecuteInEditMode]
public class SpawnOnMeshManager : MonoBehaviour
{
    private ComputeShader _spawnShader;

    private ComputeBuffer _sourceVerticesBuffer;
    private ComputeBuffer _sourceTrianglesBuffer;
    private ComputeBuffer _disksBuffer;

    private SourceVertex[] _sourceVertices;
    private SpawnData[] _spawnPositions;
    private int[] _sourceTriangles;
    private int _quadCount;

    private Mesh _mesh;

    [Range (1,5)]
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
        _sourceTrianglesBuffer.Release();
        _sourceVerticesBuffer.Release();
        _disksBuffer.Release();
    }

    private void GetMesh(Mesh m) 
    {
        _mesh = m;
    }

    private void SpawnPoint() 
    {
        if (_mesh == null)
            return;
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
        _disksBuffer = new ComputeBuffer(_spawnPositions.Length, sizeof(float) * 4, ComputeBufferType.Append);
        _disksBuffer.SetCounterValue(0);

        _spawnShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _spawnShader.SetInt("_NumTriangles", _quadCount);
        _spawnShader.SetInt("_Subdivisions", Subdivision);

        _spawnShader.SetBuffer(0, "_SourceVerticesBuffer", _sourceVerticesBuffer);
        _spawnShader.SetBuffer(0, "_SourceTrianglesBuffer", _sourceTrianglesBuffer);
        _spawnShader.SetBuffer(0, "_DisksBuffer", _disksBuffer);

        _spawnShader.Dispatch(0, Mathf.CeilToInt(_quadCount / 128f), 1, 1);

        _disksBuffer.GetData(_spawnPositions);
    }

    private void OnDrawGizmos()
    {
        if (_spawnPositions == null)
            return;

        foreach (SpawnData p in _spawnPositions) 
        {
            Gizmos.DrawSphere(p.positionWS, 0.05f);
        }
    }
}
