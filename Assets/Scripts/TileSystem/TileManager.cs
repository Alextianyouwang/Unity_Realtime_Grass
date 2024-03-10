using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-98)]
public class TileManager : MonoBehaviour
{
    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    public float TileSize;
    public int TileGridDimention;
    public Vector2 TileGridCenterXZ;
    public Material RenderMaterial;

    private void OnEnable()
    {
        SetupTileGrid();
        SetupTileVisual();
    }
    private void OnDisable()
    {
        CleanupTileVisual();
    }
    private void Update()
    {
        DrawTileVisual();
    }

    void SetupTileGrid() 
    {
        _tileData = new TileData(TileGridCenterXZ, TileGridDimention, TileSize);
        _tileData.ConstructTileGrid();
        
    }
    void SetupTileVisual() 
    {
        if (_tileData == null)
            return;
        _tileVisualizer = new TileVisualizer(_tileData.TileGrid,RenderMaterial, _tileData.TileGridDimension);
        _tileVisualizer.VisualizeTiles();
    }
    void DrawTileVisual()
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
}
