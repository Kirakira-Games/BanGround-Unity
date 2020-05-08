using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class BlurPass : ScriptableRenderPass
{
    private RenderTexture rt;
    private RenderTexture rt1;
    private RenderTexture rt2;
    private Material mat;
    private int blurSize;

    public BlurPass(RenderTexture rt, Material mat, uint blurSize)
    {
        this.rt = rt;
        this.mat = mat;
        this.blurSize = (int)blurSize;

        rt2 = new RenderTexture(Screen.width, Screen.height, 8);
        rt1 = new RenderTexture(Screen.width / this.blurSize, Screen.height / this.blurSize, 8);
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
// Standalone has resizeable window so we need to check it.
#if UNITY_STANDALONE || UNITY_EDITOR
            if(rt2.width != Screen.width || rt2.height != Screen.height)
            {
                rt1.Release();
                rt2.Release();
                
                rt1 = new RenderTexture(Screen.width / blurSize, Screen.height / blurSize, 8);
                rt2 = new RenderTexture(Screen.width, Screen.height, 8);
            }
#endif

            // It does not works without copying
            cmd.Blit(src, rt2);
            cmd.Blit(rt2, rt1);
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
    public uint blurSize = 2;

    public static bool Disabled = false;

    public override void Create()
    {
        m_ScriptablePass = new BlurPass(rt, mat, blurSize);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    
}