
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GPUCullingTester : MonoBehaviour
{
    private ComputeShader Culler;
    public GameObject[] TestObjects;
    private ComputeBuffer _posBuffer;
    private ComputeBuffer _voteBuffer;
    private ComputeBuffer _scanBuffer;
    private Vector3[] _spawnPos;
    private int[] _voteResult;
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
        _voteResult = null;
    }

    int CeilToNearestPowerOf2(int value) 
    {
        int remainder = value % 2;
        if (remainder != 0)
            return (value - remainder) * 2;
        else return value;
    }
    private void SetUp() 
    {
        if (TestObjects == null)
            return;
        if (TestObjects.Length == 0)
            return;
        _spawnPos = TestObjects.Select(n => n.transform.position).ToArray();
        int scanBufferLenght = CeilToNearestPowerOf2(_spawnPos.Length);
        _voteResult = new int[_spawnPos.Length];
        _scanResult = new int[scanBufferLenght];

        
        _posBuffer = new ComputeBuffer(_spawnPos.Length, sizeof(float) * 3);
        _voteBuffer = new ComputeBuffer(_spawnPos.Length,sizeof (uint));
        _scanBuffer = new ComputeBuffer(scanBufferLenght,sizeof (uint));
  
        _posBuffer.SetData(_spawnPos);
        Culler.SetInt("_InstanceCount", _spawnPos.Length);

        Culler.SetBuffer(0, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(0, "_VoteBuffer", _voteBuffer);


        Culler.SetBuffer(1, "_ScanBuffer", _scanBuffer);
        Culler.SetBuffer(1, "_VoteBuffer", _voteBuffer);

    }
    private void Update()
    {
        if (Culler == null)
            return;
        if (_posBuffer == null)
            return;
        if (_voteBuffer == null)
            return;
        Matrix4x4 V = Camera.main.transform.worldToLocalMatrix;
        Matrix4x4 P = Camera.main.projectionMatrix;
        Matrix4x4 VP = P * V;
        Culler.SetMatrix("_Camera_VP", VP);
        Culler.Dispatch(0, 1, 1, 1);
        Culler.Dispatch(1, 1, 1, 1);
        _voteBuffer.GetData(_voteResult);
        _scanBuffer.GetData(_scanResult);

        string indexs = "";
        foreach (int i in _scanResult)
            indexs += i.ToString() + "/";
        print(indexs);
    }
    private void OnDrawGizmos()
    {
        if (_voteResult == null)
            return;
        if (_voteResult.Length == 0)
            return;
        for (int i = 0; i< _voteResult.Length; i++)
        {
            Gizmos.color = _voteResult[i] == 1 ? Color.green : Color.red;
            Gizmos.DrawSphere(_spawnPos[i], 0.1f);
        }
    }

}
