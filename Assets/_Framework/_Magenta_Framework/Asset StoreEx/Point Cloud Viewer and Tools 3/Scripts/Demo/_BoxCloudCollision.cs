// sample code for checking bound collisions with cloud points
// NOTE: this is quite slow and uses mainthread currently

using PointCloudViewer;
using UnityEngine;

namespace pointcloudviewer.examples
{
    public class _BoxCloudCollision : BoxCloudCollision
    {
        protected override void Start()
        {
            rb = GetComponent<Rigidbody>();
            r = GetComponent<Renderer>();

            // get reference to point manager, to check collisions with
            // pointCloudManager = PointCloudViewer.PointCloudManager.instance;
            pointCloudManager = null; // Do not use!!!!!
        }

        protected override void LateUpdate()
        {
            // if we hit once, we dont check collisions anymore (since we are stopped)
            if (didHit == true)
            {
                return;
            }

            // should call this check on separate thread
            // if (pointCloudManager.BoundsIntersectsCloud(r.bounds) == true)
            if (_PointCloudManager.Instance.BoundsIntersectsCloud(r.bounds) == true)
            {
                didHit = true;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }
}