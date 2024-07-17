using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class WindBufferManager : BufferPool
{

    [Header("Unit : M/s")]
    public float WindSpeed = 10f;

    [Range (-1,1)]
    public float WindDirectionX = 1;
    [Range(-1, 1)]
    public float WindDirectionY = 1;
    [Space]
    public float WindAmplitudeMultiplier = 1.0f;
    public float WindFrequencyMultiplier = 0.05f;

    [Header("Layering Parameters")]
    [Range(1, 7)]
    public int WindOcatves = 4;
    public float WindSpeedBuildup = 0.5f;
    public float WindAmplitudeFalloff = 0.5f;
    public float WindFrequencyBuildup = 1.5f;
    private void OnEnable()
    {
        Initialize("CS/CS_GlobalWind", "_WindBuffer", sizeof(float) * 4);
        OnBufferCreated += SetInitialParameters;
        TileGrandCluster.OnRequestWindBuffer += CreateBuffer;
        TileGrandCluster.OnRequestDisposeWindBuffer += DisposeBuffer;
    }
    private void OnDisable()
    {
        TileGrandCluster.OnRequestWindBuffer -= CreateBuffer;
        TileGrandCluster.OnRequestDisposeWindBuffer -= DisposeBuffer;

        foreach (DataPerTileCluster d in _dataTracker.Values)
            d.Buffer.Dispose();
        _dataTracker.Clear();
    }

    private void OnValidate()
    {
        SetInitialParameters();
    }
    private void SetInitialParameters() 
    {
        ComputeSetFloat("_Frequency", WindFrequencyMultiplier);
        ComputeSetFloat("_FrequencyBuildup", WindFrequencyBuildup);
        ComputeSetFloat("_Speed", WindSpeed);
        ComputeSetFloat("_SpeedBuildup", WindSpeedBuildup);
        ComputeSetFloat("_Amplitude", WindAmplitudeMultiplier);
        ComputeSetFloat("_AmplitudeFalloff", WindAmplitudeFalloff);
        ComputeSetInt("_Octaves", WindOcatves);
        ComputeSetFloat("_DirectionX", WindDirectionX);
        ComputeSetFloat("_DirectionY", WindDirectionY);

    }

    void LateUpdate()
    {
        ComputeSetFloat("_Time",Time.time);
        UpdateBuffer();
    }
   


}
