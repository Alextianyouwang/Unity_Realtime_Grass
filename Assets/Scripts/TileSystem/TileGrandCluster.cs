using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-99)]
public class TileGrandCluster : MonoBehaviour
{
    [Header("re-enable to preview changes")]
    public Texture2D TileHeightmap;
    public Texture2D TileTypemap;
    public float TileHeightMultiplier = 1;
    public float TileSize = 39.0625f;
    public int TileGridDimension = 512;
    public Vector2 TileGridCenterXZ;
    public Camera RenderCam;

    public Mesh[] SpawnMesh;
    public Material SpawnMeshMaterial;
    public int SquaredInstancePerTile = 3;
    public int SquaredChunksPerCluster = 4;
    public int SquaredTilePerClump = 8;
    public bool SmoothPlacement = true;
    public bool EnableInteraction = true;


    public float LOD_Threshold_01 = 45;
    public float LOD_Threshold_12 = 125;
    public float MaxRenderDistance = 200;
    public float DensityFalloffThreshold = 10;

    public bool ShowDebugTile = true;
    public Material DebugMaterial;

    public bool EnableOcclusionCulling = true;
    public Renderer[] Occluders;
    [Range(0f, 3f)]
    public float OccludeeBoundScaleMultiplier = 1;

    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private TileChunkDispatcher _tileChunkDispatcher;

    public static float _LOD_Threshold_01 { get; private set; }
    public static float _LOD_Threshold_12 { get; private set; }
    public static float _MaxRenderDistance { get; private set; }
    public static float _DensityFalloffThreshold { get; private set; }
    public static int _SquaredInstancePerTile { get; private set; }
    public static int _SquaredChunkPerCluster { get; private set; }
    public static int _SquaredTilePerClump { get; private set; }
    public static float _OccludeeBoundScaleMultiplier { get; private set; }
    public static bool _EnableOcclusionCulling { get; private set; }

    private void OnEnable()
    {
        UpdateParam();
        SetupTileData();
        SetupTileDebug();
        SetupDrawers();
   
    }
    private void OnDisable()
    {
        CleanupDebugVisualBuffers();
        CleanupDrawBuffers();
        CleanupTileDataBuffer();
    }
    private void UpdateParam() 
    {
        _LOD_Threshold_01 = LOD_Threshold_01;
        _LOD_Threshold_12 = LOD_Threshold_12;
        _MaxRenderDistance = MaxRenderDistance;
        _DensityFalloffThreshold = DensityFalloffThreshold;
        _SquaredInstancePerTile = SquaredInstancePerTile;
        _SquaredChunkPerCluster = SquaredChunksPerCluster;
        _SquaredTilePerClump = SquaredTilePerClump;
        _OccludeeBoundScaleMultiplier = OccludeeBoundScaleMultiplier;
        _EnableOcclusionCulling = EnableOcclusionCulling;
    }
    private void LateUpdate()
    {
        UpdateParam();
        if (ShowDebugTile)
            DrawDebugView();
        IndirectDrawPerFrame();
    }


    void SetupTileData() 
    {
        if (TileHeightmap)
            _tileData = new TileData(TileGridCenterXZ, TileHeightmap.width, TileSize, TileHeightmap,TileHeightMultiplier,TileTypemap);
        else
            _tileData = new TileData(TileGridCenterXZ, TileGridDimension, TileSize, null, TileHeightMultiplier, TileTypemap);
        _tileData.ConstructTileGrid();
    }

    void CleanupTileDataBuffer() 
    {
        _tileData?.ReleaseBuffer();
    }
    void SetupDrawers()
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
            RenderCam, 
            SmoothPlacement,
            Occluders);
        _tileChunkDispatcher.InitialSpawn();
        _tileChunkDispatcher.GetWindBuffer();
        if (EnableInteraction)
            _tileChunkDispatcher.GetInteractionTexture();
        _tileChunkDispatcher.InitializeChunks();
    }
    void IndirectDrawPerFrame()
    {
        _tileChunkDispatcher?.BlitDepthTexture();
        _tileChunkDispatcher?.DispatchTileChunksDrawCall();


    }
    void CleanupDrawBuffers()
    {
        _tileChunkDispatcher?.ReleaseBuffer();
    }

    void SetupTileDebug() 
    {
        if (_tileData == null)
            return;
        _tileVisualizer = new TileVisualizer(_tileData,DebugMaterial);
        _tileVisualizer.InitializeTileDebug();
    }
    void DrawDebugView()
    {
        _tileVisualizer?.DrawIndirect();
    }
    void CleanupDebugVisualBuffers() 
    {
        _tileVisualizer?.ReleaseBuffer();
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
