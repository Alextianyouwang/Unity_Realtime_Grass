using UnityEngine;
[ExecuteInEditMode]
public class WindSimTest : MonoBehaviour
{

    private ComputeShader _windSimCompute;
    public RenderTexture NState;
    public RenderTexture Nm1State;
    public RenderTexture Np1State;
    public Material DisplayMaterial;
    
    public int Resolusion = 256;
    public float Attenuation = 1;
    public Vector3 Center = new Vector3 (0,0,1f);

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

    }

    void FixedUpdate()
    {
        if (DisplayMaterial == null)
            return;
        Graphics.CopyTexture(NState,Nm1State);
        Graphics.CopyTexture(Np1State, NState);
        _windSimCompute.SetVector("_Center", Center);
        _windSimCompute.SetFloat("_Atten", Attenuation);
        _windSimCompute.Dispatch(0, Mathf.CeilToInt(Resolusion / 8f), Mathf.CeilToInt(Resolusion / 8f), 1);

        DisplayMaterial.SetTexture("_BaseMap", NState);
    }
}
