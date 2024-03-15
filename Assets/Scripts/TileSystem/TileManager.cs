using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-98)]
public class TileManager : MonoBehaviour
{

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
    public float LOD_Threshold_01 = 45;
    public float LOD_Threshold_12 = 125;
    public float MaxRenderDistance = 200;
    public float DensityFalloffThreshold = 10;

    public bool ShowDebugView = true;
    public Material DebugMaterial;


    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private TileChunkDispatcher _tileChunkDispatcher;
    public static float _LOD_Threshold_01;
    public static float _LOD_Threshold_12;
    public static float _MaxRenderDistance;
    public static float _DensityFalloffThreshold;


    private void OnEnable()
    {
        _LOD_Threshold_01 = LOD_Threshold_01;
        _LOD_Threshold_12 = LOD_Threshold_12;
        _MaxRenderDistance = MaxRenderDistance;
        _DensityFalloffThreshold = DensityFalloffThreshold;
        SetupTileGrid();
        SetupTileDebug();
        SetupTileFunctions();
   
    }
    private void OnDisable()
    {
        CleanupTileVisual();
        CleanupTileFunction();
    }
    private void LateUpdate()
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
        _tileChunkDispatcher = new TileChunkDispatcher(
            SpawnMesh, 
            SpawnMeshMaterial, 
            _tileData, 
            SpawnSubivisions, 
            RenderCam, 
            SmoothPlacement,
            ChunksPerSide);
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
