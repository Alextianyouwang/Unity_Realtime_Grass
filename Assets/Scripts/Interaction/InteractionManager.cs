using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class InteractionManager : MonoBehaviour
{
    private RenderTexture _InteractionRT;

    public void OnEnable()
    {
        _InteractionRT = (RenderTexture)Resources.Load("RT/RT_GrassInteractData");
        TileChunkDispatcher.OnRequestInteractionTexture += GetInteractionBuffer;
    }

    private void OnDisable()
    {
        TileChunkDispatcher.OnRequestInteractionTexture -= GetInteractionBuffer;
    }

    public RenderTexture GetInteractionBuffer() 
    {
        return _InteractionRT;
    }
}
