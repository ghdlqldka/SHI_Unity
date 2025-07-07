using System.Collections.Generic;
using RTG;
using UnityEngine;

namespace _Magenta_Framework
{
    public class RTGizmosEngineEx : RTG._RTGizmosEngine
    {
        private static string LOG_FORMAT = "<color=#AA3B00><b>[RTGizmosEngineEx]</b></color> {0}";

        public static new RTGizmosEngineEx Instance
        {
            get
            {
                return Get as RTGizmosEngineEx;
            }
        }

        public override Gizmo CreateGizmo()
        {
            // base.CreateGizmo();

            GizmoEx gizmo = new GizmoEx(RTGAppEx.Instance);

            RegisterGizmo(gizmo);
            return gizmo;
        }

        public override MoveGizmo CreateMoveGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateMoveGizmo()");

            GizmoEx gizmo = CreateGizmo() as GizmoEx;
            MoveGizmoEx moveGizmo = new MoveGizmoEx();
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
            GizmoEx gizmo = CreateGizmo() as GizmoEx;
            RotationGizmoEx rotationGizmo = new RotationGizmoEx();
            gizmo.AddBehaviour(rotationGizmo);

            rotationGizmo.SharedHotkeys = _rotationGizmoHotkeys;
            rotationGizmo.SharedLookAndFeel3D = _rotationGizmoLookAndFeel3D;
            rotationGizmo.SharedSettings3D = _rotationGizmoSettings3D;

            return rotationGizmo;
        }

        public override ScaleGizmo CreateScaleGizmo()
        {
            GizmoEx gizmo = CreateGizmo() as GizmoEx;
            ScaleGizmoEx scaleGizmo = new ScaleGizmoEx();
            gizmo.AddBehaviour(scaleGizmo);

            scaleGizmo.SharedHotkeys = _scaleGizmoHotkeys;
            scaleGizmo.SharedLookAndFeel3D = _scaleGizmoLookAndFeel3D;
            scaleGizmo.SharedSettings3D = _scaleGizmoSettings3D;

            return scaleGizmo;
        }

        public override UniversalGizmo CreateUniversalGizmo()
        {
            GizmoEx gizmo = CreateGizmo() as GizmoEx;
            UniversalGizmoEx universalGizmo = new UniversalGizmoEx();
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
            // base.CreateObjectMoveGizmo();

            MoveGizmoEx moveGizmo = CreateMoveGizmo() as MoveGizmoEx;
            ObjectTransformGizmoEx transformGizmo = moveGizmo.Gizmo.AddBehaviour<ObjectTransformGizmoEx>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Position); // Move

            transformGizmo.SharedSettings = _objectMoveGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectRotationGizmo()
        {
            RotationGizmoEx rotationGizmo = CreateRotationGizmo() as RotationGizmoEx;
            ObjectTransformGizmoEx transformGizmo = rotationGizmo.Gizmo.AddBehaviour<ObjectTransformGizmoEx>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Rotation); // Rotation

            transformGizmo.SharedSettings = _objectRotationGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectScaleGizmo()
        {
            ScaleGizmoEx scaleGizmo = CreateScaleGizmo() as ScaleGizmoEx;
            ObjectTransformGizmoEx transformGizmo = scaleGizmo.Gizmo.AddBehaviour<ObjectTransformGizmoEx>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Scale); // Scale
            transformGizmo.SetTransformSpace(GizmoSpace.Local); // Local
            transformGizmo.MakeTransformSpacePermanent();

            transformGizmo.SharedSettings = _objectScaleGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectUniversalGizmo()
        {
            UniversalGizmoEx universalGizmo = CreateUniversalGizmo() as UniversalGizmoEx;
            ObjectTransformGizmoEx transformGizmo = universalGizmo.Gizmo.AddBehaviour<ObjectTransformGizmoEx>();
            transformGizmo.SetTransformChannelFlags(
                ObjectTransformGizmo.Channels.Position | ObjectTransformGizmo.Channels.Rotation | ObjectTransformGizmo.Channels.Scale);

            transformGizmo.SharedSettings = _objectUniversalGizmoSettings;

            return transformGizmo;
        }

        public override RTSceneGizmoCamera CreateSceneGizmoCamera(Camera sceneCamera, ISceneGizmoCamViewportUpdater viewportUpdater)
        {
            GameObject sceneGizmoCamObject = new GameObject(typeof(RTSceneGizmoCameraEx).ToString());
            sceneGizmoCamObject.transform.parent = RTGizmosEngineEx.Instance.transform;

            RTSceneGizmoCameraEx sgCamera = sceneGizmoCamObject.AddComponent<RTSceneGizmoCameraEx>();
            sgCamera.ViewportUpdater = viewportUpdater;
            sgCamera.SceneCamera = sceneCamera;

            _sceneGizmoCameras.Add(sgCamera);

            return sgCamera;
        }
    }
}
