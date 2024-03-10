using UnityEngine;

public class TileVisualizer 
{
    private Tile[] _tiles;
    private GameObject _visualObj;
    private int _tileDimentions;
    private bool _hasDrawVisual = false;

    public TileVisualizer(Tile[] _t, int _dimention , GameObject _visual) 
    {
        _tiles = _t;
        _visualObj = _visual;
        _tileDimentions = _dimention;
    }

    public void VisualizeTiles() 
    {
        if (_hasDrawVisual)
            return;
        _hasDrawVisual = true;
        for (int x = 0; x < _tileDimentions; x++)
        {
            for (int y = 0; y < _tileDimentions; y++)
            {
                Tile currentTile = _tiles[x * _tileDimentions + y];
                Vector3 posSize = currentTile.GetTilePosSize();
                GameObject g = GameObject.Instantiate(_visualObj);
                g.transform.position = new Vector3 (posSize.x, 0, posSize.y);
                g.transform.localScale = Vector3.one * 0.99f * posSize.z / 10 ;
            }
        }
    }
}
