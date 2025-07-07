using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class __3DModelColorLerp : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=green><b>[__3DModelColorLerp]</b></color> {0}";

        [SerializeField]
        protected Renderer[] renderers;

        [Space(10)]
        [SerializeField]
        protected float duration = 5; // This will be your time in seconds.

        [Header("Color")]
        [SerializeField]
        protected Color startColor;
        [SerializeField]
        protected Color endColor;

        protected float smoothness = 0.02f; // This will determine the smoothness of the lerp. Smaller values are smoother. Really it's the time between updates.

        // public delegate void EndLerpCallback();
        // protected event EndLerpCallback OnEndLerp;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(renderers.Length > 0);

            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = new Material(renderers[i].material);
                renderers[i].material = material;
            }
        }

        protected virtual void OnEnable()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = startColor;
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            /*
#if DEBUG
            if (Input.GetMouseButtonDown(_InputTouches.IT_GestureEx.Mouse_Left_Button))
            {
                DoLerp(startColor, endColor, duration);
            }
#endif
            */
        }

        public virtual void DoLerp(_Utilities.EndCallback endLerpCallback = null)
        {
            StopAllCoroutines();

            StartCoroutine(LerpColor(endLerpCallback));
        }

        public virtual void DoLerp(Color a, Color b, float duration, _Utilities.EndCallback endLerpCallback = null)
        {
            // StopAllCoroutines();

            startColor = a;
            endColor = b;
            this.duration = duration;

            DoLerp(endLerpCallback);
        }

        protected IEnumerator LerpColor(_Utilities.EndCallback endLerpCallback)
        {
            Debug.LogFormat(LOG_FORMAT, "LerpColor()");

            float progress = 0; // This float will serve as the 3rd parameter of the lerp function.
            float increment = smoothness / duration; // The amount of change to apply.
            Color _color;
            while (progress < 1)
            {
                _color = Color.Lerp(startColor, endColor, progress);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.color = _color;
                }
                progress += increment;
                yield return new WaitForSeconds(smoothness);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = endColor;
            }

            if (endLerpCallback != null)
            {
                endLerpCallback();
            }
        }
    }
}