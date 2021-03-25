using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DualKawaseBlur : ScriptableRendererFeature
{
    public static List<BluredItem> BluredItems = new List<BluredItem>();
    public static int BluredItemsCount => BluredItems.Count;

    [System.Serializable]
    public class DualKawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        public Material blurMaterial = null;

        [Range(2,16)]
        public int blurPasses = 8;

        [Range(1,4)]
        public int downSample = 1;

        [Range(0, 4)]
        public float radius = 1.5f;

        public bool copyToFramebuffer;

        public string targetName = "_BlurTexture";
    }

    public DualKawaseBlurSettings settings = new DualKawaseBlurSettings();

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blurMaterial { get; set; }
        public int passes { get; set; }
        public int downsample { get; set; }
        public float radius { get; set; }
        public bool copyToFramebuffer { get; set; }
        public string targetName { get; set; }
        public string profilerTag { get; set; }

        RenderTargetIdentifier globalBlurRT;

        int width, height;
            
        struct Level
        {
            internal int down;
            internal int up;
        }

        Level[] m_Pyramid;

        static class Unifrom
        {
            public static readonly int Radius = Shader.PropertyToID("_Radius");
        }

        private RenderTargetIdentifier source { get; set; }

        public void Setup(RenderTargetIdentifier source) {
            this.source = source;
        }

        public void InitRTNames()
        {
            m_Pyramid = new Level[passes];

            for (int i = 0; i < passes; i++)
            {
                m_Pyramid[i] = new Level
                {
                    down = Shader.PropertyToID("_BlurMipDown" + i),
                    up = Shader.PropertyToID("_BlurMipUp" + i)
                };
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            width = cameraTextureDescriptor.width / downsample;
            height = cameraTextureDescriptor.height / downsample;

            blurMaterial.SetFloat(Unifrom.Radius, radius);

            var globalBlurRTId = Shader.PropertyToID("GlobalBlurRT");

            cmd.GetTemporaryRT(globalBlurRTId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            globalBlurRT = new RenderTargetIdentifier(globalBlurRTId);
            ConfigureTarget(globalBlurRT);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (BluredItemsCount != 0 || copyToFramebuffer)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

                int tw = width;
                int th = height;

                // Down Sample
                RenderTargetIdentifier lastDown = source;
                for (int i = 0; i < passes; i++)
                {
                    int mipDown = m_Pyramid[i].down;

                    cmd.GetTemporaryRT(mipDown, width, height, 0, FilterMode.Bilinear, renderingData.cameraData.cameraTargetDescriptor.colorFormat);
                    cmd.Blit(lastDown, mipDown, blurMaterial, 0);

                    lastDown = mipDown; 
                    tw = Mathf.Max(tw / 2, 1);
                    th = Mathf.Max(th / 2, 1);
                }

                // Up Sample
                int lastUp = m_Pyramid.Last().down;
                for (int i = m_Pyramid.Length - 2; i >= 0; i--)
                {
                    int mipUp = m_Pyramid[i].up;

                    cmd.GetTemporaryRT(mipUp, width, height, 0, FilterMode.Bilinear, renderingData.cameraData.cameraTargetDescriptor.colorFormat);
                    cmd.Blit(lastUp, mipUp, blurMaterial, 1);

                    lastUp = mipUp;
                }

                if (copyToFramebuffer)
                {
                    cmd.Blit(lastUp, source, blurMaterial, 1);
                }
                else
                {
                    cmd.Blit(lastUp, globalBlurRT);
                    cmd.SetGlobalTexture(targetName, globalBlurRT);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                CommandBufferPool.Release(cmd);
            }
        }
    }

    CustomRenderPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new CustomRenderPass
        {
            blurMaterial = settings.blurMaterial,
            passes = settings.blurPasses,
            downsample = settings.downSample,
            copyToFramebuffer = settings.copyToFramebuffer,
            targetName = settings.targetName,
            radius = settings.radius,
            renderPassEvent = settings.renderPassEvent
        };

        scriptablePass.InitRTNames();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        scriptablePass.Setup(src);
        renderer.EnqueuePass(scriptablePass);
    }
}
