using UnityEngine;
using System.Collections.Generic;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// This is a custom camera manager that selects between an aiming camera child and a
    /// non-aiming camera child, depending on the value of some user input.
    ///
    /// The Aiming child is expected to have ThirdPersonFollow and ThirdPersonAim components,
    /// and to have a player as its Follow target.  The player is expected to have a
    /// SimplePlayerAimController behaviour on one of its children, to decouple aiminag and
    /// player rotation.
    /// </summary>
    [ExecuteAlways]
    public class _AimCameraRig : AimCameraRig
    {
        private static string LOG_FORMAT = "<color=#AA00C2><b>[_AimCameraRig]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), ChildCameras.Count : " + ChildCameras.Count);
            base.Start();

#if false //
            // Find the player and the aiming camera.
            // We expect to have one camera with a CinemachineThirdPersonAim component
            // whose Follow target is a player with a SimplePlayerAimController child.
            for (int i = 0; i < ChildCameras.Count; ++i)
            {
                var cam = ChildCameras[i];
                if (!cam.isActiveAndEnabled)
                    continue;
                if (AimCamera == null
                    && cam.TryGetComponent<CinemachineThirdPersonAim>(out var aim)
                    && aim.NoiseCancellation)
                {
                    AimCamera = cam;
                    var player = AimCamera.Follow;
                    if (player != null)
                        AimController = player.GetComponentInChildren<SimplePlayerAimController>();
                }
                else if (FreeCamera == null)
                    FreeCamera = cam;
            }
            if (AimCamera == null)
                Debug.LogError("AimCameraRig: no valid CinemachineThirdPersonAim camera found among children");
            if (AimController == null)
                Debug.LogError("AimCameraRig: no valid SimplePlayerAimController target found");
            if (FreeCamera == null)
                Debug.LogError("AimCameraRig: no valid non-aiming camera found among children");
#endif
        }

        [ReadOnly]
        [SerializeField]
        protected CinemachineVirtualCameraBase _debug_currentCamera;
        protected CinemachineVirtualCameraBase DEBUG_currentCamera
        {
            set
            {
                if (_debug_currentCamera != value)
                {
                    _debug_currentCamera = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "DEBUG_currentCamera has CHANGED to <b><color=red>" + value + "</color></b>");
                }
            }
        }

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
        {
            // Debug.LogFormat(LOG_FORMAT, "ChooseCurrentCamera()");
#if false //
            var oldCam = (CinemachineVirtualCameraBase)LiveChild;
            var newCam = IsAiming ? AimCamera : FreeCamera;
            if (AimController != null && oldCam != newCam)
            {
                // Set the mode of the player aim controller.
                // We want the player rotation to be copuled to the camera when aiming, otherwise not.
                AimController.PlayerRotation = IsAiming
                    ? SimplePlayerAimController.CouplingMode.Coupled
                    : SimplePlayerAimController.CouplingMode.Decoupled;
                AimController.RecenterPlayer();
            }
            return newCam;
#else
            CinemachineVirtualCameraBase _camera = base.ChooseCurrentCamera(worldUp, deltaTime);
#if DEBUG
            DEBUG_currentCamera = _camera;
#endif
            return _camera;
#endif
        }
    }
}
