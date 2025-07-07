using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// Implements continuous motion by wrapping the position around a range.
    /// </summary>
    public class _WrapAround : WrapAround
    {
        protected override void LateUpdate()
        {
            // Wrap the axis around the range
            var pos = this.transform.position;
            var newPos = pos;
            if (newPos[(int)Axis] < MinRange)
                newPos[(int)Axis] += MaxRange - MinRange;
            if (newPos[(int)Axis] > MaxRange)
                newPos[(int)Axis] += MinRange - MaxRange;

            Vector3 delta = newPos - pos;
            if (delta.AlmostZero() == false)
            {
                this.transform.position = newPos;

                // Handle objects driven by a Rigidbody.
                // We don't use Rigidbody.MovePosition() because it's a warp and we want to bypass interpolation.
                if (TryGetComponent<Rigidbody>(out var rb))
                    rb.position = newPos;

                // Notify any CinemachineCameras that are targeting this object
                CinemachineCore.OnTargetObjectWarped(this.transform, delta);
            }
        }
    }
}
