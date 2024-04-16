using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static int CeilToNearestPowerOf2(int value)
    {
        int target = 2;
        while (target < value)
            target <<= 1;
        return target;
    }
    public static Mesh CombineMeshes(GameObject[] objs)
    {
        CombineInstance[] combine = new CombineInstance[objs.Length];

        for (int i = 0; i < objs.Length; i++)
        {
            combine[i].mesh = objs[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objs[i].transform.localToWorldMatrix; 
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine, true, true);
        return combinedMesh;
    }
}
