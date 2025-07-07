using UnityEngine;
using System;
using RTG;

namespace _Magenta_WebGL
{
    [Serializable]
    public class MWGL_Gizmo : _Magenta_Framework.GizmoEx
    {
        // private static string LOG_FORMAT = "<color=#AA3B00><b>[GizmoEx]</b></color> {0}";

        public MWGL_Gizmo(bool dummy) : base(dummy)
        {
            // dummy constructor!!!!!
        }

        public MWGL_Gizmo(MWGL_RTGApp app) : base(true)
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

            if (behaviourType == typeof(MWGL_MoveGizmo))
            {
                _moveGizmo = behaviour as MWGL_MoveGizmo;
            }
            else if (behaviourType == typeof(MWGL_RotationGizmo))
            {
                _rotationGizmo = behaviour as MWGL_RotationGizmo;
            }
            else if (behaviourType == typeof(MWGL_ScaleGizmo))
            {
                _scaleGizmo = behaviour as MWGL_ScaleGizmo;
            }
            else if (behaviourType == typeof(MWGL_UniversalGizmo))
            {
                _universalGizmo = behaviour as MWGL_UniversalGizmo;
            }
            else if (behaviourType == typeof(MWGL_SceneGizmo))
            {
                _sceneGizmo = behaviour as MWGL_SceneGizmo;
            }
            else if (behaviourType == typeof(MWGL_ObjectTransformGizmo))
            {
                _objectTransformGizmo = behaviour as MWGL_ObjectTransformGizmo;
            }
            else if (behaviourType == typeof(MWGL_BoxColliderGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_SphereColliderGizmo))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_CapsuleColliderGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_DirectionalLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_SpotLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_PointLightGizmo3D))
            {
                //
            }
            else if (behaviourType == typeof(MWGL_TerrainGizmo))
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
