using UnityEngine;

namespace _SHI_BA
{
	public class BA_BioIKCamera : __BioIK._BioIKCamera
    {
        private static string LOG_FORMAT = "<color=#EC007A><b>[BA_BioIKCamera]</b></color> {0}";

        public static new Camera _Camera
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

        protected override void Awake()
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