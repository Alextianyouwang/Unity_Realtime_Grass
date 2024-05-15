using System;
using UnityEngine;


public abstract class TileComponent : ScriptableObject
{
    protected TileData _tileData;
    protected Action OnUpdate;
    protected Action OnInitialize;
    protected Action OnDispose;
    public bool Enabled = true;
    public void Initialization(TileData tileData) 
    {

        _tileData = tileData;
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
