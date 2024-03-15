using UnityEngine;

public class TileChunkDispatcher
{
    public TileChunk[] Chunks { get; private set; }
    private TileData _tileData;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _rawSpawnBuffer; 
    private ComputeBuffer _vertBuffer;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _renderCam;

    struct SpawnData
    {
        Vector3 positionWS;
        float hash;
    };
    private SpawnData[] _spawnData;
    private int _tileCount;

    private bool _smoothPlacement;
    private int _spawnSubivisions;
    private int _chunksPerSide;

    public TileChunkDispatcher(Mesh[] spawnMesh, Material spawmMeshMat, TileData tileData, int spawnSubD, Camera renderCam, bool smoothPlacement, int chunkPerSide)
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _tileData = tileData;
        _spawnSubivisions = spawnSubD;
        _renderCam = renderCam;
        _smoothPlacement = smoothPlacement;
        _chunksPerSide = chunkPerSide;
    }

    public void InitialSpawn()
    {
        _spawnOnTileShader = (ComputeShader)Resources.Load("CS_InitialSpawn");
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _spawnSubivisions * _spawnSubivisions;
        _vertBuffer = new ComputeBuffer(_tileCount * 4, sizeof(float) * 3);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _spawnData = new SpawnData[_tileCount * instancePerTile];
        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 4);
        _rawSpawnBuffer.SetData(_spawnData);

        _spawnOnTileShader.SetInt("_NumTiles", _tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", _spawnSubivisions);
        _spawnOnTileShader.SetInt("_NumTilesPerSide", _tileData.TileGridDimension);
        _spawnOnTileShader.SetBool("_SmoothPlacement", _smoothPlacement);

        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _vertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _rawSpawnBuffer);
        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);
    }

    public void InitializeChunks() 
    {
        Chunks = new TileChunk[_chunksPerSide * _chunksPerSide];
        int chunkDimension = _tileData.TileGridDimension / _chunksPerSide;
        int totalInstancePerChunk = chunkDimension * chunkDimension * _spawnSubivisions * _spawnSubivisions;
        float chunkSize = _tileData.TileGridDimension * _tileData.TileSize / _chunksPerSide;
        Vector2 botLeft = _tileData.TileGridCenterXZ - chunkSize * _chunksPerSide * Vector2.one / 2 + Vector2.one * chunkSize / 2;
        
        for (int x = 0; x < _chunksPerSide; x++)
        {
            for (int y = 0; y < _chunksPerSide; y++) 
            {
                SpawnData[] spawnDatas = new SpawnData[totalInstancePerChunk];
                ComputeBuffer chunkBuffer = new ComputeBuffer(totalInstancePerChunk, sizeof(float) * 4);
                chunkBuffer.SetData(spawnDatas);
                _spawnOnTileShader.SetInt("_ChunkIndexX", x);
                _spawnOnTileShader.SetInt("_ChunkIndexY", y);
                _spawnOnTileShader.SetInt("_ChunkPerSide", _chunksPerSide);
                _spawnOnTileShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer);
                _spawnOnTileShader.SetBuffer(1, "_ChunkSpawnBuffer", chunkBuffer);
                _spawnOnTileShader.Dispatch(1, Mathf.CeilToInt(totalInstancePerChunk / 128f), 1, 1);
                Bounds b = new Bounds( new Vector3 (botLeft.x + chunkSize* x,0,botLeft.y + chunkSize * y) ,Vector3.one * chunkSize);
                Chunks[x * _chunksPerSide + y] = new TileChunk(
                    _spawnMesh, 
                    _spawnMeshMaterial, 
                    _renderCam, 
                    chunkBuffer, 
                    (ComputeShader)Resources.Load("CS_GrassCulling"),
                    b);
                Chunks[x * _chunksPerSide + y].Setup();
            }
        }
    }
    public void DispatchTileChunksDrawCall() 
    {
        foreach (TileChunk t in Chunks)
            t?.DrawIndirect();
    }


    public void ReleaseBuffer()
    {
        _vertBuffer?.Dispose();
        _rawSpawnBuffer?.Dispose();
        foreach (TileChunk t in Chunks)
            t?.ReleaseBuffer();
    }
}

