using UnityEngine;
using System.Collections.Generic;
using RTG;

namespace _Magenta_WebGL
{
    public class MWGL_DemoEx : _Magenta_Framework.DemoEx
    {
        private static string LOG_FORMAT = "<color=#443BFF><b>[MWGL_DemoEx]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

#if false //
            // Create transform gizmos
            foreach (GameObject targetObj in moveTargetObjs)
            {
                MWGL_ObjectTransformGizmo transformGizmo = MWGL_RTGizmosEngine.Instance.CreateObjectMoveGizmo() as MWGL_ObjectTransformGizmo;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);

#if DEBUG
                testGizmo = transformGizmo.Gizmo as MWGL_Gizmo;
#endif
            }

            foreach (GameObject targetObj in rotationTargetObjs)
            {
                MWGL_ObjectTransformGizmo transformGizmo = MWGL_RTGizmosEngine.Instance.CreateObjectRotationGizmo() as MWGL_ObjectTransformGizmo;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }


            foreach (GameObject targetObj in scaleTargetObjs)
            {
                MWGL_ObjectTransformGizmo transformGizmo = MWGL_RTGizmosEngine.Instance.CreateObjectScaleGizmo() as MWGL_ObjectTransformGizmo;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#endif

            foreach (GameObject targetObj in universalTargetObjs)
            {
                MWGL_ObjectTransformGizmo transformGizmo = MWGL_RTGizmosEngine.Instance.CreateObjectUniversalGizmo() as MWGL_ObjectTransformGizmo;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }

#if false //
            // Create collider gizmos
            MWGL_Gizmo gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_BoxColliderGizmo3D boxColliderGizmo = gizmo.AddBehaviour<MWGL_BoxColliderGizmo3D>();
            boxColliderGizmo.SetTargetCollider(colliderObject1.GetComponent<BoxCollider>());

            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_SphereColliderGizmo sphereColliderGizmo = gizmo.AddBehaviour<MWGL_SphereColliderGizmo>();
            sphereColliderGizmo.SetTargetCollider(colliderObject2.GetComponent<SphereCollider>());

            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_CapsuleColliderGizmo3D capsuleColliderGizmo = gizmo.AddBehaviour<MWGL_CapsuleColliderGizmo3D>();
            capsuleColliderGizmo.SetTargetCollider(colliderObject3.GetComponent<CapsuleCollider>());

            // Create light gizmos
            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_DirectionalLightGizmo3D dirLightGizmo = gizmo.AddBehaviour<MWGL_DirectionalLightGizmo3D>();
            dirLightGizmo.SetTargetLight(lightObject1.GetComponent<Light>());

            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_PointLightGizmo3D pointLightGizmo = gizmo.AddBehaviour<MWGL_PointLightGizmo3D>();
            pointLightGizmo.SetTargetLight(lightObject2.GetComponent<Light>());

            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_SpotLightGizmo3D spotLightGizmo = gizmo.AddBehaviour<MWGL_SpotLightGizmo3D>();
            spotLightGizmo.SetTargetLight(lightObject3.GetComponent<Light>());

            // Create terrain gizmos
            // Note: In order to use the terrain gizmo, you first need to click on the terrain.
            gizmo = gizmo = MWGL_RTGizmosEngine.Instance.CreateGizmo() as MWGL_Gizmo;
            MWGL_TerrainGizmo terrainGizmo = gizmo.AddBehaviour<MWGL_TerrainGizmo>();
            terrainGizmo.SetTargetTerrain(terrainObject.GetComponent<Terrain>());
#endif
        }

#if DEBUG
        protected override void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.LogFormat(LOG_FORMAT, "aaaaa");
                testGizmo.SetEnabled(!testGizmo.IsEnabled);
            }
        }
#endif
    }
}
