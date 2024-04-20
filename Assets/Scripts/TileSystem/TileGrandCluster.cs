using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
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
    public bool EnableOcclusionCulling = true;
    public Renderer[] Occluders;
    public bool ShowDebugTile = true;
    public Material DebugMaterial;


    public float LOD_Threshold_01 = 45;
    public float LOD_Threshold_12 = 125;
    public float MaxRenderDistance = 200;
    public float DensityFalloffThreshold = 10;

    public FoliageObjectData[] ObjectData;

    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private  TileChunkDispatcher[] _tileChunkDispatcher;

    private RenderTexture _zTex;
    private Material _zMat;
    private Mesh _occluder;

    private ComputeBuffer _windBuffer_external;
    private RenderTexture _interactionTexture_external;

    public static Func<RenderTexture> OnRequestInteractionTexture;
    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestWindBuffer;
    public static Action<int> OnRequestDisposeWindBuffer;

    public static float _LOD_Threshold_01 { get; private set; }
    public static float _LOD_Threshold_12 { get; private set; }
    public static float _MaxRenderDistance { get; private set; }
    public static float _DensityFalloffThreshold { get; private set; }
    public static bool _EnableOcclusionCulling { get; private set; }

    private void OnEnable()
    {
        SetupTileData();
        SetupTileDebug();
        InitializeOcclusionSetup();
        InitializeInteractionTexture();
        InitializeWindBuffer();
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
        _DensityFalloffThreshold = DensityFalloffThreshold;
        _EnableOcclusionCulling = EnableOcclusionCulling;
    }
    private void LateUpdate()
    {
        UpdateParam();
        if (ShowDebugTile)
            DrawDebugView();
        DrawOcclusionTexture();
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

    public void DrawOcclusionTexture()
    {
        if (!_EnableOcclusionCulling)
            return;
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "DrawOccluderDepth";
        cmd.SetViewMatrix(RenderCam.worldToCameraMatrix);
        cmd.SetProjectionMatrix(RenderCam.projectionMatrix);
        cmd.SetRenderTarget(_zTex);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(_occluder, Matrix4x4.identity, _zMat);
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    public void InitializeInteractionTexture()
    {
        _interactionTexture_external = OnRequestInteractionTexture?.Invoke();
    }
    void InitializeWindBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _windBuffer_external = OnRequestWindBuffer?.Invoke(GetHashCode(), _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
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
           _zTex,
           _interactionTexture_external,
           data.SquaredInstancePerTile,
           data.SquaredChunksPerCluster,
           data.SquaredTilePerClump,
           data.OccludeeBoundScaleMultiplier,
           data.DensityFilter);

            dispatcher.InitialSpawn();
            dispatcher.InitializeChunks();
            _tileChunkDispatcher[i] = dispatcher;
        }

    }
    void InitializeOcclusionSetup() 
    {
        if (!EnableOcclusionCulling)
            return;
        _zTex = RenderTexture.GetTemporary(RenderCam.pixelWidth, RenderCam.pixelHeight, 32, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        _zTex.Create();
        _zMat = new Material(Shader.Find("Utility/S_DepthOnly"));
        _occluder = Utility.CombineMeshes(Occluders.Select(x => x.gameObject).ToArray());
    }
    void IndirectDrawPerFrame()
    {
        foreach (TileChunkDispatcher d in _tileChunkDispatcher) 
        {
            d?.DispatchTileChunksDrawCall();
        }

    }
    void CleanupBuffers()
    {
        _tileData?.ReleaseBuffer();
        OnRequestDisposeWindBuffer?.Invoke(GetHashCode());
        _tileVisualizer?.ReleaseBuffer();
        RenderTexture.ReleaseTemporary(_zTex);
        foreach (TileChunkDispatcher d in _tileChunkDispatcher)
        {
            d?.ReleaseBuffer();
        }

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
