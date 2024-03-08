using System;
using UnityEngine;
[ExecuteInEditMode]
public class VolumeSpawner : MonoBehaviour
{
    [HideInInspector]
    public Vector3[] Volumes;
    public float SideLenght = 5;
    public int Segments = 10;
    private void OnEnable()
    {
        SpawnPoint();
    }

    private void Update()
    {
        SpawnPoint();
    }

    private void SpawnPoint() 
    {
        int i = 0;
        Volumes = new Vector3[Segments  * Segments * Segments];
        float inc = SideLenght / (Segments - 1);
        for (int x = 0; x < Segments; x++) 
        {
            for (int y = 0; y < Segments; y++) 
            {
                for (int z = 0; z < Segments ; z++)
                {
                    Vector3 initialPos =  - new Vector3(SideLenght / 2, SideLenght / 2, SideLenght / 2);
                    Vector3 localPos = initialPos + new Vector3(x, y, z) * inc;
                    Vector4 worldPos = transform.localToWorldMatrix * new Vector4 (localPos.x,localPos.y,localPos.z,1);
                    Volumes[i] = new Vector3(worldPos.x, worldPos.y, worldPos.z);
                    i++;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        foreach (Vector3 v in Volumes)
            Gizmos.DrawSphere(v, 0.2f);
    }

}
