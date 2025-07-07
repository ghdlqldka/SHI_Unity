using UnityEngine;

namespace __BioIK
{
	public class _BioIKCamera : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FF7100><b>[_BioIKCamera]</b></color> {0}";

        protected static Camera _camera;
        public static Camera _Camera
        {
            get 
            {
                if (_camera == null)
                {
                    _camera = FindFirstObjectByType<Camera>();
                }
                return _camera;
            }
        }

#if DEBUG
        [Header("")]
        [ReadOnly]
        [SerializeField]
        protected Camera DEBUG_Camera;
#endif

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b><color=yellow>" + this.gameObject.name + "</color></b>");

            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                Debug.Assert(_camera != null);
            }
#if DEBUG
            DEBUG_Camera = _camera;
#endif
        }
    }
}