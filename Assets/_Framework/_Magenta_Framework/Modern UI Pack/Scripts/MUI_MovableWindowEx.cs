using UnityEngine;
using UnityEngine.EventSystems;

namespace _Magenta_Framework
{
    public class MUI_MovableWindowEx : Michsky.MUIP._MUI_MovableWindow
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_MovableWindowEx]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), dragArea : " + dragArea);
            // base.Start();

            if (dragArea == null)
            {
                try
                {
#if UNITY_2023_2_OR_NEWER
                    var canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)[0];
                    Debug.LogFormat(LOG_FORMAT, "canvas.gameObject : " + canvas.gameObject.name);
#else
                    var canvas = (Canvas)FindObjectsOfType(typeof(Canvas))[0];
#endif
                    dragArea = canvas.GetComponent<RectTransform>();
                }

                catch
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "<b>[Movable Window]</b> Drag Area has not been assigned.");
                }
            }
        }
    }
}