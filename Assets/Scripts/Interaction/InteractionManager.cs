using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class InteractionManager : MonoBehaviour
{
    public RenderTexture InteractionRT;

    public void OnEnable()
    {
        TileChunkDispatcher.OnRequestInteractionTexture += GetInteractionBuffer;
    }

    private void OnDisable()
    {
        TileChunkDispatcher.OnRequestInteractionTexture -= GetInteractionBuffer;
    }

    public RenderTexture GetInteractionBuffer() 
    {
        return InteractionRT;
    }

  
}
