using UnityEngine;
using System.Collections.Generic;
using WorldSpaceTransitions;

namespace RTG
{
    public class _Demo : Demo
    {
        private static string LOG_FORMAT = "<color=#443BFF><b>[_Demo]</b></color> {0}";

        [SerializeField]
        protected GameObject[] moveTargetObjs;
        [SerializeField]
        protected GameObject[] rotationTargetObjs;
        [SerializeField]
        protected GameObject[] scaleTargetObjs;
        [SerializeField]
        protected GameObject[] universalTargetObjs;

        [Space(10)]
        [SerializeField]
        protected GameObject colliderObject1; // = GameObject.Find("Cube_BoxCollider");
        [SerializeField]
        protected GameObject colliderObject2; // = GameObject.Find("Sphere_SphereCollider");
        [SerializeField]
        protected GameObject colliderObject3; // = GameObject.Find("Capsule_CapsuleCollider");

        [Space(10)]
        [SerializeField]
        protected GameObject lightObject1; // = GameObject.Find("Directional Light");
        [SerializeField]
        protected GameObject lightObject2; // = GameObject.Find("Point Light");
        [SerializeField]
        protected GameObject lightObject3; // = GameObject.Find("Spot Light");

        [Space(10)]
        [SerializeField]
        protected GameObject terrainObject; // = GameObject.Find("Terrain");

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

#if false //
            // Create transform gizmos
            string[] moveTargetNames = new string[] { "Blue Cube", "Sphere" };
            foreach (string targetName in moveTargetNames)
            {
                ObjectTransformGizmo transformGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObject });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#else
            // Create transform gizmos
            foreach (GameObject targetObj in moveTargetObjs)
            {
                ObjectTransformGizmo transformGizmo = _RTGizmosEngine.Instance.CreateObjectMoveGizmo();

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#endif

#if false //
            string[] rotationTargetNames = new string[] { "Cylinder", "Red Cube" };
            foreach (string targetName in rotationTargetNames)
            {
                var transformGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#else
            foreach (GameObject targetObj in rotationTargetObjs)
            {
                ObjectTransformGizmo transformGizmo = _RTGizmosEngine.Instance.CreateObjectRotationGizmo();

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#endif

#if false //
            string[] scaleTargetNames = new string[] { "Cylinder (1)", "Sphere (1)" };
            foreach (var targetName in scaleTargetNames)
            {
                ObjectTransformGizmo transformGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#else
            foreach (GameObject targetObj in scaleTargetObjs)
            {
                ObjectTransformGizmo transformGizmo = _RTGizmosEngine.Instance.CreateObjectScaleGizmo();

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#endif

#if false //
            string[] universalTargetNames = new string[] { "Blue Cube (1)", "Green Cube" };
            foreach (string targetName in universalTargetNames)
            {
                ObjectTransformGizmo transformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObject });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
#else
            foreach (GameObject targetObj in universalTargetObjs)
            {
                _ObjectTransformGizmo transformGizmo = _RTGizmosEngine.Instance.CreateObjectUniversalGizmo() as _ObjectTransformGizmo;

                transformGizmo.SetTargetObject(targetObj);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }

#endif

#if false //
            // Create collider gizmos
            GameObject colliderObject = GameObject.Find("Cube_BoxCollider");
            _Gizmo gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            BoxColliderGizmo3D boxColliderGizmo = gizmo.AddBehaviour<BoxColliderGizmo3D>();
            boxColliderGizmo.SetTargetCollider(colliderObject.GetComponent<BoxCollider>());
#else
            // Create collider gizmos
            _Gizmo gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _BoxColliderGizmo3D boxColliderGizmo = gizmo.AddBehaviour<_BoxColliderGizmo3D>();
            boxColliderGizmo.SetTargetCollider(colliderObject1.GetComponent<BoxCollider>());

#if DEBUG
            testGizmo = gizmo;
#endif
#endif

#if false //
            colliderObject = GameObject.Find("Sphere_SphereCollider");
            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            SphereColliderGizmo sphereColliderGizmo = gizmo.AddBehaviour<SphereColliderGizmo>();
            sphereColliderGizmo.SetTargetCollider(colliderObject.GetComponent<SphereCollider>());
#else
            gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _SphereColliderGizmo sphereColliderGizmo = gizmo.AddBehaviour<_SphereColliderGizmo>();
            sphereColliderGizmo.SetTargetCollider(colliderObject2.GetComponent<SphereCollider>());
#endif

#if false //
            colliderObject = GameObject.Find("Capsule_CapsuleCollider");
            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            CapsuleColliderGizmo3D capsuleColliderGizmo = gizmo.AddBehaviour<CapsuleColliderGizmo3D>();
            capsuleColliderGizmo.SetTargetCollider(colliderObject.GetComponent<CapsuleCollider>());
#else
            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _CapsuleColliderGizmo3D capsuleColliderGizmo = gizmo.AddBehaviour<_CapsuleColliderGizmo3D>();
            capsuleColliderGizmo.SetTargetCollider(colliderObject3.GetComponent<CapsuleCollider>());
#endif

            // Create light gizmos
            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _DirectionalLightGizmo3D dirLightGizmo = gizmo.AddBehaviour<_DirectionalLightGizmo3D>();
            dirLightGizmo.SetTargetLight(lightObject1.GetComponent<Light>());

            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _PointLightGizmo3D pointLightGizmo = gizmo.AddBehaviour<_PointLightGizmo3D>();
            pointLightGizmo.SetTargetLight(lightObject2.GetComponent<Light>());

            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _SpotLightGizmo3D spotLightGizmo = gizmo.AddBehaviour<_SpotLightGizmo3D>();
            spotLightGizmo.SetTargetLight(lightObject3.GetComponent<Light>());

            // Create terrain gizmos
            // Note: In order to use the terrain gizmo, you first need to click on the terrain.
            gizmo = gizmo = _RTGizmosEngine.Instance.CreateGizmo() as _Gizmo;
            _TerrainGizmo terrainGizmo = gizmo.AddBehaviour<_TerrainGizmo>();
            terrainGizmo.SetTargetTerrain(terrainObject.GetComponent<Terrain>());
        }

#if DEBUG
        protected _Gizmo testGizmo;

        protected virtual void Update()
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
