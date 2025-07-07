// sample code for checking bound collisions with cloud points
// NOTE: this is quite slow and uses mainthread currently

using UnityEngine;

namespace pointcloudviewer.examples
{
    public class BoxCloudCollision : MonoBehaviour
    {
        protected PointCloudViewer.PointCloudManager pointCloudManager;
        Vector3 previousPos;

        protected Rigidbody rb;
        protected Renderer r;

        protected bool didHit = false;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            r = GetComponent<Renderer>();

            // get reference to point manager, to check collisions with
            pointCloudManager = PointCloudViewer.PointCloudManager.instance;
        }

        protected virtual void LateUpdate()
        {
            // if we hit once, we dont check collisions anymore (since we are stopped)
            if (didHit == true) return;

            // should call this check on separate thread
            if (pointCloudManager.BoundsIntersectsCloud(r.bounds) == true)
            {
                didHit = true;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }
}