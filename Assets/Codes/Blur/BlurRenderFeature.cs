using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class BlurPass : ScriptableRenderPass
{
    private RenderTexture rt;
    private RenderTexture rt1;
    private Material mat;

    public BlurPass(RenderTexture rt, RenderTexture rt1, Material mat)
    {
        this.rt = rt;
        this.rt1 = rt1;
        this.mat = mat;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        RenderTargetIdentifier src = BuiltinRenderTextureType.CameraTarget;
        RenderTargetIdentifier dst = BuiltinRenderTextureType.CurrentActive;
        cmd.Blit(src, dst);
        cmd.SetRenderTarget(dst);

        if(!BlurRenderFeature.Disabled)
        {
            // It does not works without copying
            cmd.Blit(src, rt1);
            cmd.Blit(rt1, rt, mat);
        }

        // execution
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {

    }
}

public class BlurRenderFeature : ScriptableRendererFeature
{
    BlurPass m_ScriptablePass;
    public RenderTexture rt = null;
    public Material mat = null;

    public static bool Disabled = false;

    public override void Create()
    {
        m_ScriptablePass = new BlurPass(rt, new RenderTexture(Screen.width, Screen.height, 8) , mat);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}