using System;
using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class WindSimTest : MonoBehaviour
{

    private ComputeShader _windSimCompute;
    public static RenderTexture NState;
    public RenderTexture Nm1State;
    public RenderTexture Np1State;
    public Material DisplayMaterial;
    
    public int Resolusion = 256;
    public float Attenuation = 1;
    public Vector3 Center = new Vector3 (0,0,0);

    private Vector2 BotLeftCorner = new Vector2 (99.80469f, 99.80469f);
    private float TileClusterSize = 200;
    public float Size = 50;
    public float Thickness = 5;


    void InitTexture(ref RenderTexture rt) 
    {
        rt = new RenderTexture(Resolusion, Resolusion, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SNorm);
        rt.filterMode = FilterMode.Point;
        rt.enableRandomWrite = true;
        rt.Create();
    }
    private void OnEnable()
    {
        Initialize();
    }
    private void OnDisable()
    {
        DisposeTexture(ref NState);
        DisposeTexture(ref Nm1State);
        DisposeTexture(ref Np1State);

    }

    void Initialize() 
    {
        InitTexture(ref NState);
        InitTexture(ref Nm1State);
        InitTexture(ref Np1State);

        _windSimCompute = (ComputeShader)Resources.Load("CS/CS_WindSimTest");
        _windSimCompute.SetTexture(0, "_NState", NState);
        _windSimCompute.SetTexture(0, "_Nm1State", Nm1State);
        _windSimCompute.SetTexture(0, "_Np1State", Np1State);
        _windSimCompute.SetInt("_ResX", Resolusion);
        _windSimCompute.SetInt("_ResY", Resolusion);


        _windSimCompute.SetFloat("_BotX", BotLeftCorner.x);
        _windSimCompute.SetFloat("_BotY", BotLeftCorner.y);
        _windSimCompute.SetFloat("_ClusterSize", TileClusterSize);

    }

    void DisposeTexture(ref RenderTexture rt) => rt.Release();

    void  LateUpdate()
    {
        if (DisplayMaterial == null)
            return;

        _windSimCompute.SetVector("_Center", Center);
        _windSimCompute.SetFloat("_Atten", Attenuation);
        _windSimCompute.SetFloat("_Size", Size);
        _windSimCompute.SetFloat("_Thickness", Thickness);

        _windSimCompute.Dispatch(0, Mathf.CeilToInt(Resolusion / 8f), Mathf.CeilToInt(Resolusion / 8f), 1);

        DisplayMaterial.SetTexture("_BaseMap", NState);
    }
}
