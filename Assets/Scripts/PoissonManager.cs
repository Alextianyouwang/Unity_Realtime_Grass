using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PoissonManager: MonoBehaviour
{
    public float Width = 10f, Height = 10f;
    [Range(0.1f,1f)]
    public float Radius = 2f;
    [Range(2, 10)]

    public int Iterations = 5 ;
    List<Vector2> samples = new List<Vector2>();
    private void OnEnable()
    {
       samples =  PoissonDiskSampler.PoissonSamples(Radius, Iterations, new Vector2(Width, Height));
    }
    private void OnDrawGizmos()
    {
        foreach (Vector2 v in samples) 
        {
            Gizmos.DrawSphere(v, 0.02f);
        }
    }
}
