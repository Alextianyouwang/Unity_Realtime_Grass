
using UnityEngine;

public class FlexTileVisual
{
    private Material _debugMaterial;

    private ComputeBuffer _cb_instanceData;
    private ComputeBuffer _cb_vert;
    private ComputeBuffer _cb_triangle;
    private ComputeBuffer _cb_args;

    private int[] _triangles;
    private Vector3[] _vertices;
    private uint[] _args;

    private const int MAX_INSTANCE = 400;
    private MaterialPropertyBlock _mpb;

    private TileData[] _instances;

    public struct TileData 
    {
        public Vector2 Pos;
        public float Size;
        public Vector3 Color;
    }

    public FlexTileVisual( Material debugMaterial) 
    {
        _debugMaterial = debugMaterial;

    }
    public void SetData(TileData[] instance) 
    {
        _instances = instance;
    }
    public void Initialize()
    {
        GenerateQuadInfo();
        InitializeShader();
    }
    private void GenerateQuadInfo()
    {

        _triangles = new int[6];
        _triangles[0] = 0;
        _triangles[1] = 3;
        _triangles[2] = 2;
        _triangles[3] = 2;
        _triangles[4] = 1;
        _triangles[5] = 0;
        float quadSize = 0.5f;
        _vertices = new Vector3[4];
        _vertices[0] = new Vector3(-quadSize, 0, -quadSize);
        _vertices[1] = new Vector3(quadSize, 0, -quadSize);
        _vertices[2] = new Vector3(quadSize, 0, quadSize);
        _vertices[3] = new Vector3(-quadSize, 0, quadSize);

    }
  


    private void InitializeShader()
    {


        _mpb = new MaterialPropertyBlock();

        _cb_vert = new ComputeBuffer(4, sizeof(float) * 3);
        _cb_vert.SetData(_vertices);
        _cb_triangle = new ComputeBuffer(6, sizeof(float));
        _cb_triangle.SetData(_triangles);

        _cb_args = new ComputeBuffer(1, sizeof(uint) * 4, ComputeBufferType.IndirectArguments);

        _cb_instanceData = new ComputeBuffer(MAX_INSTANCE, sizeof(float) * 6);


    }
    public void Draw()
    {
        if (_debugMaterial == null)
            return;

        _cb_instanceData.SetData(_instances);

        _mpb.SetBuffer("_InstanceDataBuffer", _cb_instanceData);

        _mpb.SetBuffer("_VertBuffer", _cb_vert);
        _mpb.SetBuffer("_TriangleBuffer", _cb_triangle);

        _args = new uint[] {
            6,
            (uint) _instances.Length ,
            0,
            0
        };
        _cb_args.SetData(_args);

        Bounds cullBound = new Bounds(Vector3.zero, new Vector3(10000, 0, 10000));
        Graphics.DrawProceduralIndirect(_debugMaterial, cullBound, MeshTopology.Triangles, _cb_args, 0, null, _mpb);

    }
  
    public void Dispose()
    {
        _cb_triangle?.Dispose();
        _cb_vert?.Dispose();
        _cb_args?.Dispose();
        _cb_instanceData?.Dispose();

    }

}
