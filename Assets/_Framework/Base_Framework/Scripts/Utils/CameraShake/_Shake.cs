using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace _Base_Framework
{
    public class _Shake
    {
        public float amplitude = 1.0f;
        public float frequency = 1.0f;

        public float duration;

        [HideInInspector]
        public CameraShakeTarget target;

        float timeRemaining;

        Vector2 perlinNoiseX;
        Vector2 perlinNoiseY;
        Vector2 perlinNoiseZ;

        [HideInInspector]
        public Vector3 noise;

        public AnimationCurve amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));

        public void Init()
        {
            timeRemaining = duration;
            ApplyRandomSeed();
        }
        void Init(float amplitude, float frequency, float duration, CameraShakeTarget target)
        {
            this.amplitude = amplitude;
            this.frequency = frequency;

            this.duration = duration;
            timeRemaining = duration;

            this.target = target;

            ApplyRandomSeed();
        }

        public void ApplyRandomSeed()
        {
            float randomRange = 32.0f;

            perlinNoiseX.x = Random.Range(-randomRange, randomRange);
            perlinNoiseX.y = Random.Range(-randomRange, randomRange);

            perlinNoiseY.x = Random.Range(-randomRange, randomRange);
            perlinNoiseY.y = Random.Range(-randomRange, randomRange);

            perlinNoiseZ.x = Random.Range(-randomRange, randomRange);
            perlinNoiseZ.y = Random.Range(-randomRange, randomRange);
        }

        public _Shake(float amplitude, float frequency, float duration, CameraShakeTarget target, AnimationCurve amplitudeOverLifetimeCurve)
        {
            Init(amplitude, frequency, duration, target);
            this.amplitudeOverLifetimeCurve = amplitudeOverLifetimeCurve;
        }

        public _Shake(float amplitude, float frequency, float duration, CameraShakeTarget target, CameraShakeAmplitudeCurve amplitudeOverLifetimeCurve)
        {
            Init(amplitude, frequency, duration, target);

            switch (amplitudeOverLifetimeCurve)
            {
                case CameraShakeAmplitudeCurve.Constant:
                    {
                        this.amplitudeOverLifetimeCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
                        break;
                    }
                case CameraShakeAmplitudeCurve.FadeInOut25:
                    {
                        this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.25f, 1.0f), new Keyframe(1.0f, 0.0f));
                        break;
                    }
                case CameraShakeAmplitudeCurve.FadeInOut50:
                    {
                        this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.50f, 1.0f), new Keyframe(1.0f, 0.0f));
                        break;
                    }
                case CameraShakeAmplitudeCurve.FadeInOut75:
                    {
                        this.amplitudeOverLifetimeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.75f, 1.0f), new Keyframe(1.0f, 0.0f));
                        break;
                    }
                default:
                    {
                        throw new System.Exception("Unknown enum.");
                    }
            }
        }

        public bool IsAlive()
        {
            return timeRemaining > 0.0f;
        }

        public virtual void _Update()
        {
            if (timeRemaining < 0.0f)
            {
                return;
            }

            Vector2 frequencyVector = Time.deltaTime * new Vector2(frequency, frequency);

            perlinNoiseX += frequencyVector;
            perlinNoiseY += frequencyVector;
            perlinNoiseZ += frequencyVector;

            noise.x = Mathf.PerlinNoise(perlinNoiseX.x, perlinNoiseX.y) - 0.5f;
            noise.y = Mathf.PerlinNoise(perlinNoiseY.x, perlinNoiseY.y) - 0.5f;
            noise.z = Mathf.PerlinNoise(perlinNoiseZ.x, perlinNoiseZ.y) - 0.5f;

            float amplitudeOverLifetime = amplitudeOverLifetimeCurve.Evaluate(1.0f - (timeRemaining / duration));

            noise *= amplitude * amplitudeOverLifetime;

            timeRemaining -= Time.deltaTime;
        }
    }
}
