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
    private ComputeBuffer _groupScanBufferIn;
    private ComputeBuffer _groupScanBufferOut;
    private ComputeBuffer _compactBuffer;
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _argsBuffer_static;
    
    private Vector3[] _spawnPos;
    private uint[] _args;
    private Matrix4x4 _matrix_vp;

    private ComputeShader Culler;
    private int _elementCount;
    private int _groupCount;
    private int[] _scanResult;

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
        _groupScanBufferIn?.Release();
        _groupScanBufferOut?.Release();
        _compactBuffer?.Release();
        _argsBuffer?.Release();
        _argsBuffer_static?.Release();
    }
    private void SetUp() 
    {
        if (VolumeSpawner == null)
            return;
        if (VolumeSpawner.Volumes == null)
            return;
     
        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;
        _spawnPos = VolumeSpawner.Volumes;
        _elementCount = Utility.CeilToNearestPowerOf2(_spawnPos.Length);
        _groupCount = Utility.CeilToNearestPowerOf2(_elementCount / 128);
        _scanResult = new int[_elementCount];

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

        /* Procedural */
        _voteBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(uint));
        /* Procedural */
        _scanBuffer = new ComputeBuffer(_elementCount, sizeof(uint));
        /* Procedural */
        _groupScanBufferIn = new ComputeBuffer(_groupCount, sizeof(uint));
        /* Procedural */
        _groupScanBufferOut = new ComputeBuffer(_groupCount, sizeof(uint));
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
        Culler.SetBuffer(1, "_GroupScanBufferIn", _groupScanBufferIn);
        Culler.SetBuffer(1, "_VoteBuffer", _voteBuffer);

        Culler.SetBuffer(2, "_GroupScanBufferIn", _groupScanBufferIn);
        Culler.SetBuffer(2, "_GroupScanBufferOut", _groupScanBufferOut);

        Culler.SetBuffer(3, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(3, "_GroupScanBufferOut", _groupScanBufferOut);
        Culler.SetBuffer(3, "_VoteBuffer", _voteBuffer);
        Culler.SetBuffer(3, "_CompactBuffer", _compactBuffer);
        Culler.SetBuffer(3, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(3, "_ArgsBuffer", _argsBuffer);

        Culler.SetBuffer(4, "_ArgsBuffer", _argsBuffer);


    }
    private void Update()
    {
        if (Culler == null)
            return;

        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;
        Culler.SetMatrix("_Camera_VP", _matrix_vp);

        
        Culler.Dispatch(4, 1, 1, 1);
        Culler.Dispatch(0, Mathf.CeilToInt(_spawnPos.Length / 128f), 1, 1);
        Culler.Dispatch(1, _groupCount, 1, 1);
        Culler.Dispatch(2, 1, 1, 1);
        Culler.Dispatch(3, Mathf.CeilToInt(_spawnPos.Length / 128f), 1, 1);

        if(UseCulling)
            RenderMaterial.SetBuffer("_SpawnBuffer", _compactBuffer);
        else
            RenderMaterial.SetBuffer("_SpawnBuffer", _posBuffer);

        RenderMaterial.SetColor("_ChunkColor", Color.white);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(TestMesh, 0, RenderMaterial, bounds,
            UseCulling? _argsBuffer :_argsBuffer_static);

        // Do not use large number
        /*_scanBuffer.GetData(_scanResult);
        string indexs = "";
        foreach (int i in _scanResult)
            indexs += i.ToString() + "/";
        print(indexs);*/
    }


}
