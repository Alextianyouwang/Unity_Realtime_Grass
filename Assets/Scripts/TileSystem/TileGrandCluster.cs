using System;
using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-99)]
public class TileGrandCluster : MonoBehaviour
{
    [Header("re-enable to preview changes")]
    public Texture2D TileHeightmap;
    public float TileHeightMultiplier = 1;
    public float TileSize = 39.0625f;
    public int TileGridDimension = 512;
    public Vector2 TileGridCenterXZ;
    public Camera RenderCam;
    public bool ShowDebugTile = true;
    public Material DebugMaterial;
    public float LOD_Threshold_01 = 45;
    public float LOD_Threshold_12 = 125;
    public float MaxRenderDistance = 200;

    public bool EnableOcclusionCulling = true;

    public FoliageObjectData[] ObjectData;

    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private  TileChunkDispatcher[] _tileChunkDispatcher;

    private ComputeBuffer _windBuffer_external;
    private ComputeBuffer _maskBuffer_external;
    private RenderTexture _interactionTexture_external;

    public static Func<RenderTexture> OnRequestInteractionTexture;
    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestWindBuffer;
    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestMaskBuffer;
    public static Action<int> OnRequestDisposeWindBuffer;
    public static Action<int> OnRequestDisposeMaskBuffer;
    public static Func<RenderTexture> OnRequestOcclusionTexture;

    private int _hashcode;

    public static float _LOD_Threshold_01 { get; private set; }
    public static float _LOD_Threshold_12 { get; private set; }
    public static float _MaxRenderDistance { get; private set; }
    public static bool _EnableOcclusionCulling { get; private set; }

    private void OnEnable()
    {
        _hashcode = GetHashCode();
        SetupTileData();
        SetupTileDebug();
        InitializeInteractionTexture();
        InitializeWindBuffer();
        InitializeMaskBuffer();
        InitializeDispatcher();

    }
    private void OnDisable()
    {
        CleanupBuffers();
    }
    private void UpdateParam() 
    {
        _LOD_Threshold_01 = LOD_Threshold_01;
        _LOD_Threshold_12 = LOD_Threshold_12;
        _MaxRenderDistance = MaxRenderDistance;
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
            _tileData = new TileData(TileGridCenterXZ, TileHeightmap.width, TileSize, TileHeightmap,TileHeightMultiplier);
        else
            _tileData = new TileData(TileGridCenterXZ, TileGridDimension, TileSize, null, TileHeightMultiplier);
        _tileData.ConstructTileGrid();
    }

 
    public void InitializeInteractionTexture()
    {
        _interactionTexture_external = OnRequestInteractionTexture?.Invoke();
    }
    void InitializeWindBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _windBuffer_external = OnRequestWindBuffer?.Invoke(_hashcode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }

    void InitializeMaskBuffer() 
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _maskBuffer_external = OnRequestMaskBuffer?.Invoke(_hashcode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }
    void InitializeDispatcher() 
    {

        if (_tileData == null)
            return;
        if (RenderCam == null)
            return;
        if (ObjectData == null)
            return;
        if (ObjectData.Length == 0)
            return;
        _tileChunkDispatcher = new TileChunkDispatcher[ObjectData.Length];
        for (int i = 0; i < _tileChunkDispatcher.Length; i++) 
        {
            FoliageObjectData data = ObjectData[i];
            if (data.SpawnMesh == null)
                continue;
            foreach (Mesh m in data.SpawnMesh)
                if (m == null)
                    continue;
            if (data.SpawnMeshMaterial == null)
                continue;

            TileChunkDispatcher dispatcher = new TileChunkDispatcher(
           data.SpawnMesh,
           data.SpawnMeshMaterial,
           _tileData,
           RenderCam,
           _windBuffer_external,
           _maskBuffer_external,
           OnRequestOcclusionTexture.Invoke(),
           _interactionTexture_external,
           data.DensityMap,
           data.SquaredInstancePerTile,
           data.SquaredChunksPerCluster,
           data.SquaredTilePerClump,
           data.OccludeeBoundScaleMultiplier,
           data.DensityFilter,
           data.DensityFalloffThreshold,
           data.UseMask,
           data.ReverseMask) ;

            dispatcher.InitialSpawn();
            dispatcher.InitializeChunks();
            _tileChunkDispatcher[i] = dispatcher;
        }

    }
 
    void IndirectDrawPerFrame()
    {
        if (_tileChunkDispatcher == null)
            return;
        foreach (TileChunkDispatcher d in _tileChunkDispatcher) 
            d?.DispatchTileChunksDrawCall();

    }
    void CleanupBuffers()
    {
        _tileData?.ReleaseBuffer();
        OnRequestDisposeWindBuffer?.Invoke(_hashcode);
        OnRequestDisposeMaskBuffer?.Invoke(_hashcode);
        _tileVisualizer?.ReleaseBuffer();
        if (_tileChunkDispatcher != null)
            foreach (TileChunkDispatcher d in _tileChunkDispatcher)
                d?.ReleaseBuffer();
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

    private void OnDrawGizmos()
    {
        if (_tileChunkDispatcher == null)
            return;
        foreach (TileChunkDispatcher d in _tileChunkDispatcher)
        {
            if (d == null)
                return;
            if (d.Chunks == null)
                return;

            foreach (TileChunk c in d.Chunks)
            {
                if (c == null)
                    continue;
                Gizmos.DrawWireCube(c.ChunkBounds.center, c.ChunkBounds.size);
            }
        }
    }

}
