using UnityEngine;

public class TileVisualizer 
{
    private Tile[] _tiles;
    private Material _material;
    private MaterialPropertyBlock _mpb;
    private int _tileDimentions;

    private ComputeBuffer _instanceDataBuffer;
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _triangleBuffer;
    private ComputeBuffer _noiseBuffer;
    private ComputeBuffer _argsBuffer;

    private int[] _triangles;
    private Vector3[] _vertices;
    private InstanceData[] _instancesData;
    private uint[] _args;

    struct InstanceData 
    {
        public Vector3 position;
        public Vector3 color;
        public float size;
    }

    public TileVisualizer(Tile[] _t,Material _m, int _dimention) 
    {
        _tiles = _t;
        _material = _m;
        _tileDimentions = _dimention;
    }
    public void GetNoiseBuffer(ComputeBuffer _noise) 
    {
        _noiseBuffer = _noise;
    }
    public void InitializeTileDebug()
    {

        SetInstanceData();
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
        _vertices = new Vector3[4];
        _vertices[0] = new Vector3(-0.45f, 0, -0.45f);
        _vertices[1] = new Vector3(0.45f, 0, -0.45f);
        _vertices[2] = new Vector3(0.45f, 0, 0.45f);
        _vertices[3] = new Vector3(-0.45f, 0, 0.45f);
    }


    private void SetInstanceData() 
    {
        _instancesData = new InstanceData[_tileDimentions * _tileDimentions];
        for (int x = 0; x < _tileDimentions; x++)
        {
            for (int y = 0; y < _tileDimentions; y++)
            {
                Tile currentTile = _tiles[x * _tileDimentions + y];
                Vector4 posSize = currentTile.GetTilePosSize();
                Color col = Random.ColorHSV(0, 1, 0, 1, 0.5f, 1, 0.5f, 1);
                _instancesData[x * _tileDimentions + y] = new InstanceData()
                {
                    position = new Vector3(posSize.x, posSize.y, posSize.z),
                    color = new Vector3(col.r, col.g, col.b),
                    size = posSize.w
                };

            }
        }
    }

    private void InitializeShader() 
    {
        _mpb = new MaterialPropertyBlock();
        _instanceDataBuffer = new ComputeBuffer(_tileDimentions * _tileDimentions, sizeof(float) * 7);
        _instanceDataBuffer.SetData(_instancesData);
        _vertBuffer = new ComputeBuffer(4, sizeof(float) * 3);
        _vertBuffer.SetData(_vertices);
        _triangleBuffer = new ComputeBuffer(6, sizeof(float));
        _triangleBuffer.SetData(_triangles);
        _args = new uint[] {
            6,
            (uint)_tileDimentions * (uint)_tileDimentions,
            0,
            0
        };
        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 4,ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);

        if (_material)
            _material.SetInt("_TileDimension", _tileDimentions);

        _mpb.SetBuffer("_InstanceDataBuffer", _instanceDataBuffer);
        _mpb.SetBuffer("_VertBuffer", _vertBuffer);
        _mpb.SetBuffer("_TriangleBuffer", _triangleBuffer);

        if (_noiseBuffer != null)
            _mpb.SetBuffer("_NoiseBuffer", _noiseBuffer);

    }
    public void DrawIndirect() 
    {
        if (_material == null)
            return;

  
        Bounds cullBound = new Bounds(Vector3.zero, Vector3.one * _tileDimentions * _instancesData[0].size);
        Graphics.DrawProceduralIndirect(_material, cullBound, MeshTopology.Triangles, _argsBuffer,0,null,_mpb);
    }
    public void ReleaseBuffer() 
    {
        _triangleBuffer?.Dispose();
        _vertBuffer?.Dispose(); 
        _instanceDataBuffer?.Dispose();
        _noiseBuffer?.Dispose();
        _argsBuffer?.Dispose();
    }
}
