
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class DoubleSideMeshBaker : MonoBehaviour
{
    public Mesh SourceMesh;
    private Mesh _finalMesh;
    private List<int> _sourceTriangles = new List<int>();
    private List<Vector3> _sourceVertices = new List<Vector3>();
    private List<Vector2> _sourceUVs = new List<Vector2>();
    private List<Vector3> _sourceNormals = new List<Vector3>();


    private List<int> _finalTriangles = new List<int>();
    private List<Vector3> _finalVertices = new List<Vector3>();
    private List<Vector3> _finalNormals = new List<Vector3>();
    private List<Vector2> _finalUVs = new List<Vector2>();


    private void OnEnable()
    {
        GetSourceMeshInfo();
        GenerateBackSide();
        ExportMesh(_finalMesh, $"Assets/Meshes/{SourceMesh.name}_2Sided.mesh");
    }

    void GetSourceMeshInfo() 
    {
        if (SourceMesh == null)
            return;
        _finalTriangles = SourceMesh.GetTriangles(0).ToList();
        SourceMesh.GetVertices(_sourceVertices);
        SourceMesh.GetNormals(_sourceNormals);
        SourceMesh.GetUVs(0,_sourceUVs);
      
    }

    void GenerateBackSide() 
    {
        if (SourceMesh == null)
            return;

        int[] targetTriangles = new int[_finalTriangles.Count];
        for (int i = 0; i < _finalTriangles.Count / 3; i++) 
        {
            int triStart = i * 3;
            targetTriangles[triStart + 2] = _finalTriangles[triStart ];
            targetTriangles[triStart + 1] = _finalTriangles[triStart + 1];
            targetTriangles[triStart ] = _finalTriangles[triStart + 2];
        }

        _finalTriangles = MergeArrays(_sourceTriangles, targetTriangles.ToList());


        _finalVertices = MergeArrays(_sourceVertices, _sourceVertices);

        _finalNormals = MergeArrays(_sourceNormals, _sourceNormals);

        _finalUVs = MergeArrays(_sourceUVs, _sourceUVs);
   /*     for (int i = 0; i < _finalVertices.Count; i++) 
        {
            if(i < _finalVertices.Count/2)
                _finalVertices[i] += Vector3.forward * 0.1f;
        }*/

        _finalMesh = new Mesh();
        _finalMesh.SetVertices(_finalVertices);
        _finalMesh.SetNormals(_finalNormals);

        _finalMesh.SetIndices(_finalTriangles, MeshTopology.Triangles,0,true);
        _finalMesh.SetUVs(0,_finalUVs);

    }
/*
    private void OnDrawGizmos()
    {
        if (_finalVertices == null)
            return;
        foreach (Vector3 v in _finalVertices) 
        {
            Gizmos.DrawSphere(v, 0.01f);
        }
    }*/
    public List<T> MergeArrays<T>(List<T> arr1, List<T> arr2)
    {
        List<T> mergedList = new List<T>(arr1);
        mergedList.AddRange(arr2);
        return mergedList;
    }

    public void ExportMesh(Mesh mesh, string relativePath)
    {
        // Ensure the mesh and path are valid
        if (mesh == null)
        {
            Debug.LogError("Mesh is null. Cannot export.");
            return;
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            Debug.LogError("Path is null or empty. Cannot export.");
            return;
        }

        try
        {
            // Save mesh asset
            AssetDatabase.CreateAsset(mesh, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Mesh exported successfully to: " + relativePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error exporting mesh: " + ex.Message);
        }
    }

}
