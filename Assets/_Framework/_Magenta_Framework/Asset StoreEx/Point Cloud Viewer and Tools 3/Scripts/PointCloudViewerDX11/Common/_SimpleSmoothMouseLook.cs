// Very simple smooth mouselook for the MainCamera in Unity
// original code, by Francis R. Griffiths-Keam - www.runningdimensions.com
// http://forum.unity3d.com/threads/73117-A-Free-Simple-Smooth-Mouselook
// this is heavily modified version

using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace pointcloudviewer.extras
{
    public class _SimpleSmoothMouseLook : SimpleSmoothMouseLook
    {
        // public PointCloudViewerDX11 activeCloudViewer;
        protected _PointCloudViewerDX11 _activeCloudViewer
        {
            get
            {
                return activeCloudViewer as _PointCloudViewerDX11;
            }
        }

        protected override void Awake()
        {
            // cam = Camera.main;
            cam = this.gameObject.GetComponent<Camera>();
            Debug.Assert(cam != null);

            // Set target direction to the camera's initial orientation
            targetDirection = transform.rotation.eulerAngles;
        }

        protected override void Framing()
        {
            if (Input.GetKeyUp(zoomExtends))
            {
                // FIXME this only accepts single viewer as target..wont work with multiple ones..
                if (_activeCloudViewer != null)
                {
                    // FocusCameraOnGameObject(activeCloudViewer.GetBounds());
                    FocusCameraOnGameObject(_activeCloudViewer.CloudBounds);
                }
            }
        }

    } // class 
} // namespace