using UnityEngine;
using System.Collections.Generic;
using PaintIn3D;
using CW.Common;
using System.Collections;

namespace _Magenta_WebGL
{
	/// <summary>This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.</summary>
	// [DefaultExecutionOrder(100)]
	// [DisallowMultipleComponent]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableManager")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Manager")]
	public class MWGL_CwPaintableManager : PaintCore._CwPaintableManager
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[MWGL_CwPaintableManager]</b></color> {0}";

        
        public static new MWGL_CwPaintableManager Instance
        {
            get
            {
                return _instance as MWGL_CwPaintableManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");
            // base.Awake();

            if (Instance == null)
            {
                Instance = this;

                instances = null; // Not use!!!!!!!
                instancesNode = null; // Not use!!!!!
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}