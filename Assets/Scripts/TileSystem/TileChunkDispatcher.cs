using UnityEngine;
public class TileChunkDispatcher
{
    public TileChunk[] Chunks { get; private set; }
    private TileData _tileData;
    private TileClumpParser _tileClumpParser;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _rawSpawnBuffer;
    private ComputeBuffer _groundNormalBuffer;
    private ComputeBuffer _windBuffer_external;

    private RenderTexture _interactionTexture_external;
    private RenderTexture _zTex_external;

    private Texture2D _densityMap;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _renderCam;


    private int _tileCount;

    private int _squaredInstancePerTile;
    private int _squaredChunksPerCluster;
    private int _squaredTilePerClump;

    private float _occludeeBoundScaleMultiplier;
    private float _densityFilter;
    private float _densityFalloffThreshold;


    public TileChunkDispatcher(Mesh[] spawnMesh, Material spawmMeshMat, TileData tileData, Camera renderCam, ComputeBuffer windBuffer_external,RenderTexture zTex_external, RenderTexture interactionTexture_external, Texture2D densityMap,
        int squaredInstancePerTile, int squaredChunksPerCluster, int squaredTilePerClump, float occludeeBoundScaleMultiplier, float densityFilter, float densityFalloffThreshold)
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _tileData = tileData;
        _renderCam = renderCam;
        _windBuffer_external = windBuffer_external;
        _zTex_external = zTex_external;
        _interactionTexture_external = interactionTexture_external;
        _densityMap = densityMap;
        _squaredInstancePerTile = squaredInstancePerTile;
        _squaredChunksPerCluster = squaredChunksPerCluster;
        _squaredTilePerClump = squaredTilePerClump;
        _occludeeBoundScaleMultiplier = occludeeBoundScaleMultiplier;
        _densityFilter = densityFilter;
        _densityFalloffThreshold = densityFalloffThreshold;
    }


    public void InitialSpawn()
    {
        _spawnOnTileShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS/CS_InitialSpawn"));
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _squaredInstancePerTile * _squaredInstancePerTile;

        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 12);
        _groundNormalBuffer = new ComputeBuffer(_tileCount, sizeof(float) * 3);

        _spawnOnTileShader.SetInt("_NumTiles", _tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", _squaredInstancePerTile);
        _spawnOnTileShader.SetInt("_NumTilesPerSide", _tileData.TileGridDimension);

        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _tileData.VertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _rawSpawnBuffer);
        _spawnOnTileShader.SetBuffer(0, "_GroundNormalBuffer", _groundNormalBuffer);

        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);


    }

    private ComputeBuffer ProcessWithClumpData() 
    {
        _tileClumpParser = new TileClumpParser(
            _rawSpawnBuffer,
            _tileData.TileGridDimension,
            _tileData.TileSize,
            _tileData.TileGridCenterXZ - Vector2.one * _tileData.TileGridDimension * _tileData.TileSize * 0.5f,
            _squaredTilePerClump
            );
        _tileClumpParser.ParseClump();
        return _tileClumpParser.ShareSpawnBuffer();
    }



    public void InitializeChunks() 
    {
        _rawSpawnBuffer =  ProcessWithClumpData();
        int chunksPerSide = _squaredChunksPerCluster;
        Chunks = new TileChunk[chunksPerSide * chunksPerSide];
        int chunkDimension = _tileData.TileGridDimension / chunksPerSide;
        int totalInstancePerChunk = chunkDimension * chunkDimension * _squaredInstancePerTile * _squaredInstancePerTile;
        float chunkSize = _tileData.TileGridDimension * _tileData.TileSize / chunksPerSide;
        Vector2 botLeft = _tileData.TileGridCenterXZ - chunkSize * chunksPerSide * Vector2.one / 2 + Vector2.one * chunkSize / 2;
        
        for (int x = 0; x < chunksPerSide; x++)
        {
            for (int y = 0; y < chunksPerSide; y++) 
            {
                ComputeBuffer chunkBuffer = new ComputeBuffer(totalInstancePerChunk, sizeof(float) * 12);
                _spawnOnTileShader.SetInt("_ChunkIndexX", x);
                _spawnOnTileShader.SetInt("_ChunkIndexY", y);
                _spawnOnTileShader.SetInt("_ChunkPerSide", chunksPerSide);
                _spawnOnTileShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer);
                _spawnOnTileShader.SetBuffer(1, "_ChunkSpawnBuffer", chunkBuffer);
                _spawnOnTileShader.Dispatch(1, Mathf.CeilToInt(totalInstancePerChunk / 128f), 1, 1);
                Bounds b = new Bounds( new Vector3 (botLeft.x + chunkSize* x,0,botLeft.y + chunkSize * y) ,Vector3.one * chunkSize);
                TileChunk t = Chunks[x * chunksPerSide + y] = new TileChunk(
                    _spawnMesh, 
                    _spawnMeshMaterial, 
                    _renderCam, 
                    chunkBuffer, 
                    b,
                    _tileData,
                    _densityMap,
                    _occludeeBoundScaleMultiplier,
                    _densityFilter,
                    _densityFalloffThreshold
                    );
                t.SetWindBuffer(_windBuffer_external);
                t.SetGroundNormalBuffer(_groundNormalBuffer);
                t.SetInteractionTexture(_interactionTexture_external);
                t.SetupCuller();
            }
        }
    }

    public void DispatchTileChunksDrawCall() 
    {
        int totalInstance = 0;
        Plane[] p= GeometryUtility.CalculateFrustumPlanes(_renderCam);
        foreach (TileChunk t in Chunks)
            if (GeometryUtility.TestPlanesAABB(p, t.ChunkBounds))
                if (t != null)
                {
                    t.SetZTex(_zTex_external);
                    t.DrawContent(ref totalInstance);
                }
        //Debug.Log(totalInstance);
    }
    public void ReleaseBuffer()
    {
        _rawSpawnBuffer?.Dispose();
        _groundNormalBuffer?.Dispose();
        foreach (TileChunk t in Chunks)
            t?.ReleaseBuffer();
        _tileClumpParser?.ReleaseBuffer();
    }
}

