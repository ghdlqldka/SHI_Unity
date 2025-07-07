
using UnityEngine;

namespace MirzaBeig
{
    public class _MouseFollow : ParticleSystems.Demos.MouseFollow
    {
        [SerializeField]
        protected Camera _camera;

        protected override void Awake()
        {
            Debug.Assert(_camera != null);
        }

        protected override void Update()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = distanceFromCamera;

            Vector3 mouseScreenToWorld = _camera.ScreenToWorldPoint(mousePosition);

            float deltaTime = !ignoreTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
            Vector3 position = Vector3.Lerp(transform.position, mouseScreenToWorld, 1.0f - Mathf.Exp(-speed * deltaTime));

            transform.position = position;
        }
    }

}
