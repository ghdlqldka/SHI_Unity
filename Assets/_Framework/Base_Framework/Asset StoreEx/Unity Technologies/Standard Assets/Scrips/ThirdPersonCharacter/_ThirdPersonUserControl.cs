using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (_ThirdPersonCharacter))]
    public class _ThirdPersonUserControl : ThirdPersonUserControl
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_ThirdPersonUserControl]</b></color> {0}";

        [SerializeField]
        protected Camera _camera;

        // protected ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        protected _ThirdPersonCharacter _Character
        {
            get
            {
                return m_Character as _ThirdPersonCharacter;
            }
        }

        // protected bool m_Jump;
        protected bool _Jump
        {
            get
            {
                return m_Jump;
            }
            set
            {
                if (m_Jump != value)
                {
                    m_Jump = value;
                    Debug.LogFormat(LOG_FORMAT, "m_Jump : <b><color=yellow>" + value + "</color></b>");
                }
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(_camera != null);
            m_Cam = _camera.transform;
        }

        protected override void Start()
        {
            // get the transform of the main camera
            /*
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }
            */

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<_ThirdPersonCharacter>();
        }

        protected override void Update()
        {
            if (_Jump == false)
            {
                _Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }

        // Fixed update is called in sync with physics
        protected override void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            // if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            /*
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
            */

#if !MOBILE_INPUT
            // walk speed multiplier
            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_Move *= 0.5f;
            }
#endif

            // pass all parameters to the character control script
            _Character.Move(m_Move, crouch, _Jump);
            _Jump = false;
        }
    }
}
