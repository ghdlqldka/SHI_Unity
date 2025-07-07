using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace _Base_Framework
{
    public enum CameraShakeTarget
    {
        Position,
        Rotation
    }

    public enum CameraShakeAmplitudeCurve
    {
        Constant,

        // Fade in fully at 25%, 50%, and 75% of lifetime.

        FadeInOut25,
        FadeInOut50,
        FadeInOut75,
    }

    public enum CameraShakeAmplitudeOverDistanceCurve
    {
        Constant,
                
        LinearFadeIn,
        LinearFadeOut
    }

    public class _CameraShake : MonoBehaviour
    {
        public float smoothDampTime = 0.025f;

        protected Vector3 smoothDampPositionVelocity;

        protected float smoothDampRotationVelocityX;
        protected float smoothDampRotationVelocityY;
        protected float smoothDampRotationVelocityZ;

        protected List<_Shake> shakes = new List<_Shake>();

        protected virtual void Start()
        {
            //
        }

        public void Add(float amplitude, float frequency, float duration, CameraShakeTarget target, AnimationCurve amplitudeOverLifetimeCurve)
        {
            shakes.Add(new _Shake(amplitude, frequency, duration, target, amplitudeOverLifetimeCurve));
        }
        public void Add(float amplitude, float frequency, float duration, CameraShakeTarget target, CameraShakeAmplitudeCurve amplitudeOverLifetimeCurve)
        {
            shakes.Add(new _Shake(amplitude, frequency, duration, target, amplitudeOverLifetimeCurve));
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Add(0.25f, 1.0f, 2.0f, CameraShakeTarget.Position, CameraShakeAmplitudeCurve.FadeInOut25);
                Add(0.2f, 5.0f, 0.2f, CameraShakeTarget.Position, CameraShakeAmplitudeCurve.FadeInOut25);
                Add(4.0f, 5.0f, 0.5f, CameraShakeTarget.Rotation, CameraShakeAmplitudeCurve.FadeInOut25);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                Add(15.0f, 1.0f, 2.0f, CameraShakeTarget.Rotation, CameraShakeAmplitudeCurve.FadeInOut25);
            }

            Vector3 positionOffset = Vector3.zero;
            Vector3 rotationOffset = Vector3.zero;

            for (int i = 0; i < shakes.Count; i++)
            {
                shakes[i]._Update();

                if (shakes[i].target == CameraShakeTarget.Position)
                {
                    positionOffset += shakes[i].noise;
                }
                else
                {
                    rotationOffset += shakes[i].noise;
                }
            }

            shakes.RemoveAll(x => !x.IsAlive());

            this.transform.localPosition = Vector3.SmoothDamp(this.transform.localPosition, positionOffset, ref smoothDampPositionVelocity, smoothDampTime);

            Vector3 eulerAngles = this.transform.localEulerAngles;

            eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, rotationOffset.x, ref smoothDampRotationVelocityX, smoothDampTime);
            eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, rotationOffset.y, ref smoothDampRotationVelocityY, smoothDampTime);
            eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, rotationOffset.z, ref smoothDampRotationVelocityZ, smoothDampTime);

            this.transform.localEulerAngles = eulerAngles;
        }
    }
}
