using UnityEngine;

[ExecuteInEditMode]
public class PlaneManager : MonoBehaviour
{
    private PlaneMeshGenerator _meshGenerator;
    public int numVertX = 10;
    public int numVertY = 10;
    public Vector2 size = Vector2.one * 10;
    private void OnEnable()
    {
        _meshGenerator = new PlaneMeshGenerator();
        Mesh m = _meshGenerator.PlaneMesh(numVertX, numVertY, size);
        GetComponent<MeshFilter>().mesh = m;
    }

    private void OnDrawGizmos()
    {
        if (_meshGenerator == null)
            return;
        foreach (Vector3 v in _meshGenerator.vertices) 
        {
            Gizmos.DrawSphere(v, 0.1f);
        }
    }


}
