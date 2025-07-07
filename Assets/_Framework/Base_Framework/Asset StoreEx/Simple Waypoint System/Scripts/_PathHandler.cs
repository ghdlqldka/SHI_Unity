/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SWS
{
    /// <summary>
    /// Stores waypoints, accessed by walker objects.
    /// Provides gizmo visualization in the editor.
    /// <summary>
    public class _PathHandler : PathManager
    {
        private static string LOG_FORMAT = "<color=#2FF189><b>[_PathHandler]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, waypoints.Length : <b><color=magenta>" + waypoints.Length + "</color></b>");

            Debug.Assert(waypoints.Length > 0);

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_WaypointManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _WaypointManager.Instance._AddPath(this.gameObject);
        }

        protected override void OnDrawGizmos()
        {
            Debug.Assert(waypoints.Length > 0);
            /*
            if (waypoints.Length <= 0)
            {
                return;
            }
            */

            //get positions
            Vector3[] wpPositions = GetPathPoints();

            //assign path ends color
            Vector3 start = wpPositions[0];
            Vector3 end = wpPositions[wpPositions.Length - 1];
            Gizmos.color = color1;
            Gizmos.DrawWireCube(start, size * GetHandleSize(start) * 1.5f);
            Gizmos.DrawWireCube(end, size * GetHandleSize(end) * 1.5f);

            //assign line and waypoints color
            Gizmos.color = color2;
            for (int i = 1; i < wpPositions.Length - 1; i++)
            {
                Gizmos.DrawWireSphere(wpPositions[i], radius * GetHandleSize(wpPositions[i]));
            }

            //draw linear or curved lines with the same color
            if (drawCurved && wpPositions.Length >= 2)
            {
                WaypointManager.DrawCurved(wpPositions);
            }
            else
            {
                WaypointManager.DrawStraight(wpPositions);
            }
        }
    }
}


