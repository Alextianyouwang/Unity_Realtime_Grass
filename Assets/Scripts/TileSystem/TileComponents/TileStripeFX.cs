
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tile Compoments/StripeFX")]
public class TileStripeFX : TileComponent
{
    public Material EffectMaterial;
    public Mesh Quad;
    private MaterialPropertyBlock _mpb;

    private ComputeBuffer _instanceDataBuffer;
    private ComputeBuffer _maskBuffer_external;

    private GraphicsBuffer.IndirectDrawIndexedArgs[] _argsBuffer;
    private GraphicsBuffer _commandBuffer;


    private InstanceData[] _instancesData;

    public static Func<int, int, float, Vector2, ComputeBuffer> OnRequestMaskBuffer;
    public static Action<int> OnRequestDisposeMaskBuffer;

    private int _hashCode;
    struct InstanceData
    {
        public Vector3 position;
        public float size;
        public int side;
    }

    private void OnEnable()
    {
        OnInitialize += Initialize;
        OnUpdate += Update;
        OnDispose += Dispose;
    }

    public void GetMaskBuffer()
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _maskBuffer_external = OnRequestMaskBuffer?.Invoke(_hashCode, _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }
    public void Initialize() 
    {
        if (Quad == null)
            return;

        _hashCode = GetHashCode();
        GetMaskBuffer();
        SetInstanceData();

        _mpb = new MaterialPropertyBlock();
        _instanceDataBuffer = new ComputeBuffer(_tileData.TileGridDimension * _tileData.TileGridDimension * 4, sizeof(float) * 4 + sizeof(int));
        _instanceDataBuffer.SetData(_instancesData);

        _argsBuffer = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _argsBuffer[0].indexCountPerInstance = Quad.GetIndexCount(0);
        _argsBuffer[0].instanceCount = (uint)_tileData.TileGridDimension * (uint)_tileData.TileGridDimension * 4;



        _mpb.SetBuffer("_InstanceDataBuffer", _instanceDataBuffer);

        if (_maskBuffer_external != null)
            _mpb.SetBuffer("_MaskBuffer", _maskBuffer_external);
    }

    private void SetInstanceData()
    {
        _instancesData = new InstanceData[_tileData.TileGridDimension * _tileData.TileGridDimension * 4];
        for (int x = 0; x < _tileData.TileGridDimension; x++)
        {
            for (int y = 0; y < _tileData.TileGridDimension; y++)
            {
                for (int k = 0; k < 4; k++) 
                {
                    Tile currentTile = _tileData.TileGrid[x * _tileData.TileGridDimension + y];
                    Vector4 posSize = currentTile.GetTilePosSize();
                    _instancesData[(x * _tileData.TileGridDimension + y) * 4 + k] = new InstanceData()
                    {
                        position = new Vector3(posSize.x, posSize.y, posSize.z),
                        size = posSize.w,
                        side = k

                    };

                }
            }
        }
    }

    public void Update() 
    {

        if (EffectMaterial == null)
            return;

        if (_tileData == null)
            return;

        if (_mpb == null)
            return;

        if (Quad == null)
            return;

        Bounds cullBound = new Bounds(Vector3.zero, Vector3.one * _tileData.TileGridDimension * _tileData.TileSize);


        RenderParams rp = new RenderParams(EffectMaterial);
        rp.worldBounds = cullBound;
        rp.matProps = _mpb;
        _commandBuffer.SetData(_argsBuffer);
        Graphics.RenderMeshIndirect(rp, Quad, _commandBuffer, 1, 0);

    }

    public void Dispose() 
    {

        _instanceDataBuffer?.Dispose();
        _commandBuffer?.Dispose();

        OnRequestDisposeMaskBuffer?.Invoke(_hashCode);
    }
}
