using System;
using UnityEngine;

namespace EA.Line3D.Demo
{
    [ExecuteInEditMode]
    public class Line3DDemoAnimator : MonoBehaviour
    {
        [SerializeField] protected LineRenderer3D line;
        [Space]
        [SerializeField] protected bool localSpace;
        [SerializeField] protected int usedPointIndex;
        [SerializeField] protected int usedPointCount;
        [SerializeField] protected Transform[] points;

        protected Vector3[] vpoints;

        protected virtual void Update()
        {
            if (vpoints == null || vpoints.Length != points.Length)
                Array.Resize(ref vpoints, points.Length);

            line.PointsCount = usedPointCount;

            for (int a = usedPointIndex, c = Mathf.Min(usedPointIndex + usedPointCount, points.Length); a < c; a++)
                line.SetPoint(a - usedPointIndex, localSpace ? points[a].localPosition : points[a].position);
        }
    }
}