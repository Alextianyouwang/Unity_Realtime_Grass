using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-98)]
public class TileGrandCluster : MonoBehaviour
{

    public Texture2D TileHeightmap;
    public Texture2D TileTypemap;
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
    public int TilePerClump = 8;
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
    public static int _SpawnSubdivisions;


    private void OnEnable()
    {
        _LOD_Threshold_01 = LOD_Threshold_01;
        _LOD_Threshold_12 = LOD_Threshold_12;
        _MaxRenderDistance = MaxRenderDistance;
        _DensityFalloffThreshold = DensityFalloffThreshold;
        _SpawnSubdivisions = SpawnSubivisions;
        SetupTileData();
        SetupTileDebug();
        SetupSpawner();
   
    }
    private void OnDisable()
    {
        CleanupDebugVisualBuffers();
        CleanupDrawBuffers();
        CleanupTileDataBuffer();
    }
    private void LateUpdate()
    {
        UpdateWindData();
        if (ShowDebugView)
            DrawDebugView();
        SpawnObjectIndirect();
    }


    void SetupTileData() 
    {
        if (TileHeightmap)
            _tileData = new TileData(TileGridCenterXZ, TileHeightmap.width, TileSize, TileHeightmap,TileHeightMultiplier,TileTypemap);
        else
            _tileData = new TileData(TileGridCenterXZ, TileGridDimension, TileSize, null, TileHeightMultiplier, TileTypemap);
        _tileData.ConstructTileGrid();
        _tileData.InitializeWind();
    }

    void UpdateWindData() 
    {
        _tileData?.UpdateWind();
    }

    void CleanupTileDataBuffer() 
    {
        _tileData?.ReleaseBuffer();
    }
    void SetupSpawner()
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
            ChunksPerSide,
            TilePerClump);
        _tileChunkDispatcher.InitialSpawn();
        _tileChunkDispatcher.InitializeChunks();
    }
    void SpawnObjectIndirect()
    {

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
        _tileVisualizer = new TileVisualizer(_tileData.TileGrid,DebugMaterial, _tileData.TileGridDimension);
        _tileVisualizer.GetNoiseBuffer(_tileData.WindBuffer);
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
