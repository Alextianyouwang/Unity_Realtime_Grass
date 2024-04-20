using UnityEngine;

public class TileChunk
{
    public Bounds ChunkBounds { get; private set; }

    private TileData _tileData;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private MaterialPropertyBlock _mpb;
    private Camera _renderCam;

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

    private RenderTexture _interactionTexture_external;
    private RenderTexture _zTex_external;

    private int _elementCount;
    private int _groupCount;
    private float _occludeeBoundScaleMultiplier;
    private float _densityFilter;

    private Color _chunkColor;

    public TileChunk(Mesh[] spawnMesh, Material spawmMeshMat,  Camera renderCam, ComputeBuffer initialBuffer,Bounds chunkBounds, TileData tileData, 
        float occludeeBoundScaleMultiplier, float densityFilter) 
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _renderCam = renderCam;
        _spawnBuffer = initialBuffer;
        ChunkBounds = chunkBounds;
        _mpb = new MaterialPropertyBlock();
        _tileData = tileData;
        _occludeeBoundScaleMultiplier = occludeeBoundScaleMultiplier;
        _densityFilter = densityFilter;
    }

    public void SetWindBuffer(ComputeBuffer windBuffer) 
    {
        _windBuffer_external = windBuffer;
    }
    public void SetGroundNormalBuffer(ComputeBuffer normalBuffer) 
    { 
        _groundNormalBuffer_external = normalBuffer;
    }
    public void SetInteractionTexture(RenderTexture interactionTex) 
    {
        _interactionTexture_external = interactionTex;
    }
    public void SetZTex(RenderTexture zTex) 
    {
        _zTex_external = zTex;
    }
    public void SetupCuller() 
    {
        _cullShader = (ComputeShader)GameObject.Instantiate(Resources.Load("CS/CS_GrassCulling")) ;
        

        _elementCount = Utility.CeilToNearestPowerOf2(_spawnBuffer.count);
        _groupCount = _elementCount / 128;

        _voteBuffer = new ComputeBuffer(_elementCount, sizeof(int));
        _scanBuffer = new ComputeBuffer(_elementCount, sizeof(int));
        _groupScanInBuffer = new ComputeBuffer(_groupCount, sizeof(int));
        _groupScanOutBuffer = new ComputeBuffer(_groupCount, sizeof(int));

        _compactBuffer = new ComputeBuffer(_elementCount, sizeof(float) * 8);
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
        
        Vector2 bl = _tileData.TileGridCenterXZ - (Vector2.one * _tileData.TileSize * _tileData.TileGridDimension / 2);

        _cullShader.SetInt("_InstanceCount", _spawnBuffer.count);
        _cullShader.SetFloat("_ClusterBotLeftX", bl.x);
        _cullShader.SetFloat("_ClusterBotLeftY", bl.y);
        _cullShader.SetFloat("_TileSize", _tileData.TileSize);
        _cullShader.SetInt("_NumTilePerClusterSide", _tileData.TileGridDimension);
        _cullShader.SetFloat("_GrassBoundScale", _occludeeBoundScaleMultiplier);
        _cullShader.SetFloat("_DensityFilter", _densityFilter);

        _cullShader.SetBuffer(0, "_SpawnBuffer", _spawnBuffer);
        _cullShader.SetBuffer(0, "_VoteBuffer", _voteBuffer);
        _cullShader.SetBuffer(0, "_DensityBuffer", _tileData.TypeBuffer);

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
        _mpb.SetBuffer("_GroundNormalBuffer", _groundNormalBuffer_external);
        _mpb.SetBuffer("_WindBuffer", _windBuffer_external);
        _mpb.SetFloat("_ClusterBotLeftX",bl.x);
        _mpb.SetFloat("_ClusterBotLeftY",bl.y);
        _mpb.SetFloat("_TileSize", _tileData.TileSize);
        _mpb.SetInt("_NumTilePerClusterSide", _tileData.TileGridDimension);
        _mpb.SetColor("_ChunkColor", _chunkColor);

        if (_interactionTexture_external)
            _mpb.SetTexture("_InteractionTexture", _interactionTexture_external);
    }
    public void DrawContent(ref int instanceCount)
    {
        if (
            _spawnMesh == null
            || _spawnMeshMaterial == null
            || _argsBuffer == null
            )
            return;

        _cullShader.SetMatrix("_Camera_P", _renderCam.projectionMatrix);
        _cullShader.SetMatrix("_Camera_V", _renderCam.worldToCameraMatrix);
        _cullShader.SetFloat("_Camera_Near", _renderCam.nearClipPlane);
        _cullShader.SetFloat("_Camera_Far", _renderCam.farClipPlane);
        _cullShader.SetFloat("_MaxRenderDist", TileGrandCluster._MaxRenderDistance);
        _cullShader.SetFloat("_DensityFalloffDist", TileGrandCluster._DensityFalloffThreshold);

        _cullShader.SetBool("_EnableOcclusionCulling", TileGrandCluster._EnableOcclusionCulling);

        _cullShader.SetTexture(0, "_HiZTexture", _zTex_external);

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
        /*for (int i = 0; i < _spawnMesh.Length; i++) 
        {
            uint[] data = new uint[5];
            _argsBuffer[i].GetData(data);
            instanceCount += (int)data[1];
        }*/

    }


    public void ReleaseBuffer() 
    {
        _spawnBuffer?.Dispose();
        _voteBuffer?.Dispose();
        _scanBuffer?.Dispose();
        _groupScanInBuffer?.Dispose();
        _groupScanOutBuffer?.Dispose();
        _compactBuffer?.Dispose();
        if (_argsBuffer != null) 
            foreach (ComputeBuffer arg in _argsBuffer) 
                arg?.Dispose();
    
        _cullShader = null;
    }
}
