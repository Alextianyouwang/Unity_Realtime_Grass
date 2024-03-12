using Unity.Mathematics;
using UnityEngine;

public class TileFunctions 
{
    private TileData _tileData;

    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _randerCam;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _noiseBuffer;
    private ComputeBuffer _argsBuffer;

    private Vector4[] _noises; 
    private uint[] _args;
    private SpawnData[] _spawnData;
    private int _tileCount;

    private bool _smoothPlacement;
    private int _spawnSubivisions;

    struct SpawnData
    {
        float3 positionWS;
    };

    public TileFunctions(Mesh _mesh, Material _mat, TileData _t, int _div, Camera _cam,bool _smooth) 
    {
        _spawnMesh = _mesh;
        _spawnMeshMaterial = _mat;
        _tileData = _t;
       _spawnSubivisions = _div;
        _randerCam = _cam;
        _smoothPlacement = _smooth;
    }

    public void SetupTileCompute()
    {
        _spawnOnTileShader = (ComputeShader)Resources.Load("CS_SpawnOnTile");
        if (
           _spawnMesh == null
           || _spawnMeshMaterial == null
           )
            return;
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _spawnSubivisions * _spawnSubivisions;
        _vertBuffer = new ComputeBuffer(_tileCount * 4, sizeof(float) * 3);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _spawnData = new SpawnData[_tileCount * instancePerTile];
        _spawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 3,ComputeBufferType.Append);
        _spawnBuffer.SetCounterValue(0);
        _spawnBuffer.SetData(_spawnData);

        _noises = new Vector4[_tileCount];
        _noiseBuffer = new ComputeBuffer(_tileCount, sizeof(float) * 4);
        _noiseBuffer.SetData(_noises);

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
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        _spawnOnTileShader.SetBuffer(0, "_NoiseBuffer", _noiseBuffer);

        _spawnOnTileShader.SetBuffer(0, "_ArgsBuffer", _argsBuffer);
        _spawnOnTileShader.SetBuffer(1, "_ArgsBuffer", _argsBuffer);
    }

    public void DrawIndirect()
    {
        if (
            _spawnMesh == null
            || _spawnMeshMaterial == null
            || _argsBuffer == null
            )
            return;
        _spawnBuffer.SetCounterValue(0);
        _spawnOnTileShader.Dispatch(1, 1, 1, 1);
        _spawnOnTileShader.SetMatrix("_Camera_V", _randerCam.transform.worldToLocalMatrix);
        _spawnOnTileShader.SetMatrix("_Camera_P", _randerCam.projectionMatrix);
        _spawnOnTileShader.SetFloat("_Time", Time.time);

        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);
        _spawnMeshMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer);



        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 256f);
        Graphics.DrawMeshInstancedIndirect(_spawnMesh, 0, _spawnMeshMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);


    }

    public ComputeBuffer ShareNoiseBuffer() 
    {
        return _noiseBuffer;
    }

    public void ReleaseBuffer() 
    {
        _vertBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _spawnBuffer?.Dispose();
        _noiseBuffer?.Dispose();
    }
}
