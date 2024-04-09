using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
public class RenderZBuffer : ScriptableRendererFeature
{
   
    public RenderPassEvent RenderEvent = RenderPassEvent.BeforeRenderingOpaques;
    public LayerMask CullingMask;
    class ZBufferPass : ScriptableRenderPass
    {
        private FilteringSettings _filterSetting;
        private List<ShaderTagId> _shaderTagId = new List<ShaderTagId>();
        private RTHandle _zbuffer;
        public ZBufferPass(LayerMask mask) 
        {
            _filterSetting = new FilteringSettings(RenderQueueRange.opaque, mask);
            _shaderTagId.Add(new ShaderTagId("DepthOnly"));
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _zbuffer, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_CustomPassHandle");
            ConfigureTarget(_zbuffer);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            SortingCriteria sc = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings ds = CreateDrawingSettings(_shaderTagId, ref renderingData, sc);
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("HiZ_Buffer_LOD0"))) 
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                context.DrawRenderers(renderingData.cullResults, ref ds, ref _filterSetting);
            }

            cmd.SetGlobalTexture("_HiZTexture", _zbuffer);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            _zbuffer.Release();
        }
    }

    private ZBufferPass _depthTexturePass;

    public override void Create()
    {
        _depthTexturePass = new ZBufferPass(CullingMask);
        _depthTexturePass.renderPassEvent = RenderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
       if(renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(_depthTexturePass);
    }
}


