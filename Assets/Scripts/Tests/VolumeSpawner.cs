using UnityEngine;

[ExecuteInEditMode]
public class VolumeSpawner : MonoBehaviour
{
    [HideInInspector]
    public Vector3[] Volumes;
    public float SideLength = 5;
    public int X, Y, Z;


    private void OnEnable()
    {
        SpawnPoint();
    }


    private void SpawnPoint() 
    {
        int i = 0;
        Volumes = new Vector3[X * Y* Z];
        for (int x = 0; x < X; x++) 
        {
            for (int y = 0; y < Y; y++) 
            {
                for (int z = 0; z < Z ; z++)
                {
                    Vector3 initialPos =  - new Vector3(SideLength / 2, SideLength / 2, SideLength / 2);
                    Vector3 localPos = initialPos + new Vector3(x * SideLength / (X - 1), y * SideLength / (Y - 1), z * SideLength / (Z - 1));
                    Vector4 worldPos = transform.localToWorldMatrix * new Vector4 (localPos.x,localPos.y,localPos.z,1);
                    Volumes[i] = new Vector3(worldPos.x, worldPos.y, worldPos.z);
                    i++;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        /*foreach (Vector3 v in Volumes)
            Gizmos.DrawSphere(v, 0.2f);*/
    }

}
