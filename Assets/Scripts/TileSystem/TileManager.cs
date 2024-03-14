using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-98)]
public class TileManager : MonoBehaviour
{
    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private TileChunkDispatcher _tileChunkDispatcher;
    public Texture2D TileHeightmap;
    public float TileHeightMultiplier = 1;
    public float TileSize = 39.0625f;
    public int TileGridDimension = 512;
    public Vector2 TileGridCenterXZ;
    public Camera RenderCam;

    public Mesh[] SpawnMesh;
    public Material SpawnMeshMaterial;
    public int SpawnSubivisions = 3;
    public bool SmoothPlacement = true;
    public int ChunksPerSide = 4;
    public float LOD_Threshold_01 = 20;
    public float LOD_Threshold_12 = 40;

    public bool ShowDebugView = true;
    public Material DebugMaterial;


    private void OnEnable()
    {
        SetupTileGrid();
        SetupTileDebug();
        SetupTileFunctions();
    }
    private void OnDisable()
    {
        CleanupTileVisual();
        CleanupTileFunction();
    }
    private void Update()
    {
        if (ShowDebugView)
            DrawDebugView();
        SpawnObjectIndirect();
    }
 
    void SetupTileGrid() 
    {
        if (TileHeightmap)
            _tileData = new TileData(TileGridCenterXZ, TileHeightmap.width, TileSize, TileHeightmap,TileHeightMultiplier);
        else
            _tileData = new TileData(TileGridCenterXZ, TileGridDimension, TileSize, null, TileHeightMultiplier);
        _tileData.ConstructTileGrid();
    }
    void SetupTileFunctions()
    {
        if (_tileData == null)
            return;
        if (RenderCam == null)
            return;
        if (SpawnMesh == null)
            return;
        foreach (Mesh m in SpawnMesh)
            if (m == null)
                return;
        if (SpawnMeshMaterial == null) 
            return;
        _tileChunkDispatcher = new TileChunkDispatcher(SpawnMesh, SpawnMeshMaterial, _tileData, SpawnSubivisions, RenderCam, SmoothPlacement,ChunksPerSide, LOD_Threshold_01,LOD_Threshold_12);
        _tileChunkDispatcher.InitialSpawn();
        _tileChunkDispatcher.InitializeChunks();
    }
    void SpawnObjectIndirect()
    {
        if (_tileChunkDispatcher == null)
            return;
        _tileChunkDispatcher.DispatchTileChunksDrawCall();
    }
    void CleanupTileFunction()
    {
        if (_tileChunkDispatcher == null)
            return;
        _tileChunkDispatcher.ReleaseBuffer();
    }

    void SetupTileDebug() 
    {
        if (_tileData == null)
            return;
        _tileVisualizer = new TileVisualizer(_tileData.TileGrid,DebugMaterial, _tileData.TileGridDimension);
        _tileVisualizer.VisualizeTiles();
    }
    void DrawDebugView()
    {
        if (_tileVisualizer == null)
            return;
        _tileVisualizer.DrawIndirect();
    }
    void CleanupTileVisual() 
    {
        if (_tileVisualizer == null)
            return;
        _tileVisualizer.ReleaseBuffer();
    }

    private void OnDrawGizmos()
    {
        if (_tileChunkDispatcher == null)
            return;
        if (_tileChunkDispatcher.Chunks == null)
            return;
        foreach (TileChunk c in _tileChunkDispatcher.Chunks) 
        {
            if (c == null)
                continue;
            Gizmos.DrawWireCube(c.ChunkBounds.center, c.ChunkBounds.size);
        }
    }

}
