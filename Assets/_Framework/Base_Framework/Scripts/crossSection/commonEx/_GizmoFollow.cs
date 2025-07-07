using UnityEngine;
using System.Collections;

namespace _Base_Framework
{
    public class _GizmoFollow : WorldSpaceTransitions.GizmoFollow
    {
        [SerializeField]
        protected Camera _cam;
        public Camera _Camera
        {
            get
            {
                return _cam;
            }
            set
            {
                _cam = value;
            }
        }

        protected override void OnDrawGizmos()
        {
            Vector3 a = _Camera.transform.position;
            Plane sPlane = new Plane(transform.forward, transform.position);
            Ray cameraRay = new Ray(a, _Camera.transform.forward);
            Vector3 b = Vector3.zero;
            float planeDist = 0;
            if (sPlane.Raycast(cameraRay, out planeDist))
            {
                b = a + planeDist * _Camera.transform.forward;
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(transform.position, b);
        }
    }
}