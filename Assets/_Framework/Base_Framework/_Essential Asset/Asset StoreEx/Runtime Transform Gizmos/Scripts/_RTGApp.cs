using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace RTG
{
    public class _RTGApp : RTGApp
    {
        private static string LOG_FORMAT = "<color=#FF3B00><b>[_RTGApp]</b></color> {0}";

        public static _RTGApp Instance
        {
            get
            {
                return Get as _RTGApp;
            }
        }

        // protected RenderPipelineId _renderPipelineId;
        protected RenderPipelineId _RenderPipelineId
        {
            get 
            { 
                return _renderPipelineId;
            }
            set 
            {
                if (_renderPipelineId != value)
                {
                    _renderPipelineId = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "<b>_RenderPipelineId has CHANGED : <color=orange>" + value + "</color></b>");
                }
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            DetectRenderPipeline();

            // Undo/Redo
            _RTUndoRedo.Instance.CanUndoRedo += OnCanUndoRedo;

            // Camera
            _RTFocusCamera.Instance.CanProcessInput += OnCanCameraProcessInput;
            _RTFocusCamera.Instance.CanUseScrollWheel += OnCanCameraUseScrollWheel;
            _RTCameraViewports.Get.CameraAdded += OnViewportsCameraAdded;
            _RTCameraViewports.Get.CameraRemoved += OnViewportCameraRemoved;

            // Scene
            _RTScene.Instance.RegisterHoverableSceneEntityContainer(_RTGizmosEngine.Instance);
            _RTSceneGrid.Instance.Initialize_SystemCall();

            // Gizmo engine
            _RTGizmosEngine.Instance.CanDoHoverUpdate += OnCanDoGizmoHoverUpdate;
            if (_RenderPipelineId == RenderPipelineId.Standard)
            {
                _RTGizmosEngine.Instance.CreateSceneGizmo(_RTFocusCamera.Instance.TargetCamera);
            }
            _RTGizmosEngine.Instance.AddRenderCamera(_RTFocusCamera.Instance.TargetCamera);

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

        /*
        protected void Invoke_Initialized()
        {
            if (Initialized != null)
                Initialized();
        }
        */

        protected override void Update()
        {
            if (_isPluginActive == false)
                return;

            // Note: Don't change the order :)
            _RTInputDevice.Instance.Update_SystemCall();
            _RTFocusCamera.Instance.Update_SystemCall();
            _RTScene.Instance.Update_SystemCall();
            _RTSceneGrid.Instance.Update_SystemCall();
            _RTGizmosEngine.Instance.Update_SystemCall();
            _RTUndoRedo.Instance.Update_SystemCall();
        }

        protected override void DetectRenderPipeline()
        {
            _renderPipelineId = RenderPipelineId.Standard - 1;
            _RenderPipelineId = RenderPipelineId.Standard;
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                RenderPipelineAsset renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                string renderPipeline = renderPipelineAsset.GetType().ToString();
                Debug.LogWarningFormat(LOG_FORMAT, "<b>renderPipeline : <color=red>" + renderPipeline + "</color></b>");
                if (renderPipeline.Contains("Universal"))
                {
                    _RenderPipelineId = RenderPipelineId.URP;
                }
                // else
                // if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
                //     _renderPipelineId = RenderPipelineId.HDRP;
                else
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "RLD: Unsupported render pipeline. Only Standard and URP pipelines are supported.");
                    Debug.Break();
                }
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<b>GraphicsSettings.currentRenderPipeline == null</b>");
            }
        }

        protected override void OnCanUndoRedo(UndoRedoOpType undoRedoOpType, YesNoAnswer answer)
        {
            if (_RTGizmosEngine.Instance.DraggedGizmo == null) 
                answer.Yes();
            else 
                answer.No();
        }

        protected override void OnCanCameraProcessInput(YesNoAnswer answer)
        {
            if (_RTGizmosEngine.Instance.DraggedGizmo != null)
                answer.No();
            else
                answer.Yes();
        }

        protected override void OnCanCameraUseScrollWheel(YesNoAnswer answer)
        {
            if (_RTScene.Instance.IsAnyUIElementHovered())
                answer.No();
            else
                answer.Yes();
        }

        protected override void OnViewportsCameraAdded(Camera camera)
        {
            _RTGizmosEngine.Instance.AddRenderCamera(camera);
        }

        protected override void OnViewportCameraRemoved(Camera camera)
        {
            _RTGizmosEngine.Instance.RemoveRenderCamera(camera);
        }
    }
}
