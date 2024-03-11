using UnityEngine;

public class TileData 
{
    public float TileSize { get; private set; }
    public int TileGridDimension { get; private set; }
    public Vector2 TileGridCenterXZ { get; private set; }
    public Tile[] TileGrid { get; private set; }

    private Vector2[] _tileVerts;
    public TileData(Vector2 _center, int _dimension, float _tileSize)
    {
        TileSize = _tileSize;  
        TileGridDimension = _dimension;
        TileGridCenterXZ = _center;
    }
    public void ConstructTileGrid()
    {
        Tile[] tiles = new Tile[TileGridDimension * TileGridDimension];
        float offset = -TileGridDimension * TileSize / 2 + TileSize / 2;
        Vector2 botLeftCorner = TileGridCenterXZ + new Vector2(offset, offset);

        for (int x = 0; x < TileGridDimension; x++)
        {
            for (int y = 0; y < TileGridDimension; y++)
            {
                Vector2 tilePos = botLeftCorner + new Vector2(TileSize * x, TileSize * y);
                tiles[x * TileGridDimension + y] = new Tile(TileSize, tilePos);
            }
        }
        TileGrid = tiles;
    }
    public Vector2[] GetTileVerts() 
    {
        if (TileGrid == null)
            return null;
        _tileVerts = new Vector2[TileGridDimension * TileGridDimension * 4];
        int i = 0;
        foreach (Tile t in TileGrid) 
        {
            Vector2[] corners = t.GetTileCorners();
            for (int j = 0;j < 4;j++)
                _tileVerts[i + j] = corners[j];
             i+=4;
        }
        return _tileVerts;
    }
   
}

public class Tile 
{
    private float _tileSize;
    private Vector2 _tilePosition;

    public Tile(float _tSize, Vector2 _tPos) 
    {
        _tileSize = _tSize;
        _tilePosition = _tPos;
    }

    public Vector3 GetTilePosSize() 
    {
        return new Vector3(_tilePosition.x, _tilePosition.y, _tileSize);
    }
    public Vector2[] GetTileCorners() 
    {
        Vector2[] corners = new Vector2[4];
        corners[0] = _tilePosition - Vector2.one * _tileSize / 2;
        corners[1] = _tilePosition + new Vector2 (1,-1) * _tileSize / 2;
        corners[2] = _tilePosition + Vector2.one * _tileSize / 2;
        corners[3] = _tilePosition + new Vector2(-1,1) * _tileSize / 2;
        return corners;
    }

}
