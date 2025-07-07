using UnityEngine;
using System.Collections.Generic;
using RTG;

namespace _Magenta_Framework
{
    public class DemoEx : RTG._Demo
    {
        private static string LOG_FORMAT = "<color=#443BFF><b>[DemoEx]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // Create transform gizmos
            foreach (GameObject targetObj in moveTargetObjs)
            {
                ObjectTransformGizmoEx transformGizmo = RTGizmosEngineEx.Instance.CreateObjectMoveGizmo() as ObjectTransformGizmoEx;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);

#if DEBUG
                testGizmo = transformGizmo.Gizmo as GizmoEx;
#endif
            }

            foreach (GameObject targetObj in rotationTargetObjs)
            {
                ObjectTransformGizmoEx transformGizmo = RTGizmosEngineEx.Instance.CreateObjectRotationGizmo() as ObjectTransformGizmoEx;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }


            foreach (GameObject targetObj in scaleTargetObjs)
            {
                ObjectTransformGizmoEx transformGizmo = RTGizmosEngineEx.Instance.CreateObjectScaleGizmo() as ObjectTransformGizmoEx;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }


            foreach (GameObject targetObj in universalTargetObjs)
            {
                ObjectTransformGizmoEx transformGizmo = RTGizmosEngineEx.Instance.CreateObjectUniversalGizmo() as ObjectTransformGizmoEx;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }

            // Create collider gizmos
            GizmoEx gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            BoxColliderGizmo3DEx boxColliderGizmo = gizmo.AddBehaviour<BoxColliderGizmo3DEx>();
            boxColliderGizmo.SetTargetCollider(colliderObject1.GetComponent<BoxCollider>());

            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            SphereColliderGizmoEx sphereColliderGizmo = gizmo.AddBehaviour<SphereColliderGizmoEx>();
            sphereColliderGizmo.SetTargetCollider(colliderObject2.GetComponent<SphereCollider>());

            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            CapsuleColliderGizmo3DEx capsuleColliderGizmo = gizmo.AddBehaviour<CapsuleColliderGizmo3DEx>();
            capsuleColliderGizmo.SetTargetCollider(colliderObject3.GetComponent<CapsuleCollider>());

            // Create light gizmos
            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            DirectionalLightGizmo3DEx dirLightGizmo = gizmo.AddBehaviour<DirectionalLightGizmo3DEx>();
            dirLightGizmo.SetTargetLight(lightObject1.GetComponent<Light>());

            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            PointLightGizmo3DEx pointLightGizmo = gizmo.AddBehaviour<PointLightGizmo3DEx>();
            pointLightGizmo.SetTargetLight(lightObject2.GetComponent<Light>());

            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            SpotLightGizmo3DEx spotLightGizmo = gizmo.AddBehaviour<SpotLightGizmo3DEx>();
            spotLightGizmo.SetTargetLight(lightObject3.GetComponent<Light>());

            // Create terrain gizmos
            // Note: In order to use the terrain gizmo, you first need to click on the terrain.
            gizmo = gizmo = RTGizmosEngineEx.Instance.CreateGizmo() as GizmoEx;
            TerrainGizmoEx terrainGizmo = gizmo.AddBehaviour<TerrainGizmoEx>();
            terrainGizmo.SetTargetTerrain(terrainObject.GetComponent<Terrain>());
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
