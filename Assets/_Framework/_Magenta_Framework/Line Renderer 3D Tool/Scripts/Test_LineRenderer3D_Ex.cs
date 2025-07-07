using System.Collections.Generic;
using _Magenta_WebGL;
using UnityEngine;
using static EA.Line3D._LineRenderer3D;

namespace _Magenta_Framework
{
    public class Test_LineRenderer3D_Ex : MonoBehaviour
    {
        [SerializeField]
        protected LineRenderer3D_Ex lineRenderer;

        [SerializeField]
        protected List<_LinePoint> pointList1;

        [SerializeField]
        protected List<_LinePoint> pointList2;

        // Update is called once per frame
        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                lineRenderer.PointList = pointList1;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                lineRenderer.PointList = pointList2;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                ((MWGL_LineRenderer3D_Gizmo)lineRenderer).GizmoMode = !((MWGL_LineRenderer3D_Gizmo)lineRenderer).GizmoMode;
            }
#endif
        }
    }

}