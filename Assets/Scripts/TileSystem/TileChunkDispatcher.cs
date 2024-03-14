using Unity.Mathematics;
using UnityEngine;

public class TileChunkDispatcher
{
    private TileChunk[] _chunks;
    private TileData _tileData;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _rawSpawnBuffer; 
    private ComputeBuffer _vertBuffer;

    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _randerCam;

    struct SpawnData
    {
        float3 positionWS;
    };

    private uint[] _args;
    private SpawnData[] _spawnData;
    private int _tileCount;

    private bool _smoothPlacement;
    private int _spawnSubivisions;

    public TileChunkDispatcher(Mesh _mesh, Material _mat, TileData _t, int _div, Camera _cam, bool _smooth)
    {
        _spawnMesh = _mesh;
        _spawnMeshMaterial = _mat;
        _tileData = _t;
        _spawnSubivisions = _div;
        _randerCam = _cam;
        _smoothPlacement = _smooth;
    }

    public void InitialSpawn()
    {
        if (
           _spawnMesh == null
           || _spawnMeshMaterial == null
           )
            return;

        _spawnOnTileShader = (ComputeShader)Resources.Load("CS_InitialSpawn");
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _spawnSubivisions * _spawnSubivisions;
        _vertBuffer = new ComputeBuffer(_tileCount * 4, sizeof(float) * 3);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _spawnData = new SpawnData[_tileCount * instancePerTile];
        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 3);
        _rawSpawnBuffer.SetData(_spawnData);

        _spawnOnTileShader.SetInt("_NumTiles", _tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", _spawnSubivisions);
        _spawnOnTileShader.SetInt("_NumTilesPerSide", _tileData.TileGridDimension);
        _spawnOnTileShader.SetBool("_SmoothPlacement", _smoothPlacement);

        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _vertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _rawSpawnBuffer);
        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);
    }

    public void InitializeChunks() 
    {
        int chunkPerSide =
            _tileData.TileGridDimension % 4 == 0 ? 4 :
            _tileData.TileGridDimension % 3 == 0 ? 3 : 1;


        _chunks = new TileChunk[chunkPerSide * chunkPerSide];
        int chunkDimension = _tileData.TileGridDimension / chunkPerSide;
        int totalInstancePerChunk = chunkDimension * chunkDimension * _spawnSubivisions * _spawnSubivisions;
        for (int x = 0; x < chunkPerSide; x++)
        {
            for (int y = 0; y < chunkPerSide; y++) 
            {
                SpawnData[] spawnDatas = new SpawnData[totalInstancePerChunk];
                ComputeBuffer chunkBuffer = new ComputeBuffer(totalInstancePerChunk, sizeof(float) * 3);
                chunkBuffer.SetData(spawnDatas);
                _spawnOnTileShader.SetInt("_ChunkIndexX", x);
                _spawnOnTileShader.SetInt("_ChunkIndexY", y);
                _spawnOnTileShader.SetInt("_ChunkPerSide", chunkPerSide);
                _spawnOnTileShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer);
                _spawnOnTileShader.SetBuffer(1, "_ChunkSpawnBuffer", chunkBuffer);
                _spawnOnTileShader.Dispatch(1, Mathf.CeilToInt(totalInstancePerChunk / 128f), 1, 1);

                _chunks[x * chunkPerSide + y] = new TileChunk(_spawnMesh, _spawnMeshMaterial, _randerCam, chunkBuffer);
                _chunks[x * chunkPerSide + y].Setup();
            }
        }
    }

    public void DispatchTileChunksDrawCall() 
    {
        foreach (TileChunk t in _chunks)
            t?.DrawIndirect();

    }


    public void ReleaseBuffer()
    {
        _vertBuffer?.Dispose();
        _rawSpawnBuffer?.Dispose();
        foreach (TileChunk t in _chunks)
            t?.ReleaseBuffer();
    }
}

