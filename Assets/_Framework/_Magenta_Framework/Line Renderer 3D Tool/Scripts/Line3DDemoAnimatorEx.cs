using System;
using UnityEngine;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class Line3DDemoAnimatorEx : EA.Line3D.Demo._Line3DDemoAnimator
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[Line3DDemoAnimatorEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }

        protected override void Update()
        {
#if false //
            if (vpoints == null || vpoints.Length != points.Length)
            {
                Array.Resize(ref vpoints, points.Length);
            }
#endif

            lineRenderer3D.PointsCount = usedPointCount;

            int count = Mathf.Min(usedPointIndex + usedPointCount, pointTransformArray.Length);
            int index;
            Vector3 _point;
            for (int a = usedPointIndex; a < count; a++)
            {
                index = a - usedPointIndex;
                if (localSpace == true)
                {
                    _point = pointTransformArray[a].localPosition;
                }
                else
                {
                    _point = pointTransformArray[a].position;
                }
                
                lineRenderer3D.SetPoint(index, _point);
            }
        }
    }
}