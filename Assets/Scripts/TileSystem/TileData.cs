using System.Linq;
using UnityEngine;

public class TileData 
{
    public float TileSize { get; private set; }
    public int TileGridDimension { get; private set; }
    public Vector2 TileGridCenterXZ { get; private set; }
    public Tile[] TileGrid { get; private set; }

    public ComputeBuffer WindBuffer { get; private set; }

    private Texture2D _heightMap;
    private Texture2D _typeMap;
    private float _tileHeightMult;
    private Vector3[] _tileVerts;
    private Vector2 _botLeftCorner;


    private float[] _windValues;
    private ComputeShader _windShader;
    public TileData(Vector2 tileGridCenterXZ, int tileGridDimension, float tileSize, Texture2D heightMap,float tileHeightMult , Texture2D typeMap)
    {
      
        TileSize = tileSize;  
        TileGridDimension = tileGridDimension;
        TileGridCenterXZ = tileGridCenterXZ;
        _heightMap = heightMap;
        _typeMap = typeMap;
        _tileHeightMult = tileHeightMult;
    }
    public void ConstructTileGrid()
    {
        Tile[] tiles = new Tile[TileGridDimension * TileGridDimension];
        float offset = -TileGridDimension * TileSize / 2 + TileSize / 2;
        _botLeftCorner = TileGridCenterXZ + new Vector2(offset, offset);

        for (int x = 0; x < TileGridDimension; x++)
        {
            for (int y = 0; y < TileGridDimension; y++)
            {
                Vector2 tilePos = _botLeftCorner + new Vector2(TileSize * x, TileSize * y);
                float height = _heightMap ? _heightMap.GetPixel(x, y).r * _tileHeightMult : 0;
                float type = _typeMap ? _typeMap.GetPixel(x, y).r : 1;
                tiles[x * TileGridDimension + y] = new Tile(TileSize, height, tilePos, type);
            }
        }
        TileGrid = tiles;
    }
    public Vector3[] GetTileVerts() 
    {
        if (TileGrid == null)
            return null;
        _tileVerts = new Vector3[TileGridDimension * TileGridDimension * 4];
        int i = 0;
        foreach (Tile t in TileGrid) 
        {
            Vector3[] corners = t.GetTileCorners();
            for (int j = 0;j < 4;j++)
                _tileVerts[i + j] = corners[j];
             i+=4;
        }
        return _tileVerts;
    }
    public float[] GetTileType() => TileGrid?.Select(t => t.GetTileType()).ToArray();

    public void InitializeWind() 
    {
        _windShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS_GlobalWind"));
        _windValues = new float[TileGridDimension * TileGridDimension];
        WindBuffer = new ComputeBuffer(TileGridDimension * TileGridDimension, sizeof(float));
        WindBuffer.SetData(_windValues);
        _windShader.SetInt("_MaxCount", _windValues.Length);
        _windShader.SetFloat("_TileSize", TileSize);
        _windShader.SetFloat("_TileDimension", TileGridDimension);
        _windShader.SetFloat("_CornerX",_botLeftCorner.x);
        _windShader.SetFloat("_CornerY",_botLeftCorner.y);
        _windShader.SetBuffer(0,"_WindBuffer",WindBuffer);
    }

    public void UpdateWind() 
    {
        _windShader.SetFloat("_Time", Time.time);
        _windShader.Dispatch(0, Mathf.CeilToInt(WindBuffer.count / 1024f), 1, 1);
    }

    public void ReleaseBuffer() 
    {
        WindBuffer?.Dispose();
    }
}

public class Tile 
{
    private float _tileSize;
    private float _tileHeight;
    private Vector2 _tilePosition;
    private float _tileType;
    public float GetTileType() => _tileType;

    public Tile(float tileSize, float tileHeight,Vector2 tilePosition, float tileType) 
    {
        _tileSize = tileSize;
        _tilePosition = tilePosition;
        _tileHeight = tileHeight;
        _tileType = tileType;
    }

    public Vector4 GetTilePosSize() 
    {
        return new Vector4(_tilePosition.x, _tileHeight, _tilePosition.y, _tileSize);
    }
    public Vector3[] GetTileCorners() 
    {
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(_tilePosition.x - _tileSize / 2, _tileHeight, _tilePosition.y - _tileSize / 2);
        corners[1] = new Vector3(_tilePosition.x + _tileSize / 2, _tileHeight, _tilePosition.y - _tileSize / 2);
        corners[2] = new Vector3(_tilePosition.x + _tileSize / 2, _tileHeight, _tilePosition.y + _tileSize / 2);
        corners[3] = new Vector3(_tilePosition.x - _tileSize / 2, _tileHeight, _tilePosition.y + _tileSize / 2);
        return corners;
    }


}
