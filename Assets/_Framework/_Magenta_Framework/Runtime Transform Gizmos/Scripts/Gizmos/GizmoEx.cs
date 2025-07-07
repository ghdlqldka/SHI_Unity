using UnityEngine;
using System;
using RTG;

namespace _Magenta_Framework
{
    [Serializable]
    public class GizmoEx : _Gizmo
    {
        // private static string LOG_FORMAT = "<color=#AA3B00><b>[GizmoEx]</b></color> {0}";

        public GizmoEx(bool dummy) : base(dummy)
        {
            // dummy constructor!!!!!
        }

        public GizmoEx(RTGAppEx app) : base(true)
        {
            // Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!!!!!!!!");

            _handles = new GizmoHandleCollection(this);

            _hoverInfo.Reset();
            _dragInfo.Reset();
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

            if (behaviourType == typeof(MoveGizmoEx))
            {
                _moveGizmo = behaviour as MoveGizmoEx;
            }
            else if (behaviourType == typeof(RotationGizmoEx))
            {
                _rotationGizmo = behaviour as RotationGizmoEx;
            }
            else if (behaviourType == typeof(ScaleGizmoEx))
            {
                _scaleGizmo = behaviour as ScaleGizmoEx;
            }
            else if (behaviourType == typeof(UniversalGizmoEx))
            {
                _universalGizmo = behaviour as UniversalGizmoEx;
            }
            else if (behaviourType == typeof(SceneGizmoEx))
            {
                _sceneGizmo = behaviour as SceneGizmoEx;
            }
            else if (behaviourType == typeof(ObjectTransformGizmoEx))
            {
                _objectTransformGizmo = behaviour as ObjectTransformGizmoEx;
            }
            else if (behaviourType == typeof(BoxColliderGizmo3DEx))
            {
                //
            }
            else if (behaviourType == typeof(SphereColliderGizmoEx))
            {
                //
            }
            else if (behaviourType == typeof(CapsuleColliderGizmo3DEx))
            {
                //
            }
            else if (behaviourType == typeof(DirectionalLightGizmo3DEx))
            {
                //
            }
            else if (behaviourType == typeof(SpotLightGizmo3DEx))
            {
                //
            }
            else if (behaviourType == typeof(PointLightGizmo3DEx))
            {
                //
            }
            else if (behaviourType == typeof(TerrainGizmoEx))
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
    }
}
