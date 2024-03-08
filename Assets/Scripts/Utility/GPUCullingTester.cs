using UnityEngine;

[ExecuteInEditMode]
public class GPUCullingTester : MonoBehaviour
{
    private ComputeShader Culler;
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
    private int[] _voteResult;
    private int[] _scanResult;
    private Vector3[] _cullResult;
    private uint[] _args;



    public bool UseCulling = false;
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
        _voteResult = null;
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
        _spawnPos = VolumeSpawner.Volumes;
        int scanBufferLength = CeilToNearestPowerOf2(_spawnPos.Length);
        _voteResult = new int[_spawnPos.Length];
        _scanResult = new int[scanBufferLength];
        _cullResult = new Vector3[_spawnPos.Length];

        
        _posBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(float) * 3);
        _voteBuffer = new ComputeBuffer(_spawnPos.Length,sizeof (uint));
        _scanBuffer = new ComputeBuffer(scanBufferLength,sizeof (uint));
        _compactBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(float) * 3);
        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);

        _argsBuffer_static = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer_static.SetData(_args);

        _posBuffer.SetData(_spawnPos);

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
        if (_posBuffer == null)
            return;
        if (_voteBuffer == null)
            return;
        if (Camera == null)
            return;
        Matrix4x4 V = Camera.transform.worldToLocalMatrix;
        Matrix4x4 P = Camera.projectionMatrix;
        Matrix4x4 VP = P * V;


        Culler.SetMatrix("_Camera_VP", VP);

        Culler.Dispatch(3, 1, 1, 1);
        Culler.Dispatch(0, Mathf.CeilToInt(_spawnPos.Length / 128f), 1, 1);
        Culler.Dispatch(1, 1, 1, 1);
        Culler.Dispatch(2, 1, 1, 1);

        if(UseCulling)
            RenderMaterial.SetBuffer("_SpawnBuffer", _compactBuffer);
        else
            RenderMaterial.SetBuffer("_SpawnBuffer", _posBuffer);

        _voteBuffer.GetData(_voteResult);
        _scanBuffer.GetData(_scanResult);
        _compactBuffer.GetData(_cullResult);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        Graphics.DrawMeshInstancedIndirect(TestMesh, 0, RenderMaterial, bounds,
            UseCulling? _argsBuffer :_argsBuffer_static);

        /*string indexs = "";
        foreach (int i in _scanResult)
            indexs += i.ToString() + "/";
        print(indexs);*/

    }


}
