
using UnityEngine;

public class PlaneMeshGenerator 
{
    public Vector3[] vertices;
    Vector2[] uvs; 
    int[] triangles;
    int triStart = 0;
    public Mesh PlaneMesh(int numVertX, int numVertY, Vector2 size) 
    {
        Mesh m = new Mesh();
        vertices = new Vector3[numVertX * numVertY];
        uvs = new Vector2[numVertX * numVertY];
        triangles = new int[(numVertX - 1) * (numVertY - 1) * 6];
        Vector3 initialPoint = new Vector3(-size.x / 2, 0,- size.y / 2);
        float xInc = size.x / (numVertX - 1);
        float yInc = size.y / (numVertY - 1);

        
        for (int x = 0; x < numVertX; x++) 
        {
            for (int y = 0; y < numVertY; y++) 
            {
                vertices[x * numVertY + y] = initialPoint + new Vector3(x * xInc, 0, y * yInc);
                uvs[x * numVertY + y] = new Vector2((float)x / (numVertX-1),(float)y / (numVertY-1));
                if (x >= numVertX - 1 || y >= numVertY - 1)
                    continue;
                FillTriangle(x * numVertY + y, numVertY);
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
        int botRight = index + 1;
        int topLeft = index + numVertY;
        int topRight = index + numVertY + 1;

        triangles[triStart] = topLeft;
        triangles[triStart + 1] = index;
        triangles[triStart + 2] = topRight;
        triangles[triStart + 3] = topRight;
        triangles[triStart + 4] = index;
        triangles[triStart + 5] = botRight;

        triStart += 6;
    }
}
