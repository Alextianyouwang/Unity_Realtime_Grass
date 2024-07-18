#ifndef INSTANCE_SHARED_DATA_INCLUDE
#define INSTANCE_SHARED_DATA_INCLUDE

////////////////////////////////////////////////
// Spawn Data
struct SpawnData
{
    float3 positionWS;
    float hash;
    float4 clumpInfo;
    float4 postureData;
};
StructuredBuffer<SpawnData> _SpawnBuffer;
////////////////////////////////////////////////

////////////////////////////////////////////////
// Field Data
StructuredBuffer<float3> _GroundNormalBuffer;
StructuredBuffer<float4> _WindBuffer;
StructuredBuffer<float4> _MaskBuffer;
Texture2D<float> _InteractionTexture;
Texture2D<float4> _FlowTexture;

int _NumTilePerClusterSide;
float _ClusterBotLeftX, _ClusterBotLeftY, _TileSize;
////////////////////////////////////////////////

////////////////////////////////////////////////
// Debug
float3 _ChunkColor, _LOD_Color;
////////////////////////////////////////////////
struct VertexSharedData
{
    float3 spawnPosWS;
    float3 groundNormalWS;
    float4 wind;
    float4 mask;
    float interaction;
    float4 flow;
    float4 posture;
    float hash;
    float3 clumpCenter;
    float2 dirToClump;
    float distToClump;
    float clumpHash;
};

VertexSharedData InitializeVertexSharedData(uint instanceID)
{
    
    VertexSharedData v = (VertexSharedData) 0;
    v.spawnPosWS = _SpawnBuffer[instanceID].positionWS;
    
    int x = (v.spawnPosWS.x - _ClusterBotLeftX) / _TileSize;
    int y = (v.spawnPosWS.z - _ClusterBotLeftY) / _TileSize;

    v.groundNormalWS = _GroundNormalBuffer[x * _NumTilePerClusterSide + y];
    v.wind = _WindBuffer[x * _NumTilePerClusterSide + y]; 
    v.mask = _MaskBuffer[x * _NumTilePerClusterSide + y]; // [0,1]
    v.interaction = saturate(_InteractionTexture[int2(x, y)]);
    v.flow = normalize(_FlowTexture[int2(x, y)]);
    v.posture = _SpawnBuffer[instanceID].postureData * 2 - 1; // [-1,1]
    v.hash = _SpawnBuffer[instanceID].hash * 2 - 1; // [-1,1]
    v.clumpCenter = float3(_SpawnBuffer[instanceID].clumpInfo.x, 0, _SpawnBuffer[instanceID].clumpInfo.y);
    v.dirToClump = normalize((v.spawnPosWS).xz - _SpawnBuffer[instanceID].clumpInfo.xy);
    v.distToClump = _SpawnBuffer[instanceID].clumpInfo.z;
    v.clumpHash = _SpawnBuffer[instanceID].clumpInfo.w; // [0,1]

    return v;
}
#endif