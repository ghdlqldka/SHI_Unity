using System.Collections.Generic;
using RTG;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_RTGizmosEngine : _Magenta_Framework.RTGizmosEngineEx
    {
        private static string LOG_FORMAT = "<color=#AA3B00><b>[MWGL_RTGizmosEngine]</b></color> {0}";

        public static new MWGL_RTGizmosEngine Instance
        {
            get
            {
                return Get as MWGL_RTGizmosEngine;
            }
        }

        public override Gizmo CreateGizmo()
        {
            // base.CreateGizmo();

            MWGL_Gizmo gizmo = new MWGL_Gizmo(MWGL_RTGApp.Instance);

            RegisterGizmo(gizmo);
            return gizmo;
        }

        public override MoveGizmo CreateMoveGizmo()
        {
            Debug.LogFormat(LOG_FORMAT, "CreateMoveGizmo()");

            MWGL_Gizmo gizmo = CreateGizmo() as MWGL_Gizmo;
            MWGL_MoveGizmo moveGizmo = new MWGL_MoveGizmo();
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
            MWGL_Gizmo gizmo = CreateGizmo() as MWGL_Gizmo;
            MWGL_RotationGizmo rotationGizmo = new MWGL_RotationGizmo();
            gizmo.AddBehaviour(rotationGizmo);

            rotationGizmo.SharedHotkeys = _rotationGizmoHotkeys;
            rotationGizmo.SharedLookAndFeel3D = _rotationGizmoLookAndFeel3D;
            rotationGizmo.SharedSettings3D = _rotationGizmoSettings3D;

            return rotationGizmo;
        }

        public override ScaleGizmo CreateScaleGizmo()
        {
            MWGL_Gizmo gizmo = CreateGizmo() as MWGL_Gizmo;
            MWGL_ScaleGizmo scaleGizmo = new MWGL_ScaleGizmo();
            gizmo.AddBehaviour(scaleGizmo);

            scaleGizmo.SharedHotkeys = _scaleGizmoHotkeys;
            scaleGizmo.SharedLookAndFeel3D = _scaleGizmoLookAndFeel3D;
            scaleGizmo.SharedSettings3D = _scaleGizmoSettings3D;

            return scaleGizmo;
        }

        public override UniversalGizmo CreateUniversalGizmo()
        {
            MWGL_Gizmo gizmo = CreateGizmo() as MWGL_Gizmo;
            MWGL_UniversalGizmo universalGizmo = new MWGL_UniversalGizmo();
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

            MWGL_MoveGizmo moveGizmo = CreateMoveGizmo() as MWGL_MoveGizmo;
            MWGL_ObjectTransformGizmo transformGizmo = moveGizmo.Gizmo.AddBehaviour<MWGL_ObjectTransformGizmo>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Position); // Move

            transformGizmo.SharedSettings = _objectMoveGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectRotationGizmo()
        {
            MWGL_RotationGizmo rotationGizmo = CreateRotationGizmo() as MWGL_RotationGizmo;
            MWGL_ObjectTransformGizmo transformGizmo = rotationGizmo.Gizmo.AddBehaviour<MWGL_ObjectTransformGizmo>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Rotation); // Rotation

            transformGizmo.SharedSettings = _objectRotationGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectScaleGizmo()
        {
            MWGL_ScaleGizmo scaleGizmo = CreateScaleGizmo() as MWGL_ScaleGizmo;
            MWGL_ObjectTransformGizmo transformGizmo = scaleGizmo.Gizmo.AddBehaviour<MWGL_ObjectTransformGizmo>();
            transformGizmo.SetTransformChannelFlags(ObjectTransformGizmo.Channels.Scale); // Scale
            transformGizmo.SetTransformSpace(GizmoSpace.Local); // Local
            transformGizmo.MakeTransformSpacePermanent();

            transformGizmo.SharedSettings = _objectScaleGizmoSettings;

            return transformGizmo;
        }

        public override ObjectTransformGizmo CreateObjectUniversalGizmo()
        {
            MWGL_UniversalGizmo universalGizmo = CreateUniversalGizmo() as MWGL_UniversalGizmo;
            MWGL_ObjectTransformGizmo transformGizmo = universalGizmo.Gizmo.AddBehaviour<MWGL_ObjectTransformGizmo>();
            transformGizmo.SetTransformChannelFlags(
                ObjectTransformGizmo.Channels.Position | ObjectTransformGizmo.Channels.Rotation | ObjectTransformGizmo.Channels.Scale);

            transformGizmo.SharedSettings = _objectUniversalGizmoSettings;

            return transformGizmo;
        }

        public override RTSceneGizmoCamera CreateSceneGizmoCamera(Camera sceneCamera, ISceneGizmoCamViewportUpdater viewportUpdater)
        {
            GameObject sceneGizmoCamObject = new GameObject(typeof(MWGL_RTSceneGizmoCamera).ToString());
            sceneGizmoCamObject.transform.parent = MWGL_RTGizmosEngine.Instance.transform;

            MWGL_RTSceneGizmoCamera sgCamera = sceneGizmoCamObject.AddComponent<MWGL_RTSceneGizmoCamera>();
            sgCamera.ViewportUpdater = viewportUpdater;
            sgCamera.SceneCamera = sceneCamera;

            _sceneGizmoCameras.Add(sgCamera);

            return sgCamera;
        }
    }
}
