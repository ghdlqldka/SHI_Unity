// this is for V3 tiles viewer

#if HDRP_INSTALLED
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using pointcloudviewer.binaryviewer;

namespace pointcloudviewer.hdrp
{
    class PointCloudCustomPassHDRPV3 : CustomPass
    {
        public PointCloudViewerTilesDX11[] viewers;

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {

        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (viewers == null) return;
            // Executed every frame for all the camera inside the pass volume.
            // The context contains the command buffer to use to enqueue graphics commands.

            // loop viewers
            for (int i = 0, len1 = viewers.Length; i < len1; i++)
            {
                var viewer = viewers[i];
                if (viewer == null) continue;
                // loop tiles in viewer
                for (int j = 0, len = viewer.tilesCount; j < len; j++)
                {
                    if (viewer.tiles[j].isReady == false || viewer.tiles[j].isLoading == true || viewer.tiles[j].visiblePoints == 0) continue;
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, viewer.tiles[j].material, 0, MeshTopology.Points, viewer.tiles[j].visiblePoints);
                }
            }
        }

        protected override void Cleanup()
        {
            // Cleanup code
        }
    }
}
#endif