/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWS
{
    /// <summary>
    /// Stores waypoints of a bezier path, accessed by walker objects.
    /// Provides gizmo visualization in the editor.
    /// <summary>
    public class _BezierPathHandler : BezierPathManager
    {
        private static string LOG_FORMAT = "<color=#2CE359><b>[_BezierPathHandler]</b></color> {0}";

        protected override void Awake()
        {
            if (bPoints == null || bPoints.Count == 0)
                return;

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_WaypointManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _WaypointManager.Instance._AddPath(gameObject);

            /*
            //do not recalculate automatically with runtime created paths
            if (bPoints == null || bPoints.Count == 0)
                return;
            */

            CalculatePath();
        }

    }

}