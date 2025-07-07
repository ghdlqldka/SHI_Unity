using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Cinemachine
{
    [ExecuteAlways]
    public class _CinemachineManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FF00C2><b>[_CinemachineManager]</b></color> {0}";

        protected readonly WaitForFixedUpdate m_WaitForFixedUpdate = new();
        protected Coroutine m_PhysicsCoroutine;

        // [ReadOnly]
        [SerializeField]
        protected CinemachineBrain cinemachineBrain;

#if DEBUG
        [Header("=====> DEBUG <=====")]
        [ReadOnly]
        [SerializeField]
        protected int DEBUG_ActiveBrainCount = int.MinValue;
        [ReadOnly]
        [SerializeField]
        protected CinemachineCameraManagerBase _debug_cmCameraManager;
        protected CinemachineCameraManagerBase DEBUG_cmCameraManager
        {
            get
            {
                return _debug_cmCameraManager;
            }
            set
            {
                if (_debug_cmCameraManager != value)
                {
                    _debug_cmCameraManager = value;
                    Debug.LogWarningFormat("<color=yellow>DEBUG_cmCameraManager has CHANGED</color>!!!!!!!, <b><color=red>" + value.name + "</color></b>");
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected CinemachineCamera _debug_activeCmCamera;
        protected CinemachineCamera DEBUG_activeCmCamera
        {
            get
            {
                return _debug_activeCmCamera;
            }
            set
            {
                if (_debug_activeCmCamera != value)
                {
                    _debug_activeCmCamera = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=yellow>ActiveCmCamera has CHANGED</color>!!!!!!!, <b><color=red>" + value.name + "</color></b>");
                }
            }
        }
#endif

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(cinemachineBrain != null);
            cinemachineBrain.ShowDebugText = true; // forcelly set

#if DEBUG
            DEBUG_ActiveBrainCount = CinemachineBrain.ActiveBrainCount;
#endif
        }

        protected virtual void OnEnable()
        {
            // We check in after the physics system has had a chance to move things
            Debug.Assert(m_PhysicsCoroutine == null);
            m_PhysicsCoroutine = StartCoroutine(AfterPhysics());

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        }

        protected virtual void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            StopCoroutine(m_PhysicsCoroutine);
            m_PhysicsCoroutine = null;
        }

        protected virtual void Start()
        {
#if DEBUG
            int numCameras = CinemachineCore.VirtualCameraCount;
            Debug.LogFormat(LOG_FORMAT, "Start(), <color=red>numCameras : <b>" + numCameras + "</b></color>");

            CinemachineVirtualCameraBase cinemachineVirtualCamera;
            for (int i = 0; i < numCameras; i++) 
            {
                cinemachineVirtualCamera = CinemachineCore.GetVirtualCamera(i);
                Debug.LogWarningFormat(LOG_FORMAT, "<color=yellow> Index : <b>" + i + 
                    "</b>, CinemachineVirtualCameraBase : <b>" + cinemachineVirtualCamera.name + "</b></color>" + 
                    ", Priority : <b>" + cinemachineVirtualCamera.Priority.Value + "</b>");
            }
#endif
        }

        protected virtual void LateUpdate()
        {
            /*
            if (UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
            DoNonFixedUpdate(Time.frameCount);
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            /*
            if (Time.frameCount == m_LastFrameUpdated
                    && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
        }

        protected virtual void OnSceneUnloaded(Scene scene)
        {
            /*
            if (Time.frameCount == m_LastFrameUpdated
                    && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
        }

        protected virtual void OnCameraUpdated(CinemachineBrain brain)
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "OnCameraUpdated(), brain : " + brain.name);

            if (brain == null || brain.OutputCamera == null)
            {
                Debug.LogFormat(LOG_FORMAT, "@1-OnCameraUpdated()");
                CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
            }
            else
            {
                CinemachineCamera liveCam;
                if (brain.ActiveVirtualCamera is CinemachineCameraManagerBase)
                {
                    CinemachineCameraManagerBase managerCam = brain.ActiveVirtualCamera as CinemachineCameraManagerBase;
#if DEBUG
                    DEBUG_cmCameraManager = managerCam;
#endif
                    liveCam = managerCam.LiveChild as CinemachineCamera;
                }
                else
                {
                    liveCam = brain.ActiveVirtualCamera as CinemachineCamera;
                }

                if (liveCam != null)
                {
#if DEBUG
                    DEBUG_activeCmCamera = liveCam;
#endif
                }
                else
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "OnCameraUpdated(), <b><color=red>liveCam == null</color></b>");
                }
            }
        }

        protected virtual IEnumerator AfterPhysics()
        {
            Debug.LogFormat(LOG_FORMAT, "AfterPhysics()");

            while (true)
            {
                // FixedUpdate can be called multiple times per frame
                yield return m_WaitForFixedUpdate;
                DoFixedUpdate();
            }
        }

        protected virtual void DoFixedUpdate()
        {
            // Debug.LogFormat(LOG_FORMAT, "DoFixedUpdate()");

            // DEBUG_activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
        }

        protected virtual void DoNonFixedUpdate(int updateFrame)
        {
#if false //
            m_LastFrameUpdated = CinemachineCore.CurrentUpdateFrame = updateFrame;

            float deltaTime = GetEffectiveDeltaTime(false);
            if (Application.isPlaying && (UpdateMethod == UpdateMethods.FixedUpdate || Time.inFixedTimeStep))
            {
                CameraUpdateManager.s_CurrentUpdateFilter = CameraUpdateManager.UpdateFilter.Fixed;

                // Special handling for fixed update: cameras that have been enabled
                // since the last physics frame must be updated now
                if (BlendUpdateMethod != BrainUpdateMethods.FixedUpdate && CinemachineCore.SoloCamera == null)
                    m_BlendManager.RefreshCurrentCameraState(DefaultWorldUp, GetEffectiveDeltaTime(true));
            }
            else
            {
                var filter = CameraUpdateManager.UpdateFilter.Late;
                if (UpdateMethod == UpdateMethods.SmartUpdate)
                {
                    // Track the targets
                    UpdateTracker.OnUpdate(UpdateTracker.UpdateClock.Late, this);
                    filter = CameraUpdateManager.UpdateFilter.SmartLate;
                }
                UpdateVirtualCameras(filter, deltaTime);
            }

            if (!Application.isPlaying || BlendUpdateMethod != BrainUpdateMethods.FixedUpdate)
                m_BlendManager.UpdateRootFrame(this, TopCameraFromPriorityQueue(), DefaultWorldUp, deltaTime);

            m_BlendManager.ComputeCurrentBlend();

            // Choose the active CinemachineCamera and apply it to the Unity camera
            if (!Application.isPlaying || BlendUpdateMethod != BrainUpdateMethods.FixedUpdate)
                ProcessActiveCamera(deltaTime);
#endif
        }
    }
}