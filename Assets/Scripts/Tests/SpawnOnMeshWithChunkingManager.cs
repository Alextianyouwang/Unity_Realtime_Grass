using UnityEngine;

[ExecuteInEditMode]
public class SpawnOnMeshWithChunkingManager : MonoBehaviour
{
    private ComputeShader _gridShader;

    private Mesh _mesh;

    public Mesh TestMesh;
    public Material RenderingMaterial;
    public int NumChunk = 3;
    public int InstancePerChunk = 3;
    private Chunk[] _dataChunks;
    private uint[] _chunkArgs;

    private void OnEnable()
    {

        _gridShader = (ComputeShader)Resources.Load("CS/CS_ChunkGrid");
        TryGetMesh();
        SpawnPoint();
    }
    private void OnDisable()
    {
        ReleaseChunk();
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
      
        InitializeChunk();
    }
    private void InitializeChunk()
    {
        if (_mesh == null)
            return;
        if (TestMesh == null)
            return;
        Bounds b = _mesh.bounds;
        Vector3 initialPoint = b.center - b.extents;
     
        float chunkInc = b.size.x / NumChunk;
        float pointInc = chunkInc /  InstancePerChunk;
        _dataChunks = new Chunk[NumChunk * NumChunk];

        _chunkArgs = new uint[] {
            TestMesh.GetIndexCount(0),
            (uint)(InstancePerChunk*InstancePerChunk),
            TestMesh.GetIndexStart(0),
            TestMesh.GetBaseVertex(0),
            0
        };

        for (int x = 0; x < NumChunk; x++) 
        {
            for (int z = 0; z < NumChunk; z++) 
            {
                Vector2 root = new Vector2 (initialPoint.x,initialPoint.z) + new Vector2 (x * chunkInc,z * chunkInc);
                Vector3[] flatPositions = new Vector3[InstancePerChunk * InstancePerChunk];

                Chunk c = new Chunk() {
                    spawnBuffer = new ComputeBuffer(flatPositions.Length, sizeof(float) * 3),
                    argsBuffer = new ComputeBuffer(1, sizeof(float) * 5,ComputeBufferType.IndirectArguments),
                    mpb = new MaterialPropertyBlock()
                };
                c.spawnBuffer.SetData(flatPositions);
                c.argsBuffer.SetData(_chunkArgs);

                c.mpb.SetBuffer("_SpawnBuffer", c.spawnBuffer);
                c.mpb.SetColor("_ChunkColor", Random.ColorHSV(0, 1, 0, 1, 0.5f, 1, 0.5f, 1));
                _gridShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
                _gridShader.SetInt("_NumPointPerSide", InstancePerChunk);
                _gridShader.SetFloat("_Xoffset", root.x);
                _gridShader.SetFloat("_Yoffset", root.y);
                _gridShader.SetFloat("_Increment", pointInc);
                _gridShader.SetBuffer(0, "_SpawnDataBuffer", c.spawnBuffer);

                _gridShader.Dispatch(0, Mathf.CeilToInt (InstancePerChunk  / 8f), Mathf.CeilToInt(InstancePerChunk / 8f), 1);
                _dataChunks[x * NumChunk + z] = c;
            }
        }
    }
    private void ReleaseChunk() 
    {
        foreach (Chunk chunk in _dataChunks) 
        {
            chunk.spawnBuffer?.Release();
            chunk.argsBuffer?.Release();
        }
    }

    private void Update()
    {
        if (
            TestMesh == null
            || RenderingMaterial == null
            )
            return;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        for (int x = 0; x < NumChunk; x++)
        {
            for (int z = 0; z < NumChunk; z++)
            {
                Chunk c = _dataChunks[x * NumChunk + z];
                if (c.argsBuffer == null)
                    continue;
                Graphics.DrawMeshInstancedIndirect(TestMesh, 0,RenderingMaterial, bounds, c.argsBuffer,0, c.mpb);
            }
        }

    }

}

public struct Chunk
{
    public ComputeBuffer spawnBuffer;
    public ComputeBuffer argsBuffer;
    public MaterialPropertyBlock mpb;
}
