 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class _ThirdPersonController : ThirdPersonController
    {
        [SerializeField]
        private Camera _camera;

        protected override void Awake()
        {
            /*
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            */
            _mainCamera = _camera.gameObject;
        }

        protected override void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            // A Layer mask that is used to selectively ignore colliders when casting a capsule.
            int layMask = GroundLayers;
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, layMask, QueryTriggerInteraction.Ignore);
            // Debug.Log("Grounded : " + Grounded);

            // update animator if using character
            if (_hasAnimator == true)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
    }
}