using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class MaskBufferManager : BufferPool
{

    private void OnEnable()
    {
        Initialize("CS/CS_FoliageMask", "_MaskBuffer",sizeof(float)*4);
        OnBufferCreated += SetInitialParameters;

        TileGrandCluster.OnRequestMaskBuffer += CreateBuffer;
        TileGrandCluster.OnRequestDisposeMaskBuffer += DisposeBuffer;
        TileVisualizer.OnRequestMaskBuffer += CreateBuffer;
        TileVisualizer.OnRequestDisposeMaskBuffer += DisposeBuffer;
    }

    private void OnDisable()
    {

        TileGrandCluster.OnRequestMaskBuffer -= CreateBuffer;
        TileGrandCluster.OnRequestDisposeMaskBuffer -= DisposeBuffer;
        TileVisualizer.OnRequestMaskBuffer -= CreateBuffer;
        TileVisualizer.OnRequestDisposeMaskBuffer -= DisposeBuffer;
        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.Buffer.Dispose();
        _dataTracker.Clear();
    }
    private void SetInitialParameters() 
    {
    
    }

    private void LateUpdate()
    {
        ComputeSetFloat("_Time", Time.time);
        UpdateBuffer();
    }
}
