using UnityEngine;
using UnityEngine.Rendering;
using System;
using RTG;

namespace _Magenta_Framework
{
    public class RTGAppEx : RTG._RTGApp
    {
        private static string LOG_FORMAT = "<color=#FF3B00><b>[RTGAppEx]</b></color> {0}";

        public static new RTGAppEx Instance
        {
            get
            {
                return Get as RTGAppEx;
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            DetectRenderPipeline();

            // Undo/Redo
            RTUndoRedoEx.Instance.CanUndoRedo += OnCanUndoRedo;

            // Camera
            RTFocusCameraEx.Instance.CanProcessInput += OnCanCameraProcessInput;
            RTFocusCameraEx.Instance.CanUseScrollWheel += OnCanCameraUseScrollWheel;
            RTCameraViewportsEx.Get.CameraAdded += OnViewportsCameraAdded;
            RTCameraViewportsEx.Get.CameraRemoved += OnViewportCameraRemoved;

            // Scene
            RTSceneEx.Instance.RegisterHoverableSceneEntityContainer(RTGizmosEngineEx.Instance);
            RTSceneGridEx.Instance.Initialize_SystemCall();

            // Gizmo engine
            RTGizmosEngineEx.Instance.CanDoHoverUpdate += OnCanDoGizmoHoverUpdate;
            if (_RenderPipelineId == RenderPipelineId.Standard)
            {
                RTGizmosEngineEx.Instance.CreateSceneGizmo(RTFocusCameraEx.Instance.TargetCamera);
            }
            RTGizmosEngineEx.Instance.AddRenderCamera(RTFocusCameraEx.Instance.TargetCamera);

            RTMeshCompiler.CompileEntireScene();
            if (_RenderPipelineId != RenderPipelineId.Standard)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }

            /*
            if (Initialized != null)
                Initialized();
            */
            Invoke_Initialized();
        }
    }
}
