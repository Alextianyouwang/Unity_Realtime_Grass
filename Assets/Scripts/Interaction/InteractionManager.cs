using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class InteractionManager : MonoBehaviour
{
    private RenderTexture _InteractionRT;

    // Quick Approach, will update to non-repetitive texture later
    public void OnEnable()
    {
        _InteractionRT = (RenderTexture)Resources.Load("RT/RT_GrassInteractData");
        TileGrandCluster.OnRequestInteractionTexture += GetInteractionBuffer;
    }

    private void OnDisable()
    {
        TileGrandCluster.OnRequestInteractionTexture -= GetInteractionBuffer;
    }

    public RenderTexture GetInteractionBuffer() 
    {
        return _InteractionRT;
    }
}
