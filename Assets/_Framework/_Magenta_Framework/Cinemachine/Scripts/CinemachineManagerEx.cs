using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Magenta_Framework
{
    [ExecuteAlways]
    public class CinemachineManagerEx : Unity.Cinemachine._CinemachineManager
    {
        private static string LOG_FORMAT = "<color=#FF00C2><b>[CinemachineManagerEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(cinemachineBrain != null);
            cinemachineBrain.ShowDebugText = true; // forcelly set
        }

        protected override void OnEnable()
        {
            // We check in after the physics system has had a chance to move things
            Debug.Assert(m_PhysicsCoroutine == null);
            m_PhysicsCoroutine = StartCoroutine(AfterPhysics());

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            StopCoroutine(m_PhysicsCoroutine);
            m_PhysicsCoroutine = null;
        }

        protected override void LateUpdate()
        {
            /*
            if (UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
            DoNonFixedUpdate(Time.frameCount);
        }

        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            /*
            if (Time.frameCount == m_LastFrameUpdated
                    && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
        }

        protected override void OnSceneUnloaded(Scene scene)
        {
            /*
            if (Time.frameCount == m_LastFrameUpdated
                    && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
                DoNonFixedUpdate(Time.frameCount);
            */
        }

#if false //
        protected override IEnumerator AfterPhysics()
        {
            Debug.LogFormat(LOG_FORMAT, "AfterPhysics()");

            while (true)
            {
                // FixedUpdate can be called multiple times per frame
                yield return m_WaitForFixedUpdate;
                DoFixedUpdate();
            }
        }
#endif

        protected override void DoFixedUpdate()
        {
            // Debug.LogFormat(LOG_FORMAT, "DoFixedUpdate()");

            base.DoFixedUpdate();
        }

        protected override void DoNonFixedUpdate(int updateFrame)
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