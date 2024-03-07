using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class PlaneManager : MonoBehaviour
{
    private MeshPlaneGenerator _meshGenerator;
    public int numChunk = 10;
    public Vector2 size = Vector2.one * 10;
    public float[,] heightMap;
    public Mesh _mesh;
    private void OnEnable()
    {
        _meshGenerator = new MeshPlaneGenerator();

        _mesh = _meshGenerator.PlaneMesh(numChunk + 1, size, TryGetHeightMap(numChunk + 1));
        _mesh.name = $"Procedural Grid {numChunk} * {numChunk}";
        GetComponent<MeshFilter>().mesh = _mesh;
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
