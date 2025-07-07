using UnityEngine;
using System;

namespace RTG
{
    [Serializable]
    public class _Gizmo : Gizmo
    {
        private static string LOG_FORMAT = "<color=#AA3B00><b>[_Gizmo]</b></color> {0}";

        public _Gizmo(bool dummy) : base(dummy)
        {
            // dummy constructor!!!!!
        }

        [ReadOnly]
        [SerializeField]
        protected _RTGApp app;
        public _Gizmo(_RTGApp app) : base(true)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!!!!!!!!");

            this.app = app;
            _handles = new GizmoHandleCollection(this);

            _hoverInfo.Reset();
            _dragInfo.Reset();
        }

#if false //
        protected void Invoke_PostDisabled(Gizmo gizmo)
        {
            if (PostDisabled != null)
                PostDisabled(this);
        }

        protected void Invoke_PostEnabled(Gizmo gizmo)
        {
            if (PostEnabled != null)
                PostEnabled(this);
        }
#endif

        public override void SetEnabled(bool enabled)
        {
            // base.SetEnabled(enabled);

            if (enabled == _isEnabled)
                return;

            if (enabled == false)
            {
                EndDragSession();

                _hoverInfo.Reset();
                _hoveredHandle = null;

                _isEnabled = false;

                foreach (var behaviour in _behaviours)
                    if (behaviour.IsEnabled) behaviour.OnGizmoDisabled();

                Invoke_PostDisabled(this);
                /*
                if (PostDisabled != null)
                    PostDisabled(this);
                */
            }
            else
            {
                _isEnabled = true;

                foreach (var behaviour in _behaviours)
                    if (behaviour.IsEnabled) behaviour.OnGizmoEnabled();

                Invoke_PostEnabled(this);
                /*
                if (PostEnabled != null)
                    PostEnabled(this);
                */
            }
        }

        public override bool AddBehaviour(IGizmoBehaviour behaviour)
        {
            if (behaviour == null || behaviour.Gizmo != null)
            {
                Debug.Assert(false);
                return false;
            }

            GizmoBehaviorInitParams initParams = new GizmoBehaviorInitParams();
            initParams.Gizmo = this;

            behaviour.Init_SystemCall(initParams);
            if (_behaviours.Add(behaviour) == false)
            {
                Debug.Assert(false);
                return false;
            }

            Type behaviourType = behaviour.GetType();

            if (behaviourType == typeof(_MoveGizmo))
            {
                _moveGizmo = behaviour as _MoveGizmo;
            }
            else if (behaviourType == typeof(_RotationGizmo))
            {
                _rotationGizmo = behaviour as _RotationGizmo;
            }
            else if (behaviourType == typeof(_ScaleGizmo))
            {
                _scaleGizmo = behaviour as _ScaleGizmo;
            }
            else if (behaviourType == typeof(_UniversalGizmo))
            {
                _universalGizmo = behaviour as _UniversalGizmo;
            }
            else if (behaviourType == typeof(_SceneGizmo))
            {
                _sceneGizmo = behaviour as _SceneGizmo;
            }
            else if (behaviourType == typeof(_ObjectTransformGizmo))
            {
                _objectTransformGizmo = behaviour as _ObjectTransformGizmo;
            }
            else if (behaviourType == typeof(_BoxColliderGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(_SphereColliderGizmo))
            {
                //
            }
            else if (behaviourType == typeof(_CapsuleColliderGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(_DirectionalLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(_SpotLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(_PointLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(_TerrainGizmo))
            {
                //
            }
            else
            {
                Debug.AssertFormat(false, "" + behaviourType);
            }

            behaviour.OnAttached();
            behaviour.OnEnabled();

            return true;
        }

        /*
        protected virtual void Post_UpdateHandleHoverInfo_SystemCall(bool wasHovered, int prevHoveredHandleId)
        {
            if (wasHovered && !_hoverInfo.IsHovered)
            {
                if (PreHoverExit != null)
                {
                    PreHoverExit(this, prevHoveredHandleId);
                }

                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverExit(prevHoveredHandleId);
                }

                if (PostHoverExit != null)
                {
                    PostHoverExit(this, prevHoveredHandleId);
                }
            }
            else if (!wasHovered && _hoverInfo.IsHovered)
            {
                if (PreHoverEnter != null)
                {
                    PreHoverEnter(this, _hoverInfo.HandleId);
                }
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverEnter(_hoverInfo.HandleId);
                }
                if (PostHoverEnter != null)
                    PostHoverEnter(this, _hoverInfo.HandleId);
            }
            else if (wasHovered && _hoverInfo.IsHovered)
            {
                if (prevHoveredHandleId != _hoverInfo.HandleId)
                {
                    if (PreHoverExit != null)
                        PreHoverExit(this, prevHoveredHandleId);
                    foreach (var behaviour in _behaviours)
                    {
                        if (behaviour.IsEnabled)
                            behaviour.OnGizmoHoverExit(prevHoveredHandleId);
                    }
                    if (PostHoverExit != null)
                        PostHoverExit(this, prevHoveredHandleId);
                }

                if (PreHoverEnter != null)
                    PreHoverEnter(this, _hoverInfo.HandleId);
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverEnter(_hoverInfo.HandleId);
                }
                if (PostHoverEnter != null)
                    PostHoverEnter(this, _hoverInfo.HandleId);
            }
        }
        */

        public override void UpdateHandleHoverInfo_SystemCall(GizmoHoverInfo hoverInfo)
        {
            // base.UpdateHandleHoverInfo_SystemCall(_hoverInfo);

            if (IsEnabled == false || IsDragged)
            {
                return;
            }

            bool wasHovered = _hoverInfo.IsHovered;
            int prevHoveredHandleId = _hoverInfo.HandleId;

            _hoverInfo.Reset();
            _hoveredHandle = null;

            if (hoverInfo.IsHovered && hoverInfo.HandleId != GizmoHandleId.None)
            {
                _hoverInfo.IsHovered = true;
                _hoverInfo.HandleId = hoverInfo.HandleId;
                _hoverInfo.HoverPoint = hoverInfo.HoverPoint;
                _hoveredHandle = _handles.GetHandleById(hoverInfo.HandleId);
                _hoverInfo.HandleDimension = hoverInfo.HandleDimension;
            }

#if false //
            if (wasHovered && !_hoverInfo.IsHovered)
            {
                if (PreHoverExit != null)
                    PreHoverExit(this, prevHoveredHandleId);
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverExit(prevHoveredHandleId);
                }
                if (PostHoverExit != null)
                    PostHoverExit(this, prevHoveredHandleId);
            }
            else if (!wasHovered && _hoverInfo.IsHovered)
            {
                if (PreHoverEnter != null)
                    PreHoverEnter(this, _hoverInfo.HandleId);
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverEnter(_hoverInfo.HandleId);
                }
                if (PostHoverEnter != null)
                    PostHoverEnter(this, _hoverInfo.HandleId);
            }
            else if (wasHovered && _hoverInfo.IsHovered)
            {
                if (prevHoveredHandleId != _hoverInfo.HandleId)
                {
                    if (PreHoverExit != null)
                        PreHoverExit(this, prevHoveredHandleId);
                    foreach (var behaviour in _behaviours)
                    {
                        if (behaviour.IsEnabled)
                            behaviour.OnGizmoHoverExit(prevHoveredHandleId);
                    }
                    if (PostHoverExit != null)
                        PostHoverExit(this, prevHoveredHandleId);
                }

                if (PreHoverEnter != null)
                    PreHoverEnter(this, _hoverInfo.HandleId);
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHoverEnter(_hoverInfo.HandleId);
                }
                if (PostHoverEnter != null)
                    PostHoverEnter(this, _hoverInfo.HandleId);
            }
#else
            Post_UpdateHandleHoverInfo_SystemCall(wasHovered, prevHoveredHandleId);
#endif
        }

        public override void HandleInputDeviceEvents_SystemCall()
        {
            if (IsEnabled == false)
            {
                return;
            }

            IInputDevice inputDevice = RTInputDevice.Get.Device;
            if (inputDevice.WasButtonPressedInCurrentFrame(InputDeviceDragButtonIndex))
            {
                OnInputDevicePickButtonDown();
            }
            else if (inputDevice.WasButtonReleasedInCurrentFrame(InputDeviceDragButtonIndex))
            {
                OnInputDevicePickButtonUp();
            }

            if (inputDevice.WasMoved())
            {
                OnInputDeviceMoved();
            }
        }

        protected override void OnInputDevicePickButtonDown()
        {
            /*
            if (_hoveredHandle != null)
            {
                if (PreHandlePicked != null)
                    PreHandlePicked(this, _hoveredHandle.Id);
                foreach (var behaviour in _behaviours)
                {
                    if (behaviour.IsEnabled)
                        behaviour.OnGizmoHandlePicked(_hoveredHandle.Id);
                }
                if (PostHandlePicked != null)
                    PostHandlePicked(this, _hoveredHandle.Id);

                TryActivateDragSession();
            }
            */
            base.OnInputDevicePickButtonDown();
        }

        protected override void OnInputDevicePickButtonUp()
        {
            EndDragSession();
        }
    }
}
