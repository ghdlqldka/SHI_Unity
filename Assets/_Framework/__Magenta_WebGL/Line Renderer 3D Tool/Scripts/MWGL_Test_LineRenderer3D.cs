using System.Collections.Generic;
using _Magenta_Framework;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_Test_LineRenderer3D : Test_LineRenderer3D_Ex
    {
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
#endif
        }
    }

}