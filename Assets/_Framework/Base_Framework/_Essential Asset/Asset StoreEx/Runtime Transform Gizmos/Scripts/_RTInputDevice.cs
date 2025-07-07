//#define INPUT_DEVICE_VR_CONTROLLER
using UnityEngine;

namespace RTG
{
    public class _RTInputDevice : RTInputDevice
    {
        private static string LOG_FORMAT = "<color=#A476EA><b>[_RTCameraBackground]</b></color> {0}";

        public static _RTInputDevice Instance
        {
            get
            {
                return Get as _RTInputDevice;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1)
            _inputDevice = new TouchInputDevice(10);
#elif INPUT_DEVICE_VR_CONTROLLER
            //_inputDevice = new MyVRCtrlImplementation(...);
#else
            _inputDevice = new MouseInputDevice();
#endif
        }
    }
}
