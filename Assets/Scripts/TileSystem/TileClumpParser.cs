
using UnityEngine;

public class TileClumpParser
{
    private ComputeBuffer _rawSpawnBuffer_external;
    private ComputeBuffer _clumpCenterBuffer;
    private ComputeShader _clumpShader;

    private Vector2[] _clumpCenters;
    private int _numTilePerSide;
    private float _tileSize;
    private Vector2 _botLeftCorner;
    public TileClumpParser(ComputeBuffer rawSpawnBuffer, int numTilePerSide, float tileSize ,Vector2 botLeftCorner) 
    {
        _rawSpawnBuffer_external = rawSpawnBuffer;
        _numTilePerSide = numTilePerSide;
        _tileSize = tileSize;
        _botLeftCorner = botLeftCorner;
    }

    public void ParseClump() 
    {
        _clumpShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS_ClumpVoronoi"));
        int clumpPerSide = Mathf.CeilToInt (_numTilePerSide / TileGrandCluster._TilePerClump) ;
        _clumpCenters = new Vector2[clumpPerSide * clumpPerSide * 9];
        _clumpCenterBuffer = new ComputeBuffer(_clumpCenters.Length * 9, sizeof(float) * 2);
        _clumpCenterBuffer.SetData(_clumpCenters);

        float clumpIncrement = _tileSize * TileGrandCluster._TilePerClump;
        _clumpShader.SetFloat("_ClumpIncrement", clumpIncrement);
        _clumpShader.SetFloat("_CornerX", _botLeftCorner.x);
        _clumpShader.SetFloat("_CornerY", _botLeftCorner.y);
        _clumpShader.SetInt("_ClumpMaxCount",_clumpCenters.Length);
        _clumpShader.SetInt("_ClumpPerSide",clumpPerSide);
        _clumpShader.SetInt("_InstanceMaxCount", _rawSpawnBuffer_external.count);


        _clumpShader.SetBuffer(0,"_ClumpCenterBuffer",_clumpCenterBuffer);
        _clumpShader.Dispatch(0, Mathf.CeilToInt(_clumpCenters.Length/ 128f), 1, 1);

        _clumpShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer_external);
        _clumpShader.SetBuffer(1, "_ClumpCenterBuffer", _clumpCenterBuffer);
        _clumpShader.Dispatch(1, Mathf.CeilToInt(_rawSpawnBuffer_external.count / 1024f), 1, 1);
    }

    public ComputeBuffer ShareSpawnBuffer() 
    {
        return _rawSpawnBuffer_external;
    }

    public void ReleaseBuffer() 
    {
        _clumpCenterBuffer?.Release();

    }
}
    
