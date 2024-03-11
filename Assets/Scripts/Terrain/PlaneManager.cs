using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class PlaneManager : MonoBehaviour
{
    private MeshPlaneGenerator _meshGenerator;
    public int NumChunk = 5;
    public int QuadPerChunk = 10;
    public Vector2 size = Vector2.one * 10;
    public float[,] heightMap;
    public Mesh PlaneMesh;
    private void OnEnable()
    {
        _meshGenerator = new MeshPlaneGenerator();
        int totalChunk = QuadPerChunk * NumChunk;
        PlaneMesh = _meshGenerator.PlaneMesh(totalChunk + 1, size, TryGetHeightMap(totalChunk + 1));
        PlaneMesh.name = $"Procedural Grid {totalChunk} * {totalChunk}";
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
