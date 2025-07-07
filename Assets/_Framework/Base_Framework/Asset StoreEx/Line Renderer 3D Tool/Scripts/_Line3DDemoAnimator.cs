using System;
using UnityEngine;
using static EA.Line3D._LineRenderer3D;

namespace EA.Line3D.Demo
{
    [ExecuteInEditMode]
    public class _Line3DDemoAnimator : Line3DDemoAnimator
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[_Line3DDemoAnimator]</b></color> {0}";

        // protected LineRenderer3D line;
        protected _LineRenderer3D lineRenderer3D
        {
            get
            {
                return line as _LineRenderer3D;
            }
        }

        // protected Transform[] points;
        protected Transform[] pointTransformArray
        {
            get
            {
                return points;
            }
        }

        protected virtual void Awake()
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
            _LinePoint _point = new _LinePoint(Vector3.zero, Vector3.zero);
            for (int a = usedPointIndex; a < count; a++)
            {
                index = a - usedPointIndex;
                if (localSpace == true)
                {
                    _point.position = pointTransformArray[a].localPosition;
                }
                else
                {
                    _point.position = pointTransformArray[a].position;
                }
                
                lineRenderer3D.SetPoint(index, _point);
            }
        }
    }
}