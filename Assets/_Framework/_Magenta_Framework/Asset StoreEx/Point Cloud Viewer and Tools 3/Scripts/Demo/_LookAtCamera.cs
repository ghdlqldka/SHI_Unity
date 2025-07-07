// example script for making object always look towards camera

using UnityEngine;

namespace pointcloudviewer.examples
{
    public class _LookAtCamera : LookAtCamera
    {
        protected override void Start()
        {
            // cam = Camera.main.transform;
            cam = null; // Do not use!!!!!
        }

        protected override void LateUpdate()
        {
            // transform.LookAt(cam);
            this.transform.LookAt(PointCloudViewer._PointCloudManager.Instance._Camera.transform);
        }
    }
}