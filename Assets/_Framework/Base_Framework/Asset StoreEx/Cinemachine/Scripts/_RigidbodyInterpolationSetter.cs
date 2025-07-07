using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// Helper class to set the interpolation of a rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class _RigidbodyInterpolationSetter : RigidbodyInterpolationSetter
    {
        private static string LOG_FORMAT = "<color=#F6CC5A><b>[_RigidbodyInterpolationSetter]</b></color> {0}";

        protected override void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public override void SetInterpolation(bool on)
        {
            Debug.LogFormat(LOG_FORMAT, "this.gameObject : <b>" + this.gameObject.name + "</b>, SetInterpolation(), on : <b>" + on + "</b>");

            // m_Rigidbody.interpolation = on ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
            if (on == true)
            {
                m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            else
            {
                m_Rigidbody.interpolation = RigidbodyInterpolation.None;
            }
        }
    }
}
