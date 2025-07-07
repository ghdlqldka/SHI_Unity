using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    public class _BlendStyleManager : BlendStyleManager
    {
        private static string LOG_FORMAT = "<color=#2FE75A><b>[_BlendStyleManager]</b></color> {0}";

        public override void OnBlendCreated(CinemachineCore.BlendEventParams evt)
        {
            Debug.LogFormat(LOG_FORMAT, "OnBlendCreated()");

            // Override the blender with a custom blender
            if (m_UseCustomBlend == true)
            {
                evt.Blend.CustomBlender = m_CustomBlender;
            }
        }

        public override void DefaultBlend()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClicklDefaultBlend()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClicklDefaultBlend()");

            m_UseCustomBlend = false;
            ChangeCamera();
        }

        public override void CustomBlend()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickCustomBlend()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCustomBlend()");

            m_UseCustomBlend = true;
            ChangeCamera();
        }

        protected override void ChangeCamera()
        {
            // Cycle through all the virtual cameras, assuming that they all have the same priority.
            // Prioritize the least-recently used one.
            int numCameras = CinemachineCore.VirtualCameraCount;
            Debug.LogFormat(LOG_FORMAT, "ChangeCamera(), numCameras : " + numCameras);
            CinemachineVirtualCameraBase cinemachineVirtualCamera = CinemachineCore.GetVirtualCamera(numCameras - 1);
            cinemachineVirtualCamera.Prioritize();
        }
    }
}