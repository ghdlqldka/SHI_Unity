using pointcloudviewer.binaryviewer;
using System.Collections;
using UnityEngine;

namespace PointCloudExtras
{
    public class CreateOccluders : MonoBehaviour
    {
        public PointCloudViewerTilesDX11 viewer;

        void Start()
        {
            StartCoroutine(WaitForLoadComplete());
        }

        IEnumerator WaitForLoadComplete()
        {
            while (viewer.InitIsReady() == false)// || viewer.GetLoadQueueCount() > 0)
            {
                yield return 0;
            }

            Debug.Log("Start building occluders");
            var bounds = viewer.GetAllTileBounds();

            for (int i = 0; i < bounds.Length; i++)
            {
                //PointCloudTools.DrawBounds(bounds[i], 10);
                CreateBoundsCube(bounds[i]);
            }

            Debug.Log("Done");
        }

        private void CreateBoundsCube(Bounds bounds)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = bounds.center;
            cube.transform.localScale = bounds.size;
            cube.transform.parent = transform;
        }
    }
}