using UnityEngine;
[ExecuteInEditMode]
[DefaultExecutionOrder(-98)]
public class TileManager : MonoBehaviour
{
    private TileData _tileData;
    private TileVisualizer _tileVisualizer;
    public float TileSize;
    public int TileGridDimension;
    public Vector2 TileGridCenterXZ;
    public Material DebugMaterial;

    public Mesh SpawnMesh;
    public Material SpawnMeshMaterial;
    public int SpawnSubivisions = 3;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _argsBuffer;
    private uint[] _args;
    private Vector3[] _spawnPos;

    private void OnEnable()
    {
        _spawnOnTileShader = (ComputeShader)Resources.Load("CS_SpawnOnTile");
        SetupTileGrid();
        SetupTileCompute();
        SetupTileVisual();
    }
    private void OnDisable()
    {
        CleanupTileVisual();
    }
    private void Update()
    {
        DrawTileVisual();
        DrawIndirect();
    }
 
    void SetupTileGrid() 
    {
        _tileData = new TileData(TileGridCenterXZ, TileGridDimension, TileSize);
        _tileData.ConstructTileGrid();
        
    }
    void SetupTileCompute()
    {
        if (
           SpawnMesh == null
           || SpawnMeshMaterial == null
           )
            return;
        int tileCount = TileGridDimension * TileGridDimension;
        int instancePerTile = SpawnSubivisions * SpawnSubivisions;
        _vertBuffer = new ComputeBuffer(tileCount * 4, sizeof(float) * 2);
        _vertBuffer.SetData(_tileData.GetTileVerts());

        _spawnPos = new Vector3[tileCount * instancePerTile];
        _spawnBuffer = new ComputeBuffer(tileCount * instancePerTile, sizeof(float) * 3);
        _spawnBuffer.SetData(_spawnPos);

        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        _args = new uint[] {
            SpawnMesh.GetIndexCount(0),
            (uint)(tileCount * instancePerTile),
            SpawnMesh.GetIndexStart(0),
            SpawnMesh.GetBaseVertex(0),
            0
        };
        _argsBuffer.SetData(_args);

        _spawnOnTileShader.SetInt("_NumTiles", tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", SpawnSubivisions);
        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _vertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(tileCount / 128f), 1, 1);
        SpawnMeshMaterial.SetBuffer("_SpawnBuffer", _spawnBuffer);

    }
    void DrawIndirect() 
    {

        if (
            SpawnMesh == null
            || SpawnMeshMaterial == null
            || _argsBuffer == null
            )
            return;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        Graphics.DrawMeshInstancedIndirect(SpawnMesh, 0,SpawnMeshMaterial, bounds, _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
    }
    void SetupTileVisual() 
    {
        if (_tileData == null)
            return;
        _tileVisualizer = new TileVisualizer(_tileData.TileGrid,DebugMaterial, _tileData.TileGridDimension);
        _tileVisualizer.VisualizeTiles();
    }
    void DrawTileVisual()
    {
        if (_tileVisualizer == null)
            return;
        _tileVisualizer.DrawIndirect();
    }
    void CleanupTileVisual() 
    {
        if (_tileVisualizer == null)
            return;
        _tileVisualizer.ReleaseBuffer();
        _vertBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _spawnBuffer?.Dispose();
    }
}
