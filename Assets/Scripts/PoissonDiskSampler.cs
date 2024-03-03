using System.Collections.Generic;
using UnityEngine;
public static class PoissonDiskSampler 
{
    static int[] grids;
    static List<Vector2> allPoints;
    static List<Vector2> activePoints;

    public static List<Vector2> PoissonSamples(float r, int k , Vector2 area) 
    {
        activePoints = new List<Vector2>();
        allPoints = new List<Vector2>();
        int N = 2;
        float gridSize = r / Mathf.Sqrt(N);
        int numGridX = Mathf.CeilToInt(area.x / gridSize) + 1;
        int numGridY = Mathf.CeilToInt(area.y / gridSize) + 1;

        grids = new int[numGridX* numGridY];
        for (int x = 0; x < numGridX; x++)
            for (int y = 0; y < numGridY; y++)
                grids[x * numGridY +  y] = -1;

        Vector2 p0 = GetRandomPoint(area);
        activePoints.Add(p0);
        InsertPointToGrid(p0, gridSize, numGridY);

        while (activePoints.Count > 0) 
        {
            bool found = false;

            Vector2 sample = activePoints[Random.Range (0, activePoints.Count - 1)];

            for (int t = 0; t < k; t++) 
            {
                float theta = Random.Range(0, Mathf.PI * 2);
                float radius = Random.Range(r, 2 * r);
                float posX = sample.x  + Mathf.Cos (theta) * radius;
                float posY = sample.y  + Mathf.Sin (theta) * radius;
                Vector2 newPoint = new Vector2(posX, posY);
                if (!IsValidPoint(newPoint, gridSize, r, area, numGridX, numGridY))
                    continue;
                activePoints.Add(newPoint);
                InsertPointToGrid(newPoint, gridSize, numGridY);
                found = true;
                break;
            }
            if (!found)
                activePoints.Remove(sample);
        }
        return allPoints;
    }

    static void InsertPointToGrid(Vector2 point,float gridSize, int numGridY) 
    {
        int XGrid = Mathf.FloorToInt(point.x / gridSize);
        int YGrid = Mathf.FloorToInt(point.y / gridSize);
        if (grids[XGrid * numGridY + YGrid] != -1)
            return;
        allPoints.Add(point);
        grids[XGrid * numGridY + YGrid] = allPoints.Count - 1;
    }
    static Vector2 GetRandomPoint(Vector2 area)
    {
        return new Vector2(Random.Range(0, area.x), Random.Range(0, area.y));
    }

    static bool IsValidPoint(Vector2 sample, float gridSize, float radius, Vector2 area, int gridWidth, int gridHeight) 
    {
        if (sample.x < 0 || sample.x > area.x|| sample.y < 0 || sample.y > area.y)
            return false;
        int XGrid = Mathf.FloorToInt(sample.x / gridSize);
        int YGrid = Mathf.FloorToInt(sample.y / gridSize);

        int i0 = Mathf.Max(0, XGrid - 2);
        int i1 = Mathf.Min(gridWidth - 1, XGrid + 2);
        int j0 = Mathf.Max(0, YGrid - 2);
        int j1 = Mathf.Min(gridHeight - 1, YGrid + 2);

        for (int i = i0; i < i1; i++) 
            for (int j = j0; j < j1; j++) 
                if (grids[i * gridHeight + j] != -1)
                    if (Vector2.Distance(allPoints[grids[i * gridHeight + j]], sample) < radius)
                        return false;

        return true;
    }

}
