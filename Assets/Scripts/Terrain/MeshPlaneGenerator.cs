
using UnityEngine;

public class MeshPlaneGenerator 
{
    Vector3[] vertices;
    Vector2[] uvs; 
    int[] triangles;
    int triStart = 0;
    public Mesh PlaneMesh(int numVert, Vector2 size, float[,] heightMap) 
    {
        Mesh m = new Mesh();
        vertices = new Vector3[numVert * numVert];
        uvs = new Vector2[numVert * numVert];
        triangles = new int[(numVert - 1) * (numVert - 1) * 6];
        Vector3 initialPoint = new Vector3(-size.x / 2, 0,- size.y / 2);
        float inc = size.x / (numVert - 1);

        
        for (int x = 0; x < numVert; x++) 
        {
            for (int y = 0; y < numVert; y++) 
            {
                vertices[x * numVert + y] = initialPoint + new Vector3(x * inc, 0, y * inc);
                vertices[x * numVert + y].y += heightMap[x, y];
                uvs[x * numVert + y] = new Vector2((float)x / (numVert - 1),(float)y / (numVert - 1));
                if (x >= numVert - 1 || y >= numVert - 1)
                    continue;
                FillTriangle(x * numVert + y, numVert);
            }
        }
        m.vertices = vertices;
        m.triangles = triangles;
        m.uv = uvs;
        m.RecalculateBounds();
        m.RecalculateNormals();
        return m;
    }
    void FillTriangle(int index, int numVertY) 
    {
        int topLeft = index + 1;
        int botLeft = index + numVertY;
        int topRight = index + numVertY + 1;

        triangles[triStart] = botLeft;
        triangles[triStart + 1] = index;
        triangles[triStart + 2] = topRight;
        triangles[triStart + 3] = topRight;
        triangles[triStart + 4] = index;
        triangles[triStart + 5] = topLeft;

        triStart += 6;
    }
}
