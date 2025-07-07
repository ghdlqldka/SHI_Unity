using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Unity.Cinemachine.Samples
{
    
    public class _SimplePlayerController : SimplePlayerController
    {
        private static string LOG_FORMAT = "<color=#F6CC5A><b>[_SimplePlayerController]</b></color> {0}";

        // public Camera Camera => CameraOverride == null ? Camera.main : CameraOverride;
        public new Camera Camera
        {
            get
            {
                return CameraOverride;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.AssertFormat(CameraOverride != null, "CameraOverride is NOT SET!!!!!! in UnityEditor");
        }

        protected override void Update()
        {
            PreUpdate?.Invoke();

            // Process Jump and gravity
            bool justLanded = ProcessJump();

            // Get the reference frame for the input
            var rawInput = new Vector3(MoveX.Value, 0, MoveZ.Value);
            var inputFrame = GetInputFrame(Vector3.Dot(rawInput, m_LastRawInput) < 0.8f);
            m_LastRawInput = rawInput;

            // Read the input from the user and put it in the input frame
            m_LastInput = inputFrame * rawInput;
            if (m_LastInput.sqrMagnitude > 1)
                m_LastInput.Normalize();

            // Compute the new velocity and move the player, but only if not mid-jump
            if (!m_IsJumping)
            {
                m_IsSprinting = Sprint.Value > 0.5f;
                var desiredVelocity = m_LastInput * (m_IsSprinting ? SprintSpeed : Speed);
                var damping = justLanded ? 0 : Damping;
                if (Vector3.Angle(m_CurrentVelocityXZ, desiredVelocity) < 100)
                    m_CurrentVelocityXZ = Vector3.Slerp(
                        m_CurrentVelocityXZ, desiredVelocity, Damper.Damp(1, damping, Time.deltaTime));
                else
                    m_CurrentVelocityXZ += Damper.Damp(
                        desiredVelocity - m_CurrentVelocityXZ, damping, Time.deltaTime);
            }

            // Apply the position change
            ApplyMotion();

            // If not strafing, rotate the player to face movement direction
            if (!Strafe && m_CurrentVelocityXZ.sqrMagnitude > 0.001f)
            {
                var fwd = inputFrame * Vector3.forward;
                var qA = transform.rotation;
                var qB = Quaternion.LookRotation(
                    (InputForward == ForwardModes.Player && Vector3.Dot(fwd, m_CurrentVelocityXZ) < 0)
                        ? -m_CurrentVelocityXZ : m_CurrentVelocityXZ, UpDirection);
                var damping = justLanded ? 0 : Damping;
                transform.rotation = Quaternion.Slerp(qA, qB, Damper.Damp(1, damping, Time.deltaTime));
            }

            if (PostUpdate != null)
            {
                // Get local-space velocity
                var vel = Quaternion.Inverse(transform.rotation) * m_CurrentVelocityXZ;
                vel.y = m_CurrentVelocityY;
                PostUpdate(vel, m_IsSprinting ? JumpSpeed / SprintJumpSpeed : 1);
            }
        }

        protected override Quaternion GetInputFrame(bool inputDirectionChanged)
        {
            // Get the raw input frame, depending of forward mode setting
            Quaternion frame = Quaternion.identity;
            switch (InputForward)
            {
                case ForwardModes.Camera: 
                    frame = Camera.transform.rotation; 
                    break;
                case ForwardModes.Player: 
                    return transform.rotation;
                case ForwardModes.World: 
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            // Map the raw input frame to something that makes sense as a direction for the player
            var playerUp = transform.up;
            var up = frame * Vector3.up;

            // Is the player in the top or bottom hemisphere?  This is needed to avoid gimbal lock,
            // but only when the player is upside-down relative to the input frame.
            const float BlendTime = 2f;
            m_TimeInHemisphere += Time.deltaTime;
            bool inTopHemisphere = Vector3.Dot(up, playerUp) >= 0;
            if (inTopHemisphere != m_InTopHemisphere)
            {
                m_InTopHemisphere = inTopHemisphere;
                m_TimeInHemisphere = Mathf.Max(0, BlendTime - m_TimeInHemisphere);
            }

            // If the player is untilted relative to the input frmae, then early-out with a simple LookRotation
            var axis = Vector3.Cross(up, playerUp);
            if (axis.sqrMagnitude < 0.001f && inTopHemisphere)
            {
                return frame;
            }

            // Player is tilted relative to input frame: tilt the input frame to match
            var angle = UnityVectorExtensions.SignedAngle(up, playerUp, axis);
            var frameA = Quaternion.AngleAxis(angle, axis) * frame;

            // If the player is tilted, then we need to get tricky to avoid gimbal-lock
            // when player is tilted 180 degrees.  There is no perfect solution for this,
            // we need to cheat it :/
            Quaternion frameB = frameA;
            if (inTopHemisphere == false || m_TimeInHemisphere < BlendTime)
            {
                // Compute an alternative reference frame for the bottom hemisphere.
                // The two reference frames are incompatible where they meet, especially
                // when player up is pointing along the X axis of camera frame.
                // There is no one reference frame that works for all player directions.
                frameB = frame * m_Upsidedown;
                var axisB = Vector3.Cross(frameB * Vector3.up, playerUp);
                if (axisB.sqrMagnitude > 0.001f)
                    frameB = Quaternion.AngleAxis(180f - angle, axisB) * frameB;
            }
            // Blend timer force-expires when user changes input direction
            if (inputDirectionChanged)
            {
                m_TimeInHemisphere = BlendTime;
            }

            // If we have been long enough in one hemisphere, then we can just use its reference frame
            if (m_TimeInHemisphere >= BlendTime)
            {
                return inTopHemisphere ? frameA : frameB;
            }

            // Because frameA and frameB do not join seamlessly when player Up is along X axis,
            // we blend them over a time in order to avoid degenerate spinning.
            // This will produce weird movements occasionally, but it's the lesser of the evils.
            if (inTopHemisphere)
            {
                return Quaternion.Slerp(frameB, frameA, m_TimeInHemisphere / BlendTime);
            }
            return Quaternion.Slerp(frameA, frameB, m_TimeInHemisphere / BlendTime);
        }
    }
}
