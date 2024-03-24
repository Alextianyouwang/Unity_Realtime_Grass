using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class WindManager :MonoBehaviour
{
    private Dictionary<int, WindDataPerTileCluster> _windDataTracker = new Dictionary<int, WindDataPerTileCluster>();

    [Header("Unit : M/s")]
    public float WindSpeed = 10f;
    public float WindFrequencyMultiplier = 0.05f;
    [Header("Unit : Degree")]
    [Range (0,360)]
    public float WindDirection = 45;
    public struct WindDataPerTileCluster 
    {
        public ComputeShader WindCompute;
        public ComputeBuffer WindBuffer;
        public float[] WindValues; 
    }
    public WindDataPerTileCluster InitializeWindData(ComputeBuffer windBuffer, ComputeShader windCompute, float[] windValues) 
    {
        return new WindDataPerTileCluster {
            WindCompute = windCompute,
            WindBuffer = windBuffer,
            WindValues = windValues
        };
    }
    private void OnEnable()
    {
        TileChunkDispatcher.OnRequestWindBuffer += SendWindBuffer;
        TileChunkDispatcher.OnRequestDisposeWindBuffer += DisposeWindBuffer;
        TileVisualizer.OnRequestWindBuffer += SendWindBuffer;
        TileVisualizer.OnRequestDisposeWindBuffer += DisposeWindBuffer;
    }
    private void OnDisable()
    {
        TileChunkDispatcher.OnRequestWindBuffer -= SendWindBuffer;
        TileChunkDispatcher.OnRequestDisposeWindBuffer -= DisposeWindBuffer;
        TileVisualizer.OnRequestWindBuffer -= SendWindBuffer;
        TileVisualizer.OnRequestDisposeWindBuffer -= DisposeWindBuffer;

        foreach (WindDataPerTileCluster d in _windDataTracker.Values)
            d.WindBuffer.Dispose();
        _windDataTracker.Clear();
    }
    public ComputeBuffer SendWindBuffer(int hash, int tileGridDimension, float tileSize, Vector2 gridBotLeftCorner)
    {
        ComputeShader windCompute = Instantiate((ComputeShader)Resources.Load("CS/CS_GlobalWind"));
        ComputeBuffer windBuffer = new ComputeBuffer(tileGridDimension * tileGridDimension, sizeof(float));

        float[] windValues = new float[tileGridDimension * tileGridDimension];
        windBuffer.SetData(windValues);
        windCompute.SetInt("_MaxCount", windValues.Length);
        windCompute.SetFloat("_TileSize", tileSize);
        windCompute.SetFloat("_TileDimension", tileGridDimension);
        windCompute.SetFloat("_CornerX", gridBotLeftCorner.x);
        windCompute.SetFloat("_CornerY", gridBotLeftCorner.y);
        windCompute.SetFloat("_Frequency", WindFrequencyMultiplier);
        windCompute.SetFloat("_Speed", WindSpeed);
        windCompute.SetFloat("_Direction", WindDirection);
        windCompute.SetBuffer(0, "_WindBuffer", windBuffer);
        _windDataTracker.Add(hash,InitializeWindData(windBuffer,windCompute,windValues));
        return windBuffer;
    }
    public void DisposeWindBuffer(int hash) 
    {
        if (_windDataTracker.ContainsKey(hash)) 
        {
            WindDataPerTileCluster d = _windDataTracker[hash];
            d.WindBuffer.Dispose();
            d.WindBuffer = null;
            d.WindCompute = null;
            d.WindValues = null;
            _windDataTracker.Remove(hash);
        }
           
    }
    void LateUpdate()
    {
        UpdateWind();
    }
    public void UpdateWind()
    {
        if (_windDataTracker == null)
            return;
        if (_windDataTracker.Count == 0)
            return;
        foreach (WindDataPerTileCluster d in _windDataTracker.Values) 
        {
            d.WindCompute.SetFloat("_Time", Time.time);
            d.WindCompute.Dispatch(0, Mathf.CeilToInt(d.WindValues.Length / 1024f), 1, 1);
        }
    }


}
