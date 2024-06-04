using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tile Compoments/StripeFX")]
public class TileStripeFXComponent : TileComponent
{
    public Material EffectMaterial;
    public Mesh Quad;
    private Dictionary<int, TileStripeFX> _tileStripeFXes = new Dictionary<int, TileStripeFX>();

    private void OnEnable()
    {
        OnInitialize += InitializeComponent;
        OnUpdate += UpdateComponent;
        OnDispose += DisposeComponent;
    }

    void InitializeComponent(int hash) {
        TileStripeFX newFX = new TileStripeFX(_tileData, Quad, _maskBuffer_external, EffectMaterial);
        if (!_tileStripeFXes.ContainsKey(hash))
            _tileStripeFXes.Add(hash,newFX);
        _tileStripeFXes[hash].Initialize();
    }

    void UpdateComponent(int hash) {
        if (!Enabled)
            return;
        if (!_tileStripeFXes.ContainsKey(hash))
            return;
        _tileStripeFXes[hash].Update();
    }
    void DisposeComponent(int hash) {
        if (!_tileStripeFXes.ContainsKey(hash))
            return;
        _tileStripeFXes[hash].Dispose();
        _tileStripeFXes.Remove(hash);
    }
}


public class TileStripeFX
{
    private MaterialPropertyBlock _mpb;

    private ComputeBuffer _instanceDataBuffer;

    private GraphicsBuffer.IndirectDrawIndexedArgs[] _argsBuffer;
    private GraphicsBuffer _commandBuffer;
    private InstanceData[] _instancesData;

    private TileData _tileData;
    private Mesh _quad;
    private ComputeBuffer _maskBuffer_external;
    private Material _effectMaterial;

    public TileStripeFX(TileData tileData, Mesh quad, ComputeBuffer maskBuffer, Material effectMaterial)
    {
        _tileData = tileData;
        _quad = quad;
        _maskBuffer_external = maskBuffer;
        _effectMaterial = effectMaterial;
    }

    struct InstanceData
    {
        public Vector3 position;
        public float size;
        public int side;
    }

    public void Initialize()
    {
        if (_quad == null)
            return;
        SetInstanceData();

        _mpb = new MaterialPropertyBlock();
        _instanceDataBuffer = new ComputeBuffer(_tileData.TileGridDimension * _tileData.TileGridDimension * 4, sizeof(float) * 4 + sizeof(int));
        _instanceDataBuffer.SetData(_instancesData);

        _argsBuffer = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _argsBuffer[0].indexCountPerInstance = _quad.GetIndexCount(0);
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

        if (_effectMaterial == null)
            return;

        if (_tileData == null)
            return;

        if (_mpb == null)
            return;

        if (_quad == null)
            return;

        Bounds cullBound = new Bounds(Vector3.zero, Vector3.one * _tileData.TileGridDimension * _tileData.TileSize);
        RenderParams rp = new RenderParams(_effectMaterial);
        rp.worldBounds = cullBound;
        rp.matProps = _mpb;
        _commandBuffer.SetData(_argsBuffer);
        Graphics.RenderMeshIndirect(rp, _quad, _commandBuffer, 1, 0);

    }

    public void Dispose()
    {

        _instanceDataBuffer?.Dispose();
        _commandBuffer?.Dispose();

    }

}
