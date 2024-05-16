using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class TileComponent : ScriptableObject
{
    protected TileData _tileData;
    protected Action<int> OnUpdate;
    protected Action<int> OnInitialize;
    protected Action<int> OnDispose;
    public bool Enabled = true;

    protected ComputeBuffer _windBuffer_external;
    protected ComputeBuffer _maskBuffer_external;


    public void Initialization(TileData tileData , ComputeBuffer windBuffer, ComputeBuffer maskBuffer, int hash) 
    {

        _tileData = tileData;
        _windBuffer_external = windBuffer;
        _maskBuffer_external = maskBuffer;
        OnInitialize?.Invoke(hash);
    }

    public void UpdateEffect(int hash) 
    {
        if (!Enabled)
            return;
        OnUpdate?.Invoke(hash);
    }

    public void DisposeEffect(int hash) 
    {
        OnDispose?.Invoke(hash);
    }
    
}
