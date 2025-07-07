using UnityEngine;
using UnityEngine.Rendering;
using System;
using RTG;

namespace _Magenta_WebGL
{
    public class MWGL_RTGApp : _Magenta_Framework.RTGAppEx
    {
        private static string LOG_FORMAT = "<color=#FF3B00><b>[MWGL_RTGApp]</b></color> {0}";

        public static new MWGL_RTGApp Instance
        {
            get
            {
                return Get as MWGL_RTGApp;
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            DetectRenderPipeline();

            // Undo/Redo
            MWGL_RTUndoRedo.Instance.CanUndoRedo += OnCanUndoRedo;

            // Camera
            MWGL_RTFocusCamera.Instance.CanProcessInput += OnCanCameraProcessInput;
            MWGL_RTFocusCamera.Instance.CanUseScrollWheel += OnCanCameraUseScrollWheel;
            MWGL_RTCameraViewports.Get.CameraAdded += OnViewportsCameraAdded;
            MWGL_RTCameraViewports.Get.CameraRemoved += OnViewportCameraRemoved;

            // Scene
            MWGL_RTScene.Instance.RegisterHoverableSceneEntityContainer(MWGL_RTGizmosEngine.Instance);
            MWGL_RTSceneGrid.Instance.Initialize_SystemCall();

            // Gizmo engine
            MWGL_RTGizmosEngine.Instance.CanDoHoverUpdate += OnCanDoGizmoHoverUpdate;
            if (_RenderPipelineId == RenderPipelineId.Standard)
            {
                MWGL_RTGizmosEngine.Instance.CreateSceneGizmo(MWGL_RTFocusCamera.Instance.TargetCamera);
            }
            MWGL_RTGizmosEngine.Instance.AddRenderCamera(MWGL_RTFocusCamera.Instance.TargetCamera);

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
