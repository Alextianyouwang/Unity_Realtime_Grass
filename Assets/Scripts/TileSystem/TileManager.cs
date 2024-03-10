using UnityEngine;
[ExecuteInEditMode]
public class TileManager : MonoBehaviour
{
    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    public float TileSize;
    public int TileGridDimention;
    public Vector2 TileGridCenterXZ;
    public GameObject TileVisual;
    private void OnEnable()
    {
        SetupTileGrid();
        SetupTileVisual();
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
        _tileVisualizer = new TileVisualizer(_tileData.TileGrid, _tileData.TileGridDimension, TileVisual);
        _tileVisualizer.VisualizeTiles();
    }
}
