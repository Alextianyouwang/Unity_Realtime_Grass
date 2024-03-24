using UnityEngine;
[ExecuteInEditMode]
public class BezierController : MonoBehaviour
{
    public Material BeizerMat;

    public Transform P0;
    public Transform P1;
    public Transform P2;
    public Transform P3;
    
    void Update()
    {
        if (BeizerMat == null)
            return;
        BeizerMat.SetVector("_P0", P0 == null ? Vector3.zero : P0.position);
        BeizerMat.SetVector("_P1", P1 == null ? Vector3.zero : P1.position);
        BeizerMat.SetVector("_P2", P2 == null ? Vector3.zero : P2.position);
        BeizerMat.SetVector("_P3", P3 == null ? Vector3.zero : P3.position);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(P0 == null ? Vector3.zero : P0.position, P1 == null ? Vector3.zero : P1.position);
        Gizmos.DrawLine(P2 == null ? Vector3.zero : P2.position, P3 == null ? Vector3.zero : P3.position);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(P0 == null ? Vector3.zero : P0.position, 0.05f);
        Gizmos.DrawSphere(P1 == null ? Vector3.zero : P1.position, 0.05f);
        Gizmos.DrawSphere(P2 == null ? Vector3.zero : P2.position, 0.05f);
        Gizmos.DrawSphere(P3 == null ? Vector3.zero : P3.position, 0.05f);
        
    }
}
