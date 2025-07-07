// http://www.unity3d-france.com/unity/phpBB3/viewtopic.php?f=24&t=5409 by artemisart

using System.Collections.Generic;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace mgear.tools
{
    public class _GLDebug : GLDebug
    {
        private static string LOG_FORMAT = "<color=#AB2828><b>[_GLDebug]</b></color> {0}";

        // protected static GLDebug instance;
        public static _GLDebug Instance
        {
            get
            {
                return instance as _GLDebug;
            }
            protected set
            {
                instance = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            if (Instance == null)
            {
                Instance = this;

                linesZOn = new List<Line>();
                linesZOff = new List<Line>();

                if (useLineRenderer == true)
                {
                    lineRenderer = this.gameObject.AddComponent<LineRenderer>();
                    lineRenderer.material = lineRendererMat;
                    lineRenderer.startWidth = lineWidth;
                    lineRenderer.endWidth = lineWidth;
                    lineRenderer.positionCount = 2;
                }
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                DestroyImmediate(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = this;
        }

        public virtual void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
        {
            GLDebug.DrawLine(start, end, color, duration, depthTest);
        }
    }
}
