using System;
using UnityEngine;


public abstract class TileComponent : ScriptableObject
{
    protected TileData _tileData;
    protected Action OnUpdate;
    protected Action OnInitialize;
    protected Action OnDispose;
    public bool Enabled = true;

    protected ComputeBuffer _windBuffer_external;
    protected ComputeBuffer _maskBuffer_external;
    public void Initialization(TileData tileData , ComputeBuffer windBuffer, ComputeBuffer maskBuffer) 
    {

        _tileData = tileData;
        _windBuffer_external = windBuffer;
        _maskBuffer_external = maskBuffer;
        OnInitialize?.Invoke();
    }

    public void UpdateEffect() 
    {
        if (!Enabled)
            return;
        OnUpdate?.Invoke();
    }

    public void DisposeEffect() 
    {
        OnDispose?.Invoke();
    }
    
}
