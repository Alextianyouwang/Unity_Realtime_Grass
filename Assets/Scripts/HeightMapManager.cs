using UnityEngine;

public class HeightMapManager : MonoBehaviour
{
    [Range(0.1f, 10)]
    public float scale = 1;
    public float xOffset;
    public float yOffset;
    [Range (0,5)]
    public float persistance = 0.5f;
    [Range(0, 1)]
    public float lacunarity = 0.5f;
    [Range(1, 8)]
    public int octaves = 3;
    public int seed;
    [Range(0, 1)]
    public float weight = 1;
    
    public float[,] PerlinMap(int numVert) 
    {
       return HeightMaps.PerlinMap(numVert, scale, xOffset, yOffset, persistance, lacunarity, octaves, seed, weight);
    }

}
