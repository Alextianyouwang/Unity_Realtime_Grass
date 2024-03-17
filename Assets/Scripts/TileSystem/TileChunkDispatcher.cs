using UnityEngine;
using UnityEngine.UIElements;

public class TileChunkDispatcher
{
    public TileChunk[] Chunks { get; private set; }
    private TileData _tileData;
    private TileClumpParser _tileClumpParser;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _rawSpawnBuffer; 
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _typeBuffer;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _renderCam;

    struct SpawnData
    {
        Vector3 positionWS;
        float hash;
        Vector4 clumpInfo;
        float density;
    };
    private SpawnData[] _spawnData;
    private int _tileCount;

    private bool _smoothPlacement;
    private int _spawnSubivisions;
    private int _chunksPerSide;
    private int _tilePerClump;

    public TileChunkDispatcher(Mesh[] spawnMesh, Material spawmMeshMat, TileData tileData, int spawnSubD, Camera renderCam, bool smoothPlacement, int chunkPerSide, int tilePerClump)
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _tileData = tileData;
        _spawnSubivisions = spawnSubD;
        _renderCam = renderCam;
        _smoothPlacement = smoothPlacement;
        _chunksPerSide = chunkPerSide;
        _tilePerClump = tilePerClump;
    }


    public void InitialSpawn()
    {
        _spawnOnTileShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS_InitialSpawn"));
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _spawnSubivisions * _spawnSubivisions;
        _vertBuffer = new ComputeBuffer(_tileCount * 4, sizeof(float) * 3);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _typeBuffer = new ComputeBuffer(_tileCount,sizeof(float));
        _typeBuffer.SetData(_tileData.GetTileType());

        _spawnData = new SpawnData[_tileCount * instancePerTile];
        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 9);
        _rawSpawnBuffer.SetData(_spawnData);

        _spawnOnTileShader.SetInt("_NumTiles", _tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", _spawnSubivisions);
        _spawnOnTileShader.SetInt("_NumTilesPerSide", _tileData.TileGridDimension);
        _spawnOnTileShader.SetBool("_SmoothPlacement", _smoothPlacement);

        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _vertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_TypeBuffer", _typeBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _rawSpawnBuffer);
        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);
    }


    public ComputeBuffer ProcessWithClumpData() 
    {
        _tileClumpParser = new TileClumpParser(
            _rawSpawnBuffer,
            _tilePerClump,
            _tileData.TileGridDimension,
            _tileData.TileSize,
            _tileData.TileGridCenterXZ - Vector2.one * _tileData.TileGridDimension * _tileData.TileSize * 0.5f
            );
        _tileClumpParser.ParseClump();
        return _tileClumpParser.ShareSpawnBuffer();
    }
    public void InitializeChunks() 
    {
        _rawSpawnBuffer =  ProcessWithClumpData();
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
                ComputeBuffer chunkBuffer = new ComputeBuffer(totalInstancePerChunk, sizeof(float) * 9);
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
                    // Still don't know whats the difference between using a single compute shader
                    // and using multiple instanced compute shaders, currently go with the second.
                    (ComputeShader)Resources.Load("CS_GrassCulling"),
                    b);
                Chunks[x * _chunksPerSide + y].Setup();
            }
        }
    }

    public void DispatchTileChunksDrawCall() 
    {
        Plane[] p= GeometryUtility.CalculateFrustumPlanes(_renderCam);
        foreach (TileChunk t in Chunks) 
            if (GeometryUtility.TestPlanesAABB(p, t.ChunkBounds))
                t.DrawIndirect();
    }

    public void ReleaseBuffer()
    {
        _vertBuffer?.Dispose();
        _typeBuffer?.Dispose();
        _rawSpawnBuffer?.Dispose();
        foreach (TileChunk t in Chunks)
            t?.ReleaseBuffer();
        _tileClumpParser?.ReleaseBuffer();
    }
}

