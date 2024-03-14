using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TileChunk
{

    private Mesh _spawnMesh;
    private Material _spawnMeshMaterial;
    private MaterialPropertyBlock _mpb;
    private Camera _randerCam;

    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _argsBuffer;


    public TileChunk(Mesh _mesh, Material _mat,  Camera _cam, ComputeBuffer _initialBuffer, ComputeBuffer _initialArgsBuffer) 
    {
        _spawnMesh = _mesh;
        _spawnMeshMaterial = _mat;

        _randerCam = _cam;
        _spawnBuffer = _initialBuffer;
        _argsBuffer = _initialArgsBuffer;

        _mpb = new MaterialPropertyBlock();

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
        _mpb.SetBuffer("_SpawnBuffer", _spawnBuffer);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 200);
        Graphics.DrawMeshInstancedIndirect(_spawnMesh, 0, _spawnMeshMaterial,bounds, _argsBuffer,
            0, _mpb, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);


    }
    

    public void ReleaseBuffer() 
    {

        _argsBuffer?.Dispose();
        _spawnBuffer?.Dispose();
    }
}
