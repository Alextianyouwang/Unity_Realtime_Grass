using UnityEngine;

[ExecuteInEditMode]
public class GPUCullingTester : MonoBehaviour
{
    public bool UseCulling = false;
    public VolumeSpawner VolumeSpawner;
    public Camera Camera;
    public Mesh TestMesh;
    public Material RenderMaterial;

    private ComputeBuffer _posBuffer;
    private ComputeBuffer _voteBuffer;
    private ComputeBuffer _scanBuffer;
    private ComputeBuffer _compactBuffer;
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _argsBuffer_static;
    
    private Vector3[] _spawnPos;
    private uint[] _args;
    private Matrix4x4 _matrix_vp;

    private ComputeShader Culler;

    private void OnEnable()
    {
        Culler = (ComputeShader)Resources.Load("CS_GPUCulling");

        SetUp();
    }

    private void OnDisable()
    {
        _posBuffer?.Release();
        _voteBuffer?.Release();
        _scanBuffer?.Release();
        _compactBuffer?.Release();
        _argsBuffer?.Release();
        _argsBuffer_static?.Release();
    }

    int CeilToNearestPowerOf2(int value) 
    {
        int target = 2;

        while (target < value)
            target <<= 1;

        value = target;
        return value;
    }

    private void SetUp() 
    {
        if (VolumeSpawner == null)
            return;
        if (VolumeSpawner.Volumes == null)
            return;
        if (TestMesh)
        {
            _args = new uint[] {
            TestMesh.GetIndexCount(0),
            (uint)_spawnPos.Length,
            TestMesh.GetIndexStart(0),
            TestMesh.GetBaseVertex(0),
            0
            };
        }
        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;
        _spawnPos = VolumeSpawner.Volumes;
        int scanBufferLength = CeilToNearestPowerOf2(_spawnPos.Length);

        /* Procedural */
        _voteBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(uint));
        /* Procedural */
        _scanBuffer = new ComputeBuffer(scanBufferLength, sizeof(uint));
        /* Procedural */
        _compactBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(float) * 3);

        _posBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(float) * 3);
        _posBuffer.SetData(_spawnPos);
        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);
        _argsBuffer_static = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer_static.SetData(_args);

        Culler.SetMatrix("_Camera_VP", _matrix_vp);
        Culler.SetInt("_InstanceCount", _spawnPos.Length);

        Culler.SetBuffer(0, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(0, "_VoteBuffer", _voteBuffer);

        Culler.SetBuffer(1, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(1, "_VoteBuffer", _voteBuffer);

        Culler.SetBuffer(2, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(2, "_VoteBuffer", _voteBuffer);
        Culler.SetBuffer(2, "_CompactBuffer", _compactBuffer);
        Culler.SetBuffer(2, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(2, "_ArgsBuffer", _argsBuffer);

        Culler.SetBuffer(3, "_ArgsBuffer", _argsBuffer);

    }
    private void Update()
    {
        if (Culler == null)
            return;

        _spawnPos = VolumeSpawner.Volumes;
        _posBuffer.SetData(_spawnPos);
        Culler.SetBuffer(0, "_SpawnBuffer", _posBuffer);

        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;
        Culler.SetMatrix("_Camera_VP", _matrix_vp);

        Culler.Dispatch(3, 1, 1, 1);
        Culler.Dispatch(0, Mathf.CeilToInt(_spawnPos.Length / 128f), 1, 1);
        Culler.Dispatch(1, 1, 1, 1);
        Culler.Dispatch(2, 1, 1, 1);

        if(UseCulling)
            RenderMaterial.SetBuffer("_SpawnBuffer", _compactBuffer);
        else
            RenderMaterial.SetBuffer("_SpawnBuffer", _posBuffer);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(TestMesh, 0, RenderMaterial, bounds,
            UseCulling? _argsBuffer :_argsBuffer_static);

        /*string indexs = "";
        foreach (int i in _scanResult)
            indexs += i.ToString() + "/";
        print(indexs);*/
    }


}
