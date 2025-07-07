using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// When there is an active ThirdPersonFollow camera with noise cancellation,
    /// the position of this object is the aim target for the ThirdPersonAim camera.
    /// </summary>
    public class _AimTargetManager : AimTargetManager
    {
        private static string LOG_FORMAT = "<color=#F6CC5A><b>[_AimTargetManager]</b></color> {0}";

        protected override void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(SetAimTarget);
        }

        protected override void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(SetAimTarget);
        }

        protected override void SetAimTarget(CinemachineBrain brain)
        {
            m_HaveAimTarget = false;
            if (brain == null || brain.OutputCamera == null)
            {
                Debug.LogFormat(LOG_FORMAT, "@1-SetAimTarget()");
                CinemachineCore.CameraUpdatedEvent.RemoveListener(SetAimTarget);
            }
            else
            {
                CinemachineCamera liveCam;
                if (brain.ActiveVirtualCamera is CinemachineCameraManagerBase)
                {
                    CinemachineCameraManagerBase managerCam = brain.ActiveVirtualCamera as CinemachineCameraManagerBase;
                    liveCam = managerCam.LiveChild as CinemachineCamera;
                    if (liveCam != null)
                    {
                        // Debug.LogFormat(LOG_FORMAT, "@2_0-SetAimTarget(), liveCam : " + liveCam.Name);
                    }
                    else
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "@2_0-SetAimTarget(), liveCam : null");
                    }
                }
                else
                {
                    liveCam = brain.ActiveVirtualCamera as CinemachineCamera;
                    Debug.LogFormat(LOG_FORMAT, "@2_1-SetAimTarget(), liveCam : " + liveCam.Name);
                }

                if (liveCam != null)
                {
                    if (liveCam.TryGetComponent<CinemachineThirdPersonAim>(out var aim) && aim.enabled)
                    {
                        // Debug.LogFormat(LOG_FORMAT, "@2-SetAimTarget()");
                        // Set the worldspace aim target position so that we can know what gets hit
                        m_HaveAimTarget = aim.NoiseCancellation;
                        this.transform.position = aim.AimTarget;

                        // Set the screen-space hit target indicator position
                        if (AimTargetIndicator != null)
                        {
                            AimTargetIndicator.position = brain.OutputCamera.WorldToScreenPoint(this.transform.position);
                        }
                    }
                }
                else
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=red>@3-SetAimTarget()</color>");
                }
            }

            if (ReticleCanvas != null)
            {
                ReticleCanvas.enabled = m_HaveAimTarget;
            }
        }
    }
}
