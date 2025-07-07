using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    public class _MUI_MovableWindow : WindowDragger
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_MovableWindow]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), dragArea : <b>" + dragArea + "</b>");
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