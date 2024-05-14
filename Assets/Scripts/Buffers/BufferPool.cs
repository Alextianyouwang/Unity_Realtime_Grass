using System;
using System.Collections.Generic;
using UnityEngine;

public struct DataPerTileCluster
{
    public ComputeShader WindCompute;
    public ComputeBuffer WindBuffer;
    public int Count;

}
public abstract class BufferPool : MonoBehaviour
{
    protected Dictionary<int, DataPerTileCluster> _dataTracker = new Dictionary<int, DataPerTileCluster>();

    protected string _shaderName;
    protected string _bufferName;

    protected Action OnBufferCreated;
    protected void Initialize(string shaderName,string bufferName) 
    {
        _shaderName = shaderName;
        _bufferName = bufferName; 
    }

    public DataPerTileCluster InitializeWindData(ComputeBuffer windBuffer, ComputeShader windCompute, int count)
    {
        return new DataPerTileCluster
        {
            WindCompute = windCompute,
            WindBuffer = windBuffer,
            Count = count
        };
    }

    public ComputeBuffer CreateBuffer(int hash, int tileGridDimension, float tileSize, Vector2 gridBotLeftCorner)
    {
        int count = tileGridDimension * tileGridDimension;
        ComputeShader compute = Instantiate((ComputeShader)Resources.Load(_shaderName));
        ComputeBuffer buffer = new ComputeBuffer(count, sizeof(float) * 3);

        compute.SetInt("_MaxCount", count);
        compute.SetFloat("_TileSize", tileSize);
        compute.SetFloat("_TileDimension", tileGridDimension);
        compute.SetFloat("_CornerX", gridBotLeftCorner.x);
        compute.SetFloat("_CornerY", gridBotLeftCorner.y);

        compute.SetBuffer(0, _bufferName, buffer);
        _dataTracker.Add(hash, InitializeWindData(buffer, compute, count));

        OnBufferCreated?.Invoke();
        return buffer;
    }
    public void DisposeBuffer(int hash)
    {
        if (_dataTracker.ContainsKey(hash))
        {
            DataPerTileCluster d = _dataTracker[hash];
            d.WindBuffer.Dispose();
            d.WindBuffer = null;
            d.WindCompute = null;
            _dataTracker.Remove(hash);
        }

    }
    public void UpdateBuffer()
    {
        if (_dataTracker == null)
            return;
        if (_dataTracker.Count == 0)
            return;
        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.WindCompute.Dispatch(0, Mathf.CeilToInt(d.Count / 1024f), 1, 1);
    }
    protected void ComputeSetFloat(string name, float value)
    {
        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.WindCompute.SetFloat(name, value);
    }
    protected void ComputeSetInt(string name, int value)
    {
        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.WindCompute.SetInt(name, value);
    }

}
