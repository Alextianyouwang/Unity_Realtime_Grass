using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GPUCullingTester_Advanced : MonoBehaviour
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

    private uint[] _args;
    private Matrix4x4 _matrix_vp;

    private ComputeShader Culler;
    private int _elementCount;
    private int _instances;
    private int _groupCount;
    private int[] _scanResult;

    private void OnEnable()
    {
        Culler = (ComputeShader)Resources.Load("CS/CS_GPUCulling_Advanced");

        SetUp();
    }

    struct SpawnData
    {
        Vector3 positionWS;
        float hash;
        Vector4 clumpInfo;
        Vector4 postureData;
        public SpawnData(Vector3 _posWS, float _hash, Vector4 _clumpInfo, Vector4 _postureData)
        {
            positionWS = _posWS;
            hash = _hash;
            clumpInfo = _clumpInfo;
            postureData = _postureData;
        }
    };


    private void OnDisable()
    {
        _posBuffer?.Release();
        _voteBuffer?.Release();
        _scanBuffer?.Release();
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

        _instances = VolumeSpawner.Volumes.Length;
        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;

        _elementCount = Utility.CeilToNearestPowerOf2(_instances);
        _groupCount = Utility.CeilToNearestPowerOf2(_elementCount / 128);
        _scanResult = new int[_elementCount];

        if (TestMesh)
        {
            _args = new uint[] {
            TestMesh.GetIndexCount(0),
            (uint)_instances,
            TestMesh.GetIndexStart(0),
            TestMesh.GetBaseVertex(0),
            0
            };
        }

        /* Procedural */
        _voteBuffer = new ComputeBuffer(_instances, sizeof(uint));
        /* Procedural */
        _scanBuffer = new ComputeBuffer(_instances, sizeof(uint));
        /* Procedural */
        _compactBuffer = new ComputeBuffer(_instances, sizeof(float) * 12);

        _posBuffer = new ComputeBuffer(_instances, sizeof(float) * 12);
        _posBuffer.SetData(VolumeSpawner.Volumes.Select(x => new SpawnData(x, 0, Vector4.zero, Vector4.zero)).ToArray());
        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);
        _argsBuffer_static = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _argsBuffer_static.SetData(_args);

        Culler.SetMatrix("_Camera_VP", _matrix_vp);
        Culler.SetInt("_InstanceCount", _instances);

        Culler.SetBuffer(0, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(0, "_VoteBuffer", _voteBuffer);

        Culler.SetBuffer(1, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(1, "_VoteBuffer", _voteBuffer);

        Culler.SetBuffer(2, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(2, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(2, "_VoteBuffer", _voteBuffer);
        Culler.SetBuffer(2, "_CompactBuffer",_compactBuffer);
        Culler.SetBuffer(2, "_ArgsBuffer", _argsBuffer);

        Culler.SetBuffer(3, "_ArgsBuffer", _argsBuffer);


    }
    private void Update()
    {
        if (Culler == null)
            return;

        _matrix_vp = Camera.projectionMatrix * Camera.transform.worldToLocalMatrix;
        Culler.SetMatrix("_Camera_VP", _matrix_vp);


        Culler.Dispatch(3, 1, 1, 1);
        Culler.Dispatch(0, 1, 1, 1);
        Culler.Dispatch(1, 1, 1, 1);
        Culler.Dispatch(2, 1, 1, 1);

        if (UseCulling)
            RenderMaterial.SetBuffer("_SpawnBuffer", _compactBuffer);
        else
            RenderMaterial.SetBuffer("_SpawnBuffer", _posBuffer);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(TestMesh, 0, RenderMaterial, bounds,
            UseCulling ? _argsBuffer : _argsBuffer_static);

        // Do not use large number
        /*_scanBuffer.GetData(_scanResult);
        string indexs = "";
        foreach (int i in _scanResult)
            indexs += i.ToString() + "/";
        print(indexs);*/
    }


}
