using UnityEngine;

[ExecuteInEditMode]
public class PlaneManager : MonoBehaviour
{
    private MeshPlaneGenerator _meshGenerator;
    public int numChunk = 10;
    public Vector2 size = Vector2.one * 10;

    public float[,] heightMap;
    private void OnEnable()
    {
        _meshGenerator = new MeshPlaneGenerator();
        Mesh m = _meshGenerator.PlaneMesh(numChunk + 1, size, TryGetHeightMap(numChunk + 1));
        GetComponent<MeshFilter>().mesh = m;
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
