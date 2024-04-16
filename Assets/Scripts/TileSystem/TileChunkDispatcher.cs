using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
public class TileChunkDispatcher
{
    public TileChunk[] Chunks { get; private set; }
    private TileData _tileData;
    private TileClumpParser _tileClumpParser;

    private ComputeShader _spawnOnTileShader;
    private ComputeBuffer _rawSpawnBuffer;
    private ComputeBuffer _groundNormalBuffer;
    private ComputeBuffer _windBuffer_external;

    private RenderTexture _interactionTexture_external;
    private RenderTexture _zTex;
    private Material _zMat;

    private Mesh[] _spawnMesh;
    private Material _spawnMeshMaterial;
    private Camera _renderCam;
    private Renderer[] _occluders;
    private Mesh _occluder;

    public static Func<int,int,float,Vector2,ComputeBuffer> OnRequestWindBuffer;
    public static Func<RenderTexture> OnRequestInteractionTexture;
    public static Action<int> OnRequestDisposeWindBuffer;
    private int _tileCount;

    private bool _smoothPlacement;

    public TileChunkDispatcher(Mesh[] spawnMesh, Material spawmMeshMat, TileData tileData, Camera renderCam, bool smoothPlacement, Renderer[] occluders)
    {
        _spawnMesh = spawnMesh;
        _spawnMeshMaterial = spawmMeshMat;
        _tileData = tileData;
        _renderCam = renderCam;
        _smoothPlacement = smoothPlacement;
        _zTex = RenderTexture.GetTemporary(_renderCam.pixelWidth, _renderCam.pixelHeight, 32, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        _zTex.Create();
        _zMat = new Material(Shader.Find("Utility/S_DepthOnly"));
        _occluder = Utility.CombineMeshes(occluders.Select(x => x.gameObject).ToArray());
    }


    public void InitialSpawn()
    {
        _spawnOnTileShader = GameObject.Instantiate((ComputeShader)Resources.Load("CS/CS_InitialSpawn"));
        _tileCount = _tileData.TileGridDimension * _tileData.TileGridDimension;
        int instancePerTile = TileGrandCluster._SquaredInstancePerTile * TileGrandCluster._SquaredInstancePerTile;

        _rawSpawnBuffer = new ComputeBuffer(_tileCount * instancePerTile, sizeof(float) * 8);
        _groundNormalBuffer = new ComputeBuffer(_tileCount, sizeof(float) * 3);

        _spawnOnTileShader.SetInt("_NumTiles", _tileCount);
        _spawnOnTileShader.SetInt("_Subdivisions", TileGrandCluster._SquaredInstancePerTile);
        _spawnOnTileShader.SetInt("_NumTilesPerSide", _tileData.TileGridDimension);
        _spawnOnTileShader.SetBool("_SmoothPlacement", _smoothPlacement);

        _spawnOnTileShader.SetBuffer(0, "_VertBuffer", _tileData.VertBuffer);
        _spawnOnTileShader.SetBuffer(0, "_SpawnBuffer", _rawSpawnBuffer);
        _spawnOnTileShader.SetBuffer(0, "_GroundNormalBuffer", _groundNormalBuffer);

        _spawnOnTileShader.Dispatch(0, Mathf.CeilToInt(_tileCount / 128f), 1, 1);


    }

    private ComputeBuffer ProcessWithClumpData() 
    {
        _tileClumpParser = new TileClumpParser(
            _rawSpawnBuffer,
            _tileData.TileGridDimension,
            _tileData.TileSize,
            _tileData.TileGridCenterXZ - Vector2.one * _tileData.TileGridDimension * _tileData.TileSize * 0.5f
            );
        _tileClumpParser.ParseClump();
        return _tileClumpParser.ShareSpawnBuffer();
    }
    public void GetWindBuffer() 
    {
        float offset = -_tileData.TileGridDimension * _tileData.TileSize / 2 + _tileData.TileSize / 2;
        Vector2 botLeftCorner = _tileData.TileGridCenterXZ + new Vector2(offset, offset);
        _windBuffer_external = OnRequestWindBuffer?.Invoke(GetHashCode(), _tileData.TileGridDimension, _tileData.TileSize, botLeftCorner);
    }
    public void GetInteractionTexture() 
    {
        _interactionTexture_external = OnRequestInteractionTexture?.Invoke();
    }

    public void InitializeChunks() 
    {
        _rawSpawnBuffer =  ProcessWithClumpData();
        int chunksPerSide = TileGrandCluster._SquaredChunkPerCluster;
        Chunks = new TileChunk[chunksPerSide * chunksPerSide];
        int chunkDimension = _tileData.TileGridDimension / chunksPerSide;
        int totalInstancePerChunk = chunkDimension * chunkDimension * TileGrandCluster._SquaredInstancePerTile * TileGrandCluster._SquaredInstancePerTile;
        float chunkSize = _tileData.TileGridDimension * _tileData.TileSize / chunksPerSide;
        Vector2 botLeft = _tileData.TileGridCenterXZ - chunkSize * chunksPerSide * Vector2.one / 2 + Vector2.one * chunkSize / 2;
        
        for (int x = 0; x < chunksPerSide; x++)
        {
            for (int y = 0; y < chunksPerSide; y++) 
            {
                ComputeBuffer chunkBuffer = new ComputeBuffer(totalInstancePerChunk, sizeof(float) * 8);
                _spawnOnTileShader.SetInt("_ChunkIndexX", x);
                _spawnOnTileShader.SetInt("_ChunkIndexY", y);
                _spawnOnTileShader.SetInt("_ChunkPerSide", chunksPerSide);
                _spawnOnTileShader.SetBuffer(1, "_SpawnBuffer", _rawSpawnBuffer);
                _spawnOnTileShader.SetBuffer(1, "_ChunkSpawnBuffer", chunkBuffer);
                _spawnOnTileShader.Dispatch(1, Mathf.CeilToInt(totalInstancePerChunk / 128f), 1, 1);
                Bounds b = new Bounds( new Vector3 (botLeft.x + chunkSize* x,0,botLeft.y + chunkSize * y) ,Vector3.one * chunkSize);
                TileChunk t = Chunks[x * chunksPerSide + y] = new TileChunk(
                    _spawnMesh, 
                    _spawnMeshMaterial, 
                    _renderCam, 
                    chunkBuffer, 
                    b,
                    _tileData
                    );
                t.SetWindBuffer(_windBuffer_external);
                t.SetGroundNormalBuffer(_groundNormalBuffer);
                t.SetInteractionTexture(_interactionTexture_external);
                t.SetupCuller();
            }
        }
    }
    public void BlitDepthTexture() 
    {
        if (!TileGrandCluster._EnableOcclusionCulling)
            return;
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "DrawOccluderDepth";

        cmd.SetViewMatrix(_renderCam.worldToCameraMatrix);
        cmd.SetProjectionMatrix(_renderCam.projectionMatrix);
        cmd.SetRenderTarget(_zTex);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.DrawMesh(_occluder, Matrix4x4.identity, _zMat);
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void DispatchTileChunksDrawCall() 
    {
        int totalInstance = 0;
        Plane[] p= GeometryUtility.CalculateFrustumPlanes(_renderCam);
        foreach (TileChunk t in Chunks)
            if (GeometryUtility.TestPlanesAABB(p, t.ChunkBounds))
                if (t != null)
                {
                    t.SetZTex(_zTex);
                    t.DrawContent(ref totalInstance);
                }
        //Debug.Log(totalInstance);
    }
    public void ReleaseBuffer()
    {
        _rawSpawnBuffer?.Dispose();
        _groundNormalBuffer?.Dispose();
        OnRequestDisposeWindBuffer?.Invoke(GetHashCode());
        RenderTexture.ReleaseTemporary(_zTex);
        _zTex = null;_zMat = null;
        foreach (TileChunk t in Chunks)
            t?.ReleaseBuffer();
        _tileClumpParser?.ReleaseBuffer();
    }
}

