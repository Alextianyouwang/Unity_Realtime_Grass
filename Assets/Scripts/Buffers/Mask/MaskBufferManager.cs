using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class MaskBufferManager : BufferPool
{
    [Range(0, 1)]
    public float Threshold = 0.5f;

    [Range(1, 20)]
    public int DownSamplingScale = 1;

    [Range(0, 100)]
    public int Seed = 1;

    public Vector3 Center;
    public float Radius;
    public float Falloff;
    private void OnEnable()
    {
        Initialize("CS/CS_FoliageMask", "_MaskBuffer",sizeof(float)*4);
        OnBufferCreated += SetInitialParameters;

        TileGrandCluster.OnRequestMaskBuffer += CreateBuffer;
        TileGrandCluster.OnRequestDisposeMaskBuffer += DisposeBuffer;

    }

    private void OnDisable()
    {

        TileGrandCluster.OnRequestMaskBuffer -= CreateBuffer;
        TileGrandCluster.OnRequestDisposeMaskBuffer -= DisposeBuffer;

        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.Buffer.Dispose();
        _dataTracker.Clear();
    }

    private void SetInitialParameters() 
    {
        ComputeSetFloat("_Time", Seed);
        ComputeSetFloat("_Step", Threshold);
        ComputeSetFloat("_Radius", Radius);
        ComputeSetFloat("_Falloff", Falloff);
        ComputeSetVector("_Center", Center);
        ComputeSetInt("_DownSamplingScale", DownSamplingScale);
    }

    private void LateUpdate()
    {
        SetInitialParameters();
        UpdateBuffer();
    }
}
