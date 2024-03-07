
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class GPUCullingTester : MonoBehaviour
{
    private ComputeShader Culler;
    public GameObject[] TestObjects;
    private ComputeBuffer _posBuffer;
    private ComputeBuffer _resultBuffer;

    private CullResult[] _result;

    struct CullResult
    {
        public Vector3 pos;
        public float isVisible;
    };
    private void OnEnable()
    {
        Culler = (ComputeShader)Resources.Load("CS_GPUCulling");
        SetUp();
    }

    private void OnDisable()
    {
        _posBuffer?.Release();
        _resultBuffer?.Release();
        _result = null;
    }

    private void SetUp() 
    {
        if (TestObjects == null)
            return;
        if (TestObjects.Length == 0)
            return;
        Vector3[] positions = TestObjects.Select(n => n.transform.position).ToArray();
        _result = new CullResult[positions.Length];

        _posBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        _resultBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 4, ComputeBufferType.Append);
        _resultBuffer.SetCounterValue(0);
  
        _posBuffer.SetData(positions);
        Culler.SetInt("_InstanceCount", positions.Length);

        Culler.SetBuffer(0, "_SpawnBuffer", _posBuffer);
        Culler.SetBuffer(0, "_ResultBuffer", _resultBuffer);
  
    }
    private void Update()
    {
        if (Culler == null)
            return;
        if (_posBuffer == null)
            return;
        if (_resultBuffer == null)
            return;
        _resultBuffer.SetCounterValue(0);

        Matrix4x4 V = Camera.main.transform.worldToLocalMatrix;
        Matrix4x4 P = Camera.main.projectionMatrix;
        Matrix4x4 VP = P * V;
        Culler.SetMatrix("_Camera_VP", VP);
        Culler.Dispatch(0, 2, 1, 1);
        _resultBuffer.GetData(_result);
    }
    private void OnDrawGizmos()
    {
        if (_result == null)
            return;
        if (_result.Length == 0)
            return;
        foreach (CullResult c in _result) 
        {
            Gizmos.color = c.isVisible == 1 ? Color.green : Color.red;
            Gizmos.DrawSphere(c.pos, 0.1f);
        }
    }

}
