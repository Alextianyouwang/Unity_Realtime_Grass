using UnityEngine;

public static class HeightMaps
{
    public static float[,] PerlinMap(int numRows, float scale, float xOffset, float yOffset, float persistance, float lacunarity, int octaves, int seed , float weight)
    {
        System.Random psudoRandom = new System.Random(seed);
        Vector2[] randomSeeds = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            randomSeeds[i] = new Vector2(psudoRandom.Next(-100000, 100000), psudoRandom.Next(-100000, 100000));
        }
        float[,] map = new float[numRows, numRows];
        scale = scale <= 0 ? 0 : scale;
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numRows; x++)
            {

                float perlinValue = 0;
                float amplitude = 1;
                float frequency = 1;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - numRows / 2 + xOffset + randomSeeds[i].x) / scale * frequency;
                    float sampleY = (y - numRows / 2 + yOffset + randomSeeds[i].y) / scale * frequency;
                    float remapPerlin = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    perlinValue += remapPerlin * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                map[x, y] = perlinValue * weight;
            }
        }
        return map;
    }

    public static float[,] TextureHeightMap(Texture2D heightMap, int numRows, float xTileOffset, float yTileOffset, float scale)
    {
        float[,] manualHeightValue;
        if (heightMap.isReadable)
        {
            int resolution = heightMap.width;

            manualHeightValue = new float[resolution, resolution];
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numRows; x++)
                {
                    float sampleX = (x - numRows / 2 + xTileOffset) / scale + resolution / 2;
                    float sampleY = (y - numRows / 2 + yTileOffset) / scale + resolution / 2;

                    manualHeightValue[x, y] =
                        sampleX > resolution ||
                        sampleY > resolution ||
                        sampleX < 0 ||
                        sampleY < 0 ? -1 :
                        heightMap.GetPixel((int)sampleX, (int)sampleY).r;
                }
            }
        }
        else
        {
            manualHeightValue = null;
            Debug.LogWarning("Please Enable Texture Read/Wirte.");
        }


        return manualHeightValue;
    }

}
