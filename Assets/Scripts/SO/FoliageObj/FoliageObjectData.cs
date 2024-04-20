using UnityEngine;

[CreateAssetMenu(menuName = "Foliage Object/Object Data")]
public class FoliageObjectData : ScriptableObject
{
    public Mesh[] SpawnMesh;
    public Material SpawnMeshMaterial;
    public int SquaredInstancePerTile = 3;
    public int SquaredChunksPerCluster = 4;
    public int SquaredTilePerClump = 8;
    [Range(0f, 3f)]
    public float OccludeeBoundScaleMultiplier = 1;

}
