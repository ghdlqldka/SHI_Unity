// NOTE no idea if this is correct, but at least it works..
// resources: https://discussions.unity.com/t/introduction-of-render-graph-in-the-universal-render-pipeline-urp/930355

#if URP_INSTALLED

using pointcloudviewer.binaryviewer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace pointcloudviewer.urp
{
    public class PointCloudCustomPassURP : ScriptableRendererFeature
    {
        public PCRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new PCRenderPass();
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }

        public class PCRenderPass : ScriptableRenderPass
        {
            // using render graph and V3 Tiles viewer
#if UNITY_6000_0_OR_NEWER

        // why is this needed?
        private class PointCloudPassData
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // fetch from viewers list
            var viewers = PointCloudViewerTilesDX11.RegisteredViewers;
            if (viewers == null || viewers.Count == 0) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PointCloudPassData>("Point Cloud Render Pass", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PointCloudPassData data, RasterGraphContext rgContext) =>
                {
                    // loop viewers
                    for (int i = 0, l1 = viewers.Count; i < l1; i++)
                    {
                        var viewer = viewers[i];
                        // loop tiles
                        for (int j = 0, len = viewer.tilesCount; j < len; j++)
                        {
                            var tile = viewer.tiles[j];
                            if (!tile.isReady || tile.isLoading || tile.visiblePoints == 0) continue;

                            rgContext.cmd.DrawProcedural(Matrix4x4.identity, tile.material, 0, MeshTopology.Points, tile.visiblePoints);
                        }
                    }
                });
            }
        }

        // regular URP, older versions or if you need URP compatibility mode, use this
#else
            public PointCloudViewerTilesDX11[] viewers;

            CommandBuffer commandBuffer;
            private PointCloudCustomPassURP pointCloudCustomPassURP;

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                //Debug.Log("comfigure");
                commandBuffer = new CommandBuffer { name = "PointCloudViewer" };

                if (viewers == null)
                {
                    // NOTE only finds V3 (tiles viewer)
                    viewers = FindObjectsByType<PointCloudViewerTilesDX11>(sortMode: FindObjectsSortMode.None);
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                //if (viewers.Length < 1) return;
                commandBuffer.Clear();

                // loop viewers
                for (int i = 0, len1 = viewers.Length; i < len1; i++)
                {
                    // loop tiles in viewer
                    for (int j = 0, len = viewers[i].tilesCount; j < len; j++)
                    {
                        if (viewers[i].tiles[j].isReady == false || viewers[i].tiles[j].isLoading == true || viewers[i].tiles[j].visiblePoints == 0) continue;
                        commandBuffer.DrawProcedural(Matrix4x4.identity, viewers[i].tiles[j].material, 0, MeshTopology.Points, viewers[i].tiles[j].visiblePoints);
                    }
                }

                context.ExecuteCommandBuffer(commandBuffer);
            }
#endif
            public override void FrameCleanup(CommandBuffer cmd)
            {
                // ???
            }

        } // pass
    } // class
}

#endif
