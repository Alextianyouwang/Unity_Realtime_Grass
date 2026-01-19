
#ifndef PREFIX_SUM_INCLUDE
#define PREFIX_SUM_INCLUDE

// NOTE: #pragma kernel declarations must be in the main .compute file, not in includes.
// Unity 6000.2+ no longer processes kernel declarations from included files.
// Add these lines to your .compute file BEFORE including this file:
//   #pragma kernel ScanThreadGroup
//   #pragma kernel ScanGroup

#define NUM_THREAD_GROUP 256
#define LOG_NUM_THREAD_GROUP (uint)log2 (NUM_THREAD_GROUP)
#define NUM_GROUP 1024
#define LOG_NUM_GROUP (uint)log2 (NUM_GROUP)


groupshared int temp[2 * NUM_THREAD_GROUP * 2 - 2];
groupshared int tempGroup[2 * NUM_GROUP * 2 - 2];

RWStructuredBuffer<uint> _VoteBuffer;
RWStructuredBuffer<uint> _ScanBuffer;
RWStructuredBuffer<uint> _GroupScanBufferIn;
RWStructuredBuffer<uint> _GroupScanBufferOut;




[numthreads(NUM_THREAD_GROUP, 1, 1)]
void ScanThreadGroup(uint3 id : SV_DispatchThreadID, uint3 groupTID : SV_GroupThreadID, uint3 groupID : SV_GroupID)
{
    temp[2 * groupTID.x] = _VoteBuffer[2 * id.x];
    temp[2 * groupTID.x + 1] = _VoteBuffer[2 * id.x + 1];
    uint d;
    uint end = NUM_THREAD_GROUP * 2;
    uint start = 0;
    [unroll]
    for (d = LOG_NUM_THREAD_GROUP; d >= 1; d--)
    {
        GroupMemoryBarrierWithGroupSync();

        uint depth = 1 << d;
        uint active = groupTID.x < depth;
        temp[end + groupTID.x] = (temp[2 * groupTID.x + start] + temp[2 * groupTID.x + 1 + start]) * active;
            
        start = end;
        end += depth;

    }
    GroupMemoryBarrierWithGroupSync();
    if (groupTID.x == 0)
    {
        _GroupScanBufferIn[groupID.x] = temp[end - 1] + temp[end - 2];
        temp[end - 1] = temp[end - 2];
        temp[end - 2] = 0;
    }
    [unroll]
    for (d = 1; d <= LOG_NUM_THREAD_GROUP; d++)
    {
        GroupMemoryBarrierWithGroupSync();
        uint depth = 1 << d;
        end -= depth;
        start -= depth * 2;
        uint active = groupTID.x < depth;

        uint s = temp[start + groupTID.x * 2];
        uint s2 = temp[start + groupTID.x * 2 + 1];
        uint e = temp[end + groupTID.x];
        temp[start + groupTID.x * 2] = active ? e : s;
        temp[start + groupTID.x * 2 + 1] = active ? s + e : s2;
    }
    GroupMemoryBarrierWithGroupSync();
    
    _ScanBuffer[2 * id.x] = temp[2 * groupTID.x];
    _ScanBuffer[2 * id.x + 1] = temp[2 * groupTID.x + 1];
   
}
[numthreads(NUM_GROUP, 1, 1)]
void ScanGroup(uint3 id : SV_DispatchThreadID, uint3 groupTID : SV_GroupThreadID, uint3 groupID : SV_GroupID)
{
    tempGroup[2 * id.x] = _GroupScanBufferIn[2 * id.x];
    tempGroup[2 * id.x + 1] = _GroupScanBufferIn[2 * id.x + 1];
    uint d;
    uint end = NUM_GROUP * 2;
    uint start = 0;
    [unroll]
    for (d = LOG_NUM_GROUP; d >= 1; d--)
    {
        GroupMemoryBarrierWithGroupSync();

        uint depth = 1 << d;
        uint active = id.x < depth;
        tempGroup[end + id.x] = (tempGroup[2 * id.x + start] + tempGroup[2 * id.x + 1 + start]) * active;
            
        start = end;
        end += depth;

    }
    GroupMemoryBarrierWithGroupSync();
    if (id.x == 0)
    {
        tempGroup[end - 1] = tempGroup[end - 2];
        tempGroup[end - 2] = 0;
    }
    [unroll]
    for (d = 1; d <= LOG_NUM_GROUP; d++)
    {
        GroupMemoryBarrierWithGroupSync();
        uint depth = 1 << d;
        end -= depth;
        start -= depth * 2;
        uint active = id.x < depth;

        uint s = tempGroup[start + id.x * 2];
        uint s2 = tempGroup[start + id.x * 2 + 1];
        uint e = tempGroup[end + id.x];
        tempGroup[start + id.x * 2] = active ? e : s;
        tempGroup[start + id.x * 2 + 1] = active ? s + e : s2;
    }
    GroupMemoryBarrierWithGroupSync();
    
    _GroupScanBufferOut[2 * id.x] = tempGroup[2 * id.x];
    _GroupScanBufferOut[2 * id.x + 1] = tempGroup[2 * id.x + 1];
   
}
#endif