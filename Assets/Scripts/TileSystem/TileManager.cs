using UnityEngine;
[ExecuteAlways]
[DefaultExecutionOrder(-98)]
public class TileManager : MonoBehaviour
{
    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    private TileFunctions _tileFunctions;
    public Texture2D TileHeightmap;
    public float TileHeightMultiplier = 1;
    public float TileSize;
    public int TileGridDimension = 512;
    public Vector2 TileGridCenterXZ;
    public Camera RenderCam;

    public Mesh SpawnMesh;
    public Material SpawnMeshMaterial;
    public int SpawnSubivisions = 3;
    public bool SmoothPlacement = true;

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
        _tileFunctions = new TileFunctions(SpawnMesh, SpawnMeshMaterial, _tileData, SpawnSubivisions,RenderCam,SmoothPlacement);
        _tileFunctions.SetupTileCompute();
    }
    void SpawnObjectIndirect()
    {
        if (_tileFunctions == null)
            return;
        _tileFunctions.DrawIndirect();
    }
    void CleanupTileFunction()
    {
        if (_tileFunctions == null)
            return;
        _tileFunctions.ReleaseBuffer();
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
        if (_tileFunctions == null)
            return;
        _tileVisualizer.GetNoiseBuffer(_tileFunctions.ShareNoiseBuffer());
        _tileVisualizer.DrawIndirect();
    }
    void CleanupTileVisual() 
    {
        if (_tileVisualizer == null)
            return;
        _tileVisualizer.ReleaseBuffer();

    }

}
