using UnityEngine;
public class GrassInteractor : MonoBehaviour
{
    private GameObject _interactionPainter;
    private GameObject _interactionPainter_instance;
    [Header ("Unit : Meter")]
    public float InteractorDiameter = 2f;
    public LayerMask GrasslandLayer;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        _interactionPainter_instance.transform.position = transform.position + Vector3.down * 100f;
        _interactionPainter_instance.SetActive(IsTouchingGrass());
    }
    private bool IsTouchingGrass() 
    {
        float rayLength = InteractorDiameter / 2 + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down * rayLength, rayLength, GrasslandLayer);
    }
    private void Initialize() 
    {
        _interactionPainter = (GameObject)Resources.Load("Prefab/P_InteractionPainter");

        if (_interactionPainter == null)
            return;
        _interactionPainter_instance = Instantiate(_interactionPainter, transform);
        _interactionPainter_instance.transform.localScale = new Vector3(InteractorDiameter,InteractorDiameter,InteractorDiameter);
        _interactionPainter.SetActive(true);
    }
}
