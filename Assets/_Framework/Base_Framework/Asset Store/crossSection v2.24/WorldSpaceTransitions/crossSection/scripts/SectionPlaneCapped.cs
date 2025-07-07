using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class SectionPlaneCapped : MonoBehaviour
    {
        public GameObject toBeSectioned;
        [SerializeField]
        private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        private Transform gizmo;
        private Vector3 boundsCenterLocal;

        void Start()
        {
#pragma warning disable 0618
            GizmoFollow g = (GizmoFollow)FindObjectOfType(typeof(GizmoFollow));
#pragma warning restore 0618
            if (g)
            {
                gizmo = g.transform;
            }
            this.enabled = g;

            //Shader.SetGlobalVector("_SectionDirX", Vector3.right);
            //Shader.SetGlobalVector("_SectionDirY", Vector3.up);
            //Shader.SetGlobalVector("_SectionDirZ", Vector3.forward);
            Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
            Shader.SetGlobalColor("_SectionColor", Color.black);

        }

        void Update()
        {
            if (gizmo == null) Start();
            transform.rotation = gizmo.rotation * Quaternion.Euler(180, 0, 0);
            Plane plane = new Plane(gizmo.forward, gizmo.position);
            //Ray ray = new Ray(bounds.center, gizmo.forward);
            Ray ray = new Ray(toBeSectioned.transform.TransformPoint(boundsCenterLocal), gizmo.forward);
            //Debug.DrawRay(toBeSectioned.transform.position, gizmo.forward, Color.green,10);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                transform.position = ray.GetPoint(rayDistance);
                //Debug.Log(rayDistance.ToString());
                //Debug.DrawLine(toBeSectioned.transform.position, ray.GetPoint(rayDistance), Color.red);
            }
            else
            {
                ray = new Ray(bounds.center, -gizmo.forward);
                //ray = new Ray(toBeSectioned.transform.position - toBeSectioned.transform.rotation * boundsOffset, -gizmo.forward);
                ray = new Ray(toBeSectioned.transform.TransformPoint(boundsCenterLocal), - gizmo.forward);
                if (plane.Raycast(ray, out rayDistance)) { 
                    transform.position = ray.GetPoint(rayDistance);
                    //Debug.DrawLine(toBeSectioned.transform.position, ray.GetPoint(rayDistance), Color.red);
                    Debug.Log(rayDistance.ToString());
                }
            }

        }


#if UNITY_EDITOR
        void OnValidate()
        {
            if (toBeSectioned)
            {
                bounds = SectionSetup.GetBounds(toBeSectioned);
                transform.localScale = Vector3.one * bounds.size.magnitude;
                transform.position = bounds.center;
                boundsCenterLocal = toBeSectioned.transform.InverseTransformPoint(bounds.center);
                //boundsOffset = bounds.center - toBeSectioned.transform.position;
            }
        }
#endif
    }
}
