// sample script to get points inside box collider and do something with them
// you can clone this and make your own version of it


using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace PointCloudViewer.Experimental
{
    public class GetPointsInsideBoxCustom : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to PointCloudViewerTilesDX11 component")]
        public PointCloudViewerTilesDX11 viewer;

        [Tooltip("Gameobject with Box collider")]
        public Transform selectionBox;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                viewer.RunGetPointsInsideBoxThread(this, selectionBox); // nick custom
            }
        }

    }
}

