using System;
using System.Linq;
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
    public float LOD_Threshold_01 = 45;
    public float LOD_Threshold_12 = 125;
    public float MaxRenderDistance = 200;

    public bool EnableOcclusionCulling = true;

    public FoliageObjectData[] ObjectData;
    public TileComponent[] TileComponents;

    private TileData _tileData;
    private TileChunkDispatcher[] _tileChunkDispatcher;

    private ComputeBuffer _windBuffer;
    private ComputeBuffer _maskBuffer;
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
        InitializeInteractionTexture();
        InitializeWindBuffer();
        InitializeMaskBuffer();
        InitializeDispatcher();
        InitializeTileComponent();

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
        IndirectDrawPerFrame();
        UpdateTileComponent();
    }


    void SetupTileData()
    {
        if (TileHeightmap)
            _tileData = new TileData(TileGridCenterXZ, TileHeightmap.width, TileSize, TileHeightmap, TileHeightMultiplier);
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
        _windBuffer = OnRequestWindBuffer?.Invoke(_hashcode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }

    void InitializeMaskBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _maskBuffer = OnRequestMaskBuffer?.Invoke(_hashcode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
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
           _windBuffer,
           _maskBuffer,
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
           data.ReverseMask);

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
        if (_tileChunkDispatcher != null)
            foreach (TileChunkDispatcher d in _tileChunkDispatcher)
                d?.ReleaseBuffer();
        DisposeTileComponent();
    }


    private void InitializeTileComponent() =>  TileComponents?.ToList().ForEach(t => t?.Initialization(_tileData,_windBuffer,_maskBuffer,_hashcode));
    private void UpdateTileComponent() =>  TileComponents?.ToList().ForEach(t => t?.UpdateEffect(_hashcode));
    private void DisposeTileComponent() => TileComponents?.ToList().ForEach(t => t?.DisposeEffect(_hashcode));
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
