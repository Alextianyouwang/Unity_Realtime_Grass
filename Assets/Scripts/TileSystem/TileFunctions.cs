using UnityEngine;

public class TileFunctions 
{
    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private int _spawnSubivisions;
    private TileData _tileData;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _argsBuffer;
    private uint[] _args;
    private Vector3[] _spawnPos;

    public TileFunctions(Mesh _mesh, Material _mat, TileData _t, int _div) 
    {
        _spawnMesh = _mesh;
        _spawnMeshMaterial = _mat;
        _tileData = _t;
       _spawnSubivisions = _div;
    }

    public void SetupTileCompute()
    {
        _spawnOnTileShader = (ComputeShader)Resources.Load("CS_SpawnOnTile");
        if (
           _spawnMesh == null
           || _spawnMeshMaterial == null
           )
            return;
        int tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = _spawnSubivisions * _spawnSubivisions;
        _vertBuffer = new ComputeBuffer(tileCount * 4, sizeof(float) * 3);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _spawnPos = new Vector3[tileCount * instancePerTile];
        _spawnBuffer = new ComputeBuffer(tileCount * instancePerTile, sizeof(float) * 3,ComputeBufferType.Append);
        _spawnBuffer.SetCounterValue(0);
        _spawnBuffer.SetData(_spawnPos);

        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _args = new uint[] {
            _spawnMesh.GetIndexCount(0),
            (uint)(tileCount * instancePerTile),
            _spawnMesh.GetIndexStart(0),
            _spawnMesh.GetBaseVertex(0),
            0
        };
        _argsBuffer.SetData(_args);

        _spawnOnTileShader.SetInt("_NumTiles", tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", _spawnSubivisions);
        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _vertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(tileCount / 128f), 1, 1);
        _spawnMeshMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer);

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
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(_spawnMesh, 0, _spawnMeshMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
    }

    public void ReleaseBuffer() 
    {
        _vertBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _spawnBuffer?.Dispose();
    }
}
