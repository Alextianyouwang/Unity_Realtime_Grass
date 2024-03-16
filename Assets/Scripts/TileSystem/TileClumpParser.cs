
using UnityEngine;

public class TileClumpParser
{
    private ComputeBuffer _rawSpawnBuffer;
    private ComputeBuffer _clumpEnabledSpawnBuffer;
    private ComputeBuffer _clumpCenterBuffer;
    private ComputeShader _clumpShader;

    private Vector2[] _clumpCenters;
    private int _numTilePerSide;
    private float _tileSize;
    private int _numTilePerClumpSide;
    private Vector2 _botLeftCorner;

    struct SpawnData
    {
        Vector3 positionWS;
        float hash;
        Vector4 clumpInfo;
    };
    private SpawnData[] _clumpedSpawnData;
    public TileClumpParser(ComputeBuffer rawSpawnBuffer, int numTilePerClumpSide, int numTilePerSide, float tileSize ,Vector2 botLeftCorner) 
    {
        _rawSpawnBuffer = rawSpawnBuffer;
        _numTilePerClumpSide = numTilePerClumpSide;
        _numTilePerSide = numTilePerSide;
        _tileSize = tileSize;
        _botLeftCorner = botLeftCorner;
    }

    public void ParseClump() 
    {
        _clumpShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS_ClumpVoronoi"));
        int clumpPerSide = Mathf.CeilToInt (_numTilePerSide / _numTilePerClumpSide) ;
        _clumpCenters = new Vector2[clumpPerSide * clumpPerSide * 9];
        _clumpCenterBuffer = new ComputeBuffer(_clumpCenters.Length * 9, sizeof(float) * 2);
        _clumpCenterBuffer.SetData(_clumpCenters);

        _clumpedSpawnData = new SpawnData[_rawSpawnBuffer.count];
        _clumpEnabledSpawnBuffer = new ComputeBuffer(_rawSpawnBuffer.count, sizeof(float) * 8);
        _clumpEnabledSpawnBuffer.SetData(_clumpedSpawnData);

        float clumpIncrement = _tileSize * _numTilePerClumpSide;
        _clumpShader.SetFloat("_ClumpIncrement", clumpIncrement);
        _clumpShader.SetFloat("_CornerX", _botLeftCorner.x);
        _clumpShader.SetFloat("_CornerY", _botLeftCorner.y);
        _clumpShader.SetInt("_ClumpMaxCount",_clumpCenters.Length);
        _clumpShader.SetInt("_ClumpPerSide",clumpPerSide);
        _clumpShader.SetInt("_InstanceMaxCount", _rawSpawnBuffer.count);


        _clumpShader.SetBuffer(0,"_ClumpCenterBuffer",_clumpCenterBuffer);
        _clumpShader.Dispatch(0, Mathf.CeilToInt(_clumpCenters.Length/ 128f), 1, 1);

        _clumpShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer);
        _clumpShader.SetBuffer(1, "_ClumpEnabledSpawnBuffer",_clumpEnabledSpawnBuffer);
        _clumpShader.SetBuffer(1, "_ClumpCenterBuffer", _clumpCenterBuffer);
        _clumpShader.Dispatch(1, Mathf.CeilToInt(_rawSpawnBuffer.count / 1024f), 1, 1);
    }

    public ComputeBuffer ShareSpawnBuffer() 
    {
        return _clumpEnabledSpawnBuffer;
    }

    public void ReleaseBuffer() 
    {
        _clumpCenterBuffer?.Release();
        _rawSpawnBuffer?.Release();
        _clumpEnabledSpawnBuffer?.Release();
    }
}
    
