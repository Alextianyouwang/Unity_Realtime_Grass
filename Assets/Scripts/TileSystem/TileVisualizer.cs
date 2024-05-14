using System;
using UnityEngine;

public class TileVisualizer 
{
    private TileData _tileData;
    private Material _material;
    private MaterialPropertyBlock _mpb;

    private ComputeBuffer _instanceDataBuffer;
    private ComputeBuffer _vertBuffer;
    private ComputeBuffer _triangleBuffer;
    private ComputeBuffer _windBuffer_external;
    private ComputeBuffer _maskBuffer_external;
    private ComputeBuffer _argsBuffer;

    private int[] _triangles;
    private Vector3[] _vertices;
    private InstanceData[] _instancesData;
    private uint[] _args;

    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestWindBuffer;
    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestMaskBuffer;
    public static Action<int> OnRequestDisposeWindBuffer;
    public static Action<int> OnRequestDisposeMaskBuffer;

    private int _hashCode;
    struct InstanceData 
    {
        public Vector3 position;
        public Vector3 color;
        public float size;
    }

    public TileVisualizer(TileData tileData,Material material) 
    {
        _tileData = tileData;
        _material = material;
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
        _instancesData = new InstanceData[_tileData.TileGridDimension * _tileData.TileGridDimension];
        for (int x = 0; x < _tileData.TileGridDimension; x++)
        {
            for (int y = 0; y < _tileData.TileGridDimension; y++)
            {
                Tile currentTile = _tileData.TileGrid[x * _tileData.TileGridDimension + y];
                Vector4 posSize = currentTile.GetTilePosSize();
                Color col = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0.5f, 1, 0.5f, 1);
                _instancesData[x * _tileData.TileGridDimension + y] = new InstanceData()
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
        _instanceDataBuffer = new ComputeBuffer(_tileData.TileGridDimension * _tileData.TileGridDimension, sizeof(float) * 7);
        _instanceDataBuffer.SetData(_instancesData);
        _vertBuffer = new ComputeBuffer(4, sizeof(float) * 3);
        _vertBuffer.SetData(_vertices);
        _triangleBuffer = new ComputeBuffer(6, sizeof(float));
        _triangleBuffer.SetData(_triangles);
        _args = new uint[] {
            6,
            (uint)_tileData.TileGridDimension * (uint)_tileData.TileGridDimension,
            0,
            0
        };
        _argsBuffer = new ComputeBuffer(1, sizeof(uint) * 4,ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(_args);

        if (_material)
            _material.SetInt("_TileDimension", _tileData.TileGridDimension);

        _mpb.SetBuffer("_InstanceDataBuffer", _instanceDataBuffer);
        _mpb.SetBuffer("_VertBuffer", _vertBuffer);
        _mpb.SetBuffer("_TriangleBuffer", _triangleBuffer);
        GetWindBuffer();
        GetMaskBuffer();
        if (_windBuffer_external != null)
            _mpb.SetBuffer("_NoiseBuffer", _windBuffer_external);
        if (_maskBuffer_external != null) 
            _mpb.SetBuffer("_MaskBuffer", _maskBuffer_external);
    }

    public void GetWindBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _windBuffer_external = OnRequestWindBuffer?.Invoke(_hashCode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }

    public void GetMaskBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _maskBuffer_external = OnRequestMaskBuffer?.Invoke(_hashCode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }
    public void DrawIndirect() 
    {
        if (_material == null)
            return;

  
        Bounds cullBound = new Bounds(Vector3.zero, Vector3.one * _tileData.TileGridDimension * _instancesData[0].size);
        Graphics.DrawProceduralIndirect(_material, cullBound, MeshTopology.Triangles, _argsBuffer,0,null,_mpb);
    }
    public void ReleaseBuffer() 
    {
        _triangleBuffer?.Dispose();
        _vertBuffer?.Dispose(); 
        _instanceDataBuffer?.Dispose();
        _argsBuffer?.Dispose();
        OnRequestDisposeWindBuffer?.Invoke(_hashCode);
        OnRequestDisposeMaskBuffer?.Invoke(_hashCode);
    }
}
