using Unity.Mathematics;
using UnityEngine;

public class TileChunk
{
    private TileData _tileData;

    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _randerCam;

    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _argsBuffer;



    public TileChunk(Mesh _mesh, Material _mat, TileData _t, int _div, Camera _cam,bool _smooth, Vector2 _offsets, ComputeBuffer _initialBuffer, ComputeBuffer _initialArgsBuffer) 
    {
        _spawnMesh = _mesh;
        _spawnMeshMaterial = _mat;
        _tileData = _t;

        _randerCam = _cam;
        _spawnBuffer = _initialBuffer;
        _argsBuffer = _initialArgsBuffer;

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
        _spawnMeshMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 256f);
        Graphics.DrawMeshInstancedIndirect(_spawnMesh, 0, _spawnMeshMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);


    }


    public void ReleaseBuffer() 
    {

        _argsBuffer?.Dispose();
        _spawnBuffer?.Dispose();
    }
}
