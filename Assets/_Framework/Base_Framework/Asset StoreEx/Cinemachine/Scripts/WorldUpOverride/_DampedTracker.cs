using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// Will match a GameObject's position and rotation to a target's position
    /// and rotation, with damping
    /// </summary>
    [ExecuteAlways]
    public class _DampedTracker : DampedTracker
    {
        protected override void OnEnable()
        {
            if (Target != null)
                this.transform.SetPositionAndRotation(Target.position, Target.rotation);
        }

        protected override void LateUpdate()
        {
            if (Target != null)
            {
                // Match the player's position
                float t = Damper.Damp(1, PositionDamping, Time.deltaTime);
                Vector3 pos = Vector3.Lerp(this.transform.position, Target.position, t);

                // Rotate my transform to make my up match the target's up
                Quaternion rot = this.transform.rotation;
                t = Damper.Damp(1, RotationDamping, Time.deltaTime);
                Vector3 srcUp = this.transform.up;
                Vector3 dstUp = Target.up;
                Vector3 axis = Vector3.Cross(srcUp, dstUp);
                if (axis.sqrMagnitude > 0.001f)
                {
                    float angle = UnityVectorExtensions.SignedAngle(srcUp, dstUp, axis) * t;
                    rot = Quaternion.AngleAxis(angle, axis) * rot;
                }
                this.transform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
