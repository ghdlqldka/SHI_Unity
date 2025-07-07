using System.Collections.Generic;
using UnityEngine;

namespace RTG
{
    public class _RTGizmosEngine : RTGizmosEngine
    {
        private static string LOG_FORMAT = "<color=#E31BB1><b>[_RTGizmosEngine]</b></color> {0}";

        public static _RTGizmosEngine Instance
        {
            get
            {
                return Get as _RTGizmosEngine;
            }
        }

        // protected List<Gizmo> _gizmos = new List<Gizmo>();
        protected List<Gizmo> _gizmoList
        {
            get
            {
                return _gizmos;
            }
        }
#if false //
        [SerializeField]
        protected new EditorToolbar _mainToolbar = new EditorToolbar
        (
            new EditorToolbarTab[]
            {
                new EditorToolbarTab("_General", "General gizmo engine settings."),
                new EditorToolbarTab("Scene gizmo", "Scene gizmo specific settings."),
                new EditorToolbarTab("Move gizmo", "Allows you to change move gizmo settings."),
                new EditorToolbarTab("Rotation gizmo", "Allows you to change rotation settings."),
                new EditorToolbarTab("Scale gizmo", "Allows you to change scale gizmo settings."),
                new EditorToolbarTab("Universal gizmo", "Allows you to change universal gizmo settings."),
            },
            6, Color.green
        );
#endif

#if DEBUG
        [Header("=======> DEBUG <=======")]
        [ReadOnly]
        [SerializeField]
        protected List<_Gizmo> DEBUG_gizmoList = new List<_Gizmo>();
#endif

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        /*
        protected void Invoke_CanDoHoverUpdate(YesNoAnswer answer)
        {
            if (CanDoHoverUpdate != null)
                CanDoHoverUpdate(answer);
        }
        */

        public override void Update_SystemCall()
        {
            // base.Update_SystemCall();

            foreach (RTSceneGizmoCamera sceneGizmoCam in _sceneGizmoCameras)
            {
                sceneGizmoCam.Update_SystemCall();
            }

            _pipelineStage = GizmosEnginePipelineStage.Update;
            IInputDevice inputDevice = RTInputDevice.Get.Device;
            bool deviceHasPointer = inputDevice.HasPointer();
            Vector3 inputDevicePos = inputDevice.GetPositionYAxisUp();

            bool isUIHovered = RTScene.Get.IsAnyUIElementHovered();
            bool canUpdateHoverInfo = _draggedGizmo == null && !isUIHovered && Cursor.lockState == CursorLockMode.None;
            _justReleasedDrag = false;

            if (canUpdateHoverInfo)
            {
                YesNoAnswer answer = new YesNoAnswer();
#if false //
                if (CanDoHoverUpdate != null) 
                    CanDoHoverUpdate(answer);
#else
                Invoke_CanDoHoverUpdate(answer);
#endif
                if (answer.HasNo)
                    canUpdateHoverInfo = false;
            }

            //if (canUpdateHoverInfo) 
            {
                // Note: We always reset the hovered info even if the UI is hovered. This is because
                //       we want to catch special cases where the mouse cursor moves from the gizmo
                //       over to the UI. In that case, if we don't reset the hovered info, the gizmo
                //       will be affected by mouse movements when interacting with the UI.
                _hoveredGizmo = null;
                _gizmoHoverInfo.Reset();
            }

            bool isDeviceInsideFocusCamera = deviceHasPointer && RTFocusCamera.Get.IsViewportHoveredByDevice(); //RTFocusCamera.Get.TargetCamera.pixelRect.Contains(inputDevicePos);
            bool focusCameraCanRenderGizmos = IsRenderCamera(RTFocusCamera.Get.TargetCamera);
            var hoverDataCollection = new List<GizmoHandleHoverData>(10);
            foreach (Gizmo gizmo in _gizmoList)
            {
                gizmo.OnUpdateBegin_SystemCall();
                if (canUpdateHoverInfo && gizmo.IsEnabled &&
                    isDeviceInsideFocusCamera && deviceHasPointer && focusCameraCanRenderGizmos)
                {
                    var handleHoverData = GetGizmoHandleHoverData(gizmo);
                    if (handleHoverData != null) hoverDataCollection.Add(handleHoverData);
                }
            }

            GizmoHandleHoverData hoverData = null;
            if (canUpdateHoverInfo && hoverDataCollection.Count != 0)
            {
                SortHandleHoverDataCollection(hoverDataCollection, inputDevicePos);

                hoverData = hoverDataCollection[0];
                _hoveredGizmo = hoverData.Gizmo;
                _gizmoHoverInfo.HandleId = hoverData.HandleId;
                _gizmoHoverInfo.HandleDimension = hoverData.HandleDimension;
                _gizmoHoverInfo.HoverPoint = hoverData.HoverPoint;
                _gizmoHoverInfo.IsHovered = true;
            }

            foreach (Gizmo gizmo in _gizmoList)
            {
                Debug.AssertFormat(gizmo is _Gizmo, "" + gizmo.GetType());
                _Gizmo _gizmo = gizmo as _Gizmo;

                _gizmoHoverInfo.IsHovered = (_gizmo == _hoveredGizmo);
                _gizmo.UpdateHandleHoverInfo_SystemCall(_gizmoHoverInfo);

                _gizmo.HandleInputDeviceEvents_SystemCall();
                _gizmo.OnUpdateEnd_SystemCall();
            }

            _pipelineStage = GizmosEnginePipelineStage.PostUpdate;
        }

        public override Gizmo CreateGizmo()
        {
            // base.CreateGizmo();

            _Gizmo gizmo = new _Gizmo(_RTGApp.Instance);

            RegisterGizmo(gizmo);
            return gizmo;
        }

        public void RemoveGizmo(_Gizmo gizmo)
        {
            UnregisterGizmo(gizmo);
        }

        protected void RegisterGizmo(_Gizmo gizmo)
        {
            _gizmoList.Add(gizmo);
#if DEBUG
            DEBUG_gizmoList.Add(gizmo);
#endif

            gizmo.PreDragBegin += OnGizmoDragBegin;
            gizmo.PreDragEnd += OnGizmoDragEnd;
        }

        protected void UnregisterGizmo(_Gizmo gizmo)
        {
            if (_gizmoList.Remove(gizmo) == true)
            {
#if DEBUG
                DEBUG_gizmoList.Remove(gizmo);
#endif
                gizmo.PreDragBegin -= OnGizmoDragBegin;
                gizmo.PreDragEnd -= OnGizmoDragEnd;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public override MoveGizmo CreateMoveGizmo()
        {
            // Debug.LogFormat(LOG_FORMAT, "CreateMoveGizmo()");

            _Gizmo gizmo = CreateGizmo() as _Gizmo;
            _MoveGizmo moveGizmo = new _MoveGizmo();
            gizmo.AddBehaviour(moveGizmo);

            moveGizmo.SharedHotkeys = _moveGizmoHotkeys;
            moveGizmo.SharedLookAndFeel2D = _moveGizmoLookAndFeel2D;
            moveGizmo.SharedLookAndFeel3D = _moveGizmoLookAndFeel3D;
            moveGizmo.SharedSettings2D = _moveGizmoSettings2D;
            moveGizmo.SharedSettings3D = _moveGizmoSettings3D;

            return moveGizmo;
        }

        public override RotationGizmo CreateRotationGizmo()
        {
            // Debug.LogFormat(LOG_FORMAT, "CreateRotationGizmo()");

            _Gizmo gizmo = CreateGizmo() as _Gizmo;
            _RotationGizmo rotationGizmo = new _RotationGizmo();
            gizmo.AddBehaviour(rotationGizmo);

            rotationGizmo.SharedHotkeys = _rotationGizmoHotkeys;
            rotationGizmo.SharedLookAndFeel3D = _rotationGizmoLookAndFeel3D;
            rotationGizmo.SharedSettings3D = _rotationGizmoSettings3D;

            return rotationGizmo;
        }

        public override ScaleGizmo CreateScaleGizmo()
        {
            // Debug.LogFormat(LOG_FORMAT, "CreateScaleGizmo()");

            _Gizmo gizmo = CreateGizmo() as _Gizmo;
            _ScaleGizmo scaleGizmo = new _ScaleGizmo();
            gizmo.AddBehaviour(scaleGizmo);

            scaleGizmo.SharedHotkeys = _scaleGizmoHotkeys;
            scaleGizmo.SharedLookAndFeel3D = _scaleGizmoLookAndFeel3D;
            scaleGizmo.SharedSettings3D = _scaleGizmoSettings3D;

            return scaleGizmo;
        }

        public override UniversalGizmo CreateUniversalGizmo()
        {
            // Debug.LogFormat(LOG_FORMAT, "CreateUniversalGizmo()");

            _Gizmo gizmo = CreateGizmo() as _Gizmo;
            _UniversalGizmo universalGizmo = new _UniversalGizmo();
            gizmo.AddBehaviour(universalGizmo);

            universalGizmo.SharedHotkeys = _universalGizmoHotkeys;
            universalGizmo.SharedLookAndFeel2D = _universalGizmoLookAndFeel2D;
            universalGizmo.SharedLookAndFeel3D = _universalGizmoLookAndFeel3D;
            universalGizmo.SharedSettings2D = _universalGizmoSettings2D;
            universalGizmo.SharedSettings3D = _universalGizmoSettings3D;

            return universalGizmo;
        }

        public override ObjectTransformGizmo CreateObjectMoveGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateObjectMoveGizmo()");
            // base.CreateObjectMoveGizmo();

            _MoveGizmo moveGizmo = CreateMoveGizmo() as _MoveGizmo;
            _ObjectTransformGizmo gizmo = moveGizmo.Gizmo.AddBehaviour<_ObjectTransformGizmo>();
            gizmo.SetTransformChannelFlags(_ObjectTransformGizmo.Channels.Position); // Move

            gizmo.SharedSettings = _objectMoveGizmoSettings;

            return gizmo;
        }

        public override ObjectTransformGizmo CreateObjectRotationGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateObjectRotationGizmo()");

            _RotationGizmo rotationGizmo = CreateRotationGizmo() as _RotationGizmo;
            _ObjectTransformGizmo gizmo = rotationGizmo.Gizmo.AddBehaviour<_ObjectTransformGizmo>();
            gizmo.SetTransformChannelFlags(_ObjectTransformGizmo.Channels.Rotation); // Rotation

            gizmo.SharedSettings = _objectRotationGizmoSettings;

            return gizmo;
        }

        public override ObjectTransformGizmo CreateObjectScaleGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateObjectScaleGizmo()");

            _ScaleGizmo scaleGizmo = CreateScaleGizmo() as _ScaleGizmo;
            _ObjectTransformGizmo gizmo = scaleGizmo.Gizmo.AddBehaviour<_ObjectTransformGizmo>();
            gizmo.SetTransformChannelFlags(_ObjectTransformGizmo.Channels.Scale); // Scale
            gizmo.SetTransformSpace(GizmoSpace.Local); // Local
            gizmo.MakeTransformSpacePermanent();

            gizmo.SharedSettings = _objectScaleGizmoSettings;

            return gizmo;
        }

        public override ObjectTransformGizmo CreateObjectUniversalGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateObjectUniversalGizmo()");

            _UniversalGizmo universalGizmo = CreateUniversalGizmo() as _UniversalGizmo;
            _ObjectTransformGizmo gizmo = universalGizmo.Gizmo.AddBehaviour<_ObjectTransformGizmo>();
            gizmo.SetTransformChannelFlags(
                _ObjectTransformGizmo.Channels.Position | _ObjectTransformGizmo.Channels.Rotation | _ObjectTransformGizmo.Channels.Scale);

            gizmo.SharedSettings = _objectUniversalGizmoSettings;

            return gizmo;
        }

        public override SceneGizmo CreateSceneGizmo(Camera sceneCamera)
        {
            if (GetSceneGizmoByCamera(sceneCamera) != null)
                return null;

            _Gizmo gizmo = new _Gizmo(_RTGApp.Instance);
            RegisterGizmo(gizmo);

            var sceneGizmo = gizmo.AddBehaviour<_SceneGizmo>();
            sceneGizmo.SceneGizmoCamera.SceneCamera = sceneCamera;
            sceneGizmo.SharedLookAndFeel = SceneGizmoLookAndFeel;

            _sceneGizmos.Add(sceneGizmo);

            return sceneGizmo;
        }

        public override RTSceneGizmoCamera CreateSceneGizmoCamera(Camera sceneCamera, ISceneGizmoCamViewportUpdater viewportUpdater)
        {
            GameObject sceneGizmoCamObject = new GameObject(typeof(_RTSceneGizmoCamera).ToString());
            sceneGizmoCamObject.transform.parent = _RTGizmosEngine.Instance.transform;

            _RTSceneGizmoCamera sgCamera = sceneGizmoCamObject.AddComponent<_RTSceneGizmoCamera>();
            sgCamera.ViewportUpdater = viewportUpdater;
            sgCamera.SceneCamera = sceneCamera;

            _sceneGizmoCameras.Add(sgCamera);

            return sgCamera;
        }
    }
}
