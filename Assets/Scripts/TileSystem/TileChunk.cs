using UnityEngine;

public class TileChunk
{
    public Bounds ChunkBounds { get; private set; }

    private TileData _tileData;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private MaterialPropertyBlock _mpb;
    private Camera _renderCam;

    private ComputeShader _sampleTileShader;
    private ComputeShader _cullShader;


    private ComputeBuffer _spawnBuffer;
    private ComputeBuffer _voteBuffer;
    private ComputeBuffer _scanBuffer;
    private ComputeBuffer _groupScanInBuffer;
    private ComputeBuffer _groupScanOutBuffer;
    private ComputeBuffer _compactBuffer;
    private ComputeBuffer[] _argsBuffer;

    private ComputeBuffer _windBuffer_external;
    private ComputeBuffer _groundNormalBuffer_external;

    private ComputeBuffer _sampledGroundNormalBuffer;

    private int _elementCount;
    private int _groupCount;

    private Color _chunkColor;
    private Vector4 _samplingData;// xy:index z:tilePerChunk w:chunkPerSide

    private Vector3[] _groundNormals;
    public TileChunk(Mesh[] spawnMesh, Material spawmMeshMat,  Camera renderCam, ComputeBuffer initialBuffer,Bounds chunkBounds, Vector4 samplingData , TileData tileData) 
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _renderCam = renderCam;
        _spawnBuffer = initialBuffer;
        ChunkBounds = chunkBounds;
        _mpb = new MaterialPropertyBlock();
        _samplingData = samplingData;
        _tileData = tileData;
    }
    public void Init() 
    {
        SetupWindSampler();
        SetupGroundNormalSampler();
        SetupCuller();
    }
    public void Update() 
    {
        UpdateWind();
        DrawContent();
    }
    public void SetWindBuffer(ComputeBuffer windBuffer) 
    {
        _windBuffer_external = windBuffer;
    }
    public void SetGroundNormalBuffer(ComputeBuffer normalBuffer) 
    { 
        _groundNormalBuffer_external = normalBuffer;
    }
    private void SetupWindSampler() 
    {
        if (_windBuffer_external == null)
            return;
        _sampleTileShader = (ComputeShader)GameObject.Instantiate(Resources.Load("CS_SampleWind"));
        _sampleTileShader.SetInt("_IndexX",(int)_samplingData.x);
        _sampleTileShader.SetInt("_IndexY",(int)_samplingData.y);
        _sampleTileShader.SetInt("_ChunkDimension", (int)_samplingData.z);
        _sampleTileShader.SetInt("_NumChunkPerSide", (int)_samplingData.w);
        _sampleTileShader.SetInt("_InstancePerTile", TileGrandCluster._SpawnSubdivisions * TileGrandCluster._SpawnSubdivisions);

        _sampleTileShader.SetBuffer(0,"_SpawnBuffer", _spawnBuffer);
        _sampleTileShader.SetBuffer(0,"_WindBuffer", _windBuffer_external);
    }

    private void SetupGroundNormalSampler() 
    {
        _groundNormals = new Vector3[(int)_samplingData.z * (int)_samplingData.z];
        _sampledGroundNormalBuffer = new ComputeBuffer((int)_samplingData.z * (int)_samplingData.z, sizeof(float) * 3);
        _sampledGroundNormalBuffer.SetData(_groundNormals);
        _sampleTileShader.SetBuffer(1, "_NormalBuffer",_groundNormalBuffer_external);
        _sampleTileShader.SetBuffer(1, "_SampledNormalBuffer", _sampledGroundNormalBuffer);
        _sampleTileShader.Dispatch(1, Mathf.CeilToInt((int)_samplingData.z * (int)_samplingData.z / 128f), 1, 1);
    }

    private void SetupCuller() 
    {
        _cullShader = (ComputeShader)GameObject.Instantiate(Resources.Load("CS_GrassCulling")) ;
        

        _elementCount = Utility.CeilToNearestPowerOf2(_spawnBuffer.count);
        _groupCount = _elementCount / 128;

        _voteBuffer = new ComputeBuffer(_elementCount, sizeof(int));
        _scanBuffer = new ComputeBuffer(_elementCount, sizeof(int));
        _groupScanInBuffer = new ComputeBuffer(_groupCount, sizeof(int));
        _groupScanOutBuffer = new ComputeBuffer(_groupCount, sizeof(int));

        _compactBuffer = new ComputeBuffer(_elementCount, sizeof(float) * 13);
        _argsBuffer = new ComputeBuffer[_spawnMesh.Length];
        for(int i = 0; i< _spawnMesh.Length; i++)
        {
            _argsBuffer[i] = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
            uint[] _args = new uint[] {
            _spawnMesh[i].GetIndexCount(0),
            (uint)_spawnBuffer.count,
            _spawnMesh[i].GetIndexStart(0),
            _spawnMesh[i].GetBaseVertex(0),
            0
        };
            _argsBuffer[i].SetData(_args);
        }
        

        _cullShader.SetInt("_InstanceCount", _spawnBuffer.count);
        _cullShader.SetFloat("_MaxRenderDist", TileGrandCluster._MaxRenderDistance);
        _cullShader.SetFloat("_DensityFalloffDist", TileGrandCluster._DensityFalloffThreshold);
        _cullShader.SetFloat("_OffsetX", -ChunkBounds.center.x);
        _cullShader.SetFloat("_OffsetY", -ChunkBounds.center.z);

        _cullShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        _cullShader.SetBuffer(0, "_VoteBuffer", _voteBuffer);

        _cullShader.SetBuffer(1, "_VoteBuffer", _voteBuffer);
        _cullShader.SetBuffer(1, "_ScanBuffer", _scanBuffer);
        _cullShader.SetBuffer(1, "_GroupScanBufferIn", _groupScanInBuffer);

        _cullShader.SetBuffer(2, "_GroupScanBufferIn", _groupScanInBuffer);
        _cullShader.SetBuffer(2, "_GroupScanBufferOut", _groupScanOutBuffer);

        _cullShader.SetBuffer(3, "_CompactBuffer", _compactBuffer);
        _cullShader.SetBuffer(3, "_SpawnBuffer", _spawnBuffer);
        _cullShader.SetBuffer(3, "_VoteBuffer", _voteBuffer);
        _cullShader.SetBuffer(3, "_ScanBuffer", _scanBuffer);
        _cullShader.SetBuffer(3, "_GroupScanBufferOut", _groupScanOutBuffer);
        for (int i = 0; i < _spawnMesh.Length; i++) 
        {
            _cullShader.SetBuffer(3, $"_ArgsBuffer{i}", _argsBuffer[i]);
            _cullShader.SetBuffer(4, $"_ArgsBuffer{i}", _argsBuffer[i]);
        }

        _chunkColor = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0.5f, 1, 0.5f, 1);
        _mpb.SetBuffer("_SpawnBuffer", _compactBuffer);
        _mpb.SetBuffer("_RawSpawnBuffer", _spawnBuffer);
        _mpb.SetBuffer("_GroundNormalBuffer", _sampledGroundNormalBuffer);
        _mpb.SetInt("_InstancePerTile", TileGrandCluster._SpawnSubdivisions * TileGrandCluster._SpawnSubdivisions);
        _mpb.SetInt("_IndexX", (int)_samplingData.x);
        _mpb.SetInt("_IndexY", (int)_samplingData.y);
        _mpb.SetInt("_ChunkDimension", (int)_samplingData.z);
        _mpb.SetInt("_NumChunkPerSide", (int)_samplingData.w);
        _mpb.SetColor("_ChunkColor", _chunkColor);
    }

    private void UpdateWind()
    {
        if (_windBuffer_external == null)
            return;
        _sampleTileShader.Dispatch(0, Mathf.CeilToInt((int)_samplingData.z * (int)_samplingData.z / 128f), 1, 1);
    }
    private void DrawContent()
    {
        if (
            _spawnMesh == null
            || _spawnMeshMaterial == null
            || _argsBuffer == null
            )
            return;

        _cullShader.SetMatrix("_Camera_P", _renderCam.projectionMatrix);
        _cullShader.SetMatrix("_Camera_V", _renderCam.transform.worldToLocalMatrix);
        _cullShader.Dispatch(4, 1, 1, 1);
        _cullShader.Dispatch(0, Mathf.CeilToInt(_spawnBuffer.count / 128f), 1, 1);
        _cullShader.Dispatch(1, _groupCount, 1, 1);
        _cullShader.Dispatch(2, 1, 1, 1);
        _cullShader.Dispatch(3, Mathf.CeilToInt(_spawnBuffer.count / 128f), 1, 1);

        float dist = Vector3.Distance(_renderCam.transform.position, ChunkBounds.center);
        if (dist < TileGrandCluster._LOD_Threshold_01)
        {
            Graphics.DrawMeshInstancedIndirect(_spawnMesh[0], 0, _spawnMeshMaterial, ChunkBounds, _argsBuffer[0],
          0, _mpb, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
            _mpb.SetColor("_LOD_Color", Color.green);

        }
        else if (dist >= TileGrandCluster._LOD_Threshold_01 && dist <= TileGrandCluster._LOD_Threshold_12)
        {
            Graphics.DrawMeshInstancedIndirect(_spawnMesh[1], 0, _spawnMeshMaterial, ChunkBounds, _argsBuffer[1],
         0, _mpb, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
            _mpb.SetColor("_LOD_Color", Color.blue);

        }
        else  if (dist > TileGrandCluster._LOD_Threshold_12)
        {
            Graphics.DrawMeshInstancedIndirect(_spawnMesh[2], 0, _spawnMeshMaterial, ChunkBounds, _argsBuffer[2],
           0, _mpb, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null, UnityEngine.Rendering.LightProbeUsage.BlendProbes);
            _mpb.SetColor("_LOD_Color", Color.yellow);

        }
    }
    

    public void ReleaseBuffer() 
    {
        _spawnBuffer?.Dispose();
        _voteBuffer?.Dispose();
        _scanBuffer?.Dispose();
        _groupScanInBuffer?.Dispose();
        _groupScanOutBuffer?.Dispose();
        _compactBuffer?.Dispose();

        _sampledGroundNormalBuffer?.Dispose();
        if (_argsBuffer != null) 
            foreach (ComputeBuffer arg in _argsBuffer) 
                arg?.Dispose();
    
        _cullShader = null;
    }
}
