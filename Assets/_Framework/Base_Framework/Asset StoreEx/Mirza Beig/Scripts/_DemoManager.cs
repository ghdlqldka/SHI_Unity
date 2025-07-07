using MirzaBeig.ParticleSystems.Demos;
using UnityEngine;

namespace MirzaBeig
{
    public class _DemoManager : DemoManager
    {
        [SerializeField]
        protected _Base_Framework._CameraShake cameraShake;

        protected override void Update()
        {
            // base.Update();

            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");

            // Get targets.
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetCameraPosition.z += input.y * cameraMoveAmount;
                targetCameraPosition.z = Mathf.Clamp(targetCameraPosition.z, -6.3f, -1.0f);
            }
            else
            {
                targetCameraRotation.y += input.x * cameraRotateAmount;
                targetCameraRotation.x += input.y * cameraRotateAmount;

                targetCameraRotation.x = Mathf.Clamp(targetCameraRotation.x, cameraAngleLimits.x, cameraAngleLimits.y);
            }

            // Camera position.
            cameraTranslationTransform.localPosition = Vector3.SmoothDamp(
                cameraTranslationTransform.localPosition, targetCameraPosition, ref cameraPositionSmoothDampVelocity, 1.0f / cameraMoveSpeed, Mathf.Infinity, Time.unscaledDeltaTime);

            // Camera container rotation.

            cameraRotation = Vector3.SmoothDamp(
                cameraRotation, targetCameraRotation, ref cameraRotationSmoothDampVelocity, 1.0f / cameraRotationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);

            cameraRotationTransform.localEulerAngles = cameraRotation;

            cameraTranslationTransform.LookAt(cameraLookAtPosition);

            // if (Input.GetAxis("Mouse ScrollWheel") < 0)
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                Next();
            }
            // else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                Previous();
            }

            // Toggle UI.
            if (Input.GetKeyDown(KeyCode.U))
            {
                ui.SetActive(!ui.activeSelf);
            }

            // Switch between one-shot and looping prefabs.
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (particleMode == ParticleMode.looping)
                {
                    SetToOneshotParticleMode(true);
                }
                else
                {
                    SetToLoopingParticleMode(true);
                }
            }

            // Cycle levels.
            if (Input.GetKeyDown(KeyCode.L))
            {
                SetLevel((Level)((int)(currentLevel + 1) % System.Enum.GetNames(typeof(Level)).Length));
            }

            // Random prefab while holding key.
            else if (Input.GetKey(KeyCode.R))
            {
                /*
                if (particleMode == ParticleMode.oneshot)
                {
                    oneshotParticleSystems.randomize();
                    updateCurrentParticleSystemNameText();

                    // If also holding down, auto-spawn at random point.

                    if (Input.GetKey(KeyCode.T))
                    {
                        //oneshotParticleSystems.instantiateParticlePrefabRandom();
                    }
                }
                */
            }

            if (particleMode == ParticleMode.oneshot)
            {
                Vector3 mousePosition = Input.mousePosition;

                if (Input.GetMouseButtonDown(0))
                {
                    // CameraShake cameraShake = FindObjectOfType<CameraShake>();
                    Debug.Assert(cameraShake != null);

                    cameraShake.Add(0.2f, 5.0f, 0.2f, _Base_Framework.CameraShakeTarget.Position, _Base_Framework.CameraShakeAmplitudeCurve.FadeInOut25);
                    cameraShake.Add(4.0f, 5.0f, 0.5f, _Base_Framework.CameraShakeTarget.Rotation, _Base_Framework.CameraShakeAmplitudeCurve.FadeInOut25);

                    oneshotParticleSystems.InstantiateParticlePrefab(mousePosition, mouse.distanceFromCamera);
                }
                if (Input.GetMouseButton(1))
                {
                    oneshotParticleSystems.InstantiateParticlePrefab(mousePosition, mouse.distanceFromCamera);
                }
            }

            // Reset.
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCameraTransformTargets();
            }
        }

    }
}
