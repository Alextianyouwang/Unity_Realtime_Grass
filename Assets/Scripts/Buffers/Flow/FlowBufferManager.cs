using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class FlowBufferManager : BufferPool
{
    private void OnEnable()
    {
        Initialize("CS/CS_Flow", "_FlowBuffer", sizeof(float) * 4);
        OnBufferCreated += SetInitialParameters;


        TileGrandCluster.OnRequestFlowBuffer += CreateBuffer;
        TileGrandCluster.OnRequestDisposeFlowBuffer += DisposeBuffer;
    }


    private void OnDisable()
    {

        TileGrandCluster.OnRequestFlowBuffer -= CreateBuffer;
        TileGrandCluster.OnRequestDisposeFlowBuffer -= DisposeBuffer;

        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.Buffer.Dispose();
        _dataTracker.Clear();
    }
    private void OnValidate()
    {

    }

    private void LateUpdate()
    {

        UpdateBuffer();
    }

    private void SetInitialParameters()
    {

    }

}
