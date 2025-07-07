// this is for V1, V2 point cloud viewer
// TODO could use this? https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@16.0/manual/Global-Custom-Pass-API.html

#if HDRP_INSTALLED
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using pointcloudviewer.binaryviewer;

namespace pointcloudviewer.hdrp
{
    class PointCloudCustomPassHDRP : CustomPass
    {
        public PointCloudViewerDX11[] viewers;

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {

        }

        protected override void Execute(CustomPassContext ctx)
        {
            // Executed every frame for all the camera inside the pass volume.
            // The context contains the command buffer to use to enqueue graphics commands.
            if (viewers == null) return;

            // loop viewers
            for (int i = 0, len = viewers.Length; i < len; i++)
            {
                var viewer = viewers[i];
                if (viewer == null || viewer.isReady == false) continue;
                ctx.cmd.DrawProcedural(Matrix4x4.identity, viewer.cloudMaterial, 0, MeshTopology.Points, viewer.totalMaxPoints);
            }
        }

        protected override void Cleanup()
        {
            // Cleanup code
        }
    }
}
#endif