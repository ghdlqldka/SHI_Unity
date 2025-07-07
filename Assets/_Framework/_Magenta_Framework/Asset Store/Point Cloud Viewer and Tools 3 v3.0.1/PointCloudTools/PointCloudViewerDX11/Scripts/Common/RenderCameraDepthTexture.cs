using UnityEngine;

namespace pointcloudviewer.extras
{
//    [ExecuteInEditMode]
    public class RenderCameraDepthTexture : MonoBehaviour
    {
        void Awake()
        {
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.depthTextureMode |= DepthTextureMode.Depth;
            }
        }
    }
}
