// example code for getting tile and point count from V3 viewer

using PointCloudHelpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace pointcloudviewer.binaryviewer
{
    public class ViewerStats : MonoBehaviour
    {
        public PointCloudViewerTilesDX11 viewer;
        public Text statsText;
        public bool autoUpdate = false;

        private void Start()
        {
            if (statsText != null && autoUpdate == true)
            {
                StartCoroutine(StatsUpdate());
            }
        }

        void Update()
        {
            // output stats
            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("Visible tiles:" + viewer.GetVisibleTileCount() + " Visible points:" + viewer.GetVisiblePointCount() + " Total points:" + viewer.GetTotalPointCount());
            }

            // visualize tile bounds
            if (Input.GetKeyDown(KeyCode.B))
            {
                var bounds = viewer.GetAllTileBounds();
                for (int i = 0, len = bounds.Length; i < len; i++)
                {
                    PointCloudTools.DrawBounds(bounds[i], 30);
                }
            }

            // visualize tile culling spheres
            if (Input.GetKeyDown(KeyCode.N))
            {
                var spheres = viewer.GetCullingSpheres();
                for (int i = 0; i < spheres.Length; i++)
                {
                    var pos = new Vector3(spheres[i].x, spheres[i].y, spheres[i].z);
                    PointCloudTools.DrawWireSphere(pos, spheres[i].w, Random.ColorHSV(), 30);
                }
            }

        }

        IEnumerator StatsUpdate()
        {
            // wait for viewer to be ready
            while (viewer.InitIsReady() == false)
            {
                yield return new WaitForSeconds(2);
            }

            Debug.Log("Start updating stats..");

            while (true)
            {
                statsText.text = "Visible tiles: " + viewer.GetVisibleTileCount() + " Visible points: " + PointCloudTools.HumanReadableCount(viewer.GetVisiblePointCount()) + " Total points: " + PointCloudTools.HumanReadableCount(viewer.GetTotalPointCount());
                yield return new WaitForSeconds(2);
            }
        }

    }
}