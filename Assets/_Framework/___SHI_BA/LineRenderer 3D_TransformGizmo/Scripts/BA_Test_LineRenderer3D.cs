using System.Collections.Generic;
using _Magenta_Framework;
using _Magenta_WebGL;
using UnityEngine;
using static EA.Line3D._LineRenderer3D;

namespace _SHI_BA
{
    public class BA_Test_LineRenderer3D : _Magenta_Framework.Test_LineRenderer3D_Ex
    {
        protected BA_LineRenderer3D_Gizmo _lineRenderer
        {
            get
            {
                return lineRenderer as BA_LineRenderer3D_Gizmo;
            }
        }
        // Update is called once per frame
        protected override void Update()
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
                _lineRenderer.GizmoMode = !_lineRenderer.GizmoMode;
            }
#endif
        }
    }

}