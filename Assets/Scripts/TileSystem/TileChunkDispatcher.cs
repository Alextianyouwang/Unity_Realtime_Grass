using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TileChunkDispatcher
{
    private TileChunk[] _chunks;
    private TileData _tileData;

    private ComputeShader _spawnOnTileShader;
    private ComputeShader _cullShader;
    private ComputeBuffer _rawSpawnBuffer; 
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _argsBuffer;

    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _randerCam;

    struct SpawnData
    {
        float3 positionWS;
    };

    private Vector4[] _noises;
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
        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 3, ComputeBufferType.Append);
        _rawSpawnBuffer.SetCounterValue(0);
        _rawSpawnBuffer.SetData(_spawnData);

        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _args = new uint[] {
            _spawnMesh.GetIndexCount(0),
            (uint)(_tileCount * instancePerTile),
            _spawnMesh.GetIndexStart(0),
            _spawnMesh.GetBaseVertex(0),
            0
        };
        _argsBuffer.SetData(_args);

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
        _chunks = new TileChunk[1];
        _chunks[0] = new TileChunk(_spawnMesh,_spawnMeshMaterial, _tileData,_spawnSubivisions,_randerCam,_smoothPlacement, new Vector2(0, 0),_rawSpawnBuffer,_argsBuffer);
    }

    public void DispatchTileChunksDrawCall() 
    {
        _chunks[0].DrawIndirect();
    }


    public void ReleaseBuffer()
    {
        _vertBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _rawSpawnBuffer?.Dispose();
    }
}

