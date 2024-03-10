using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class PlaneManager : MonoBehaviour
{
    private MeshPlaneGenerator _meshGenerator;
    public int numChunk = 10;
    public Vector2 size = Vector2.one * 10;
    public float[,] heightMap;
    public Mesh PlaneMesh;
    private void OnEnable()
    {
        _meshGenerator = new MeshPlaneGenerator();

        PlaneMesh = _meshGenerator.PlaneMesh(numChunk + 1, size, TryGetHeightMap(numChunk + 1));
        PlaneMesh.name = $"Procedural Grid {numChunk} * {numChunk}";
        GetComponent<MeshFilter>().mesh = PlaneMesh;
    }

    float[,] TryGetHeightMap(int numVert) 
    {
        HeightMapManager heightMap = GetComponent<HeightMapManager>();
        if (heightMap != null)
            return heightMap.PerlinMap(numVert);
        else
            return new float[numVert, numVert];
    }


}
