using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RTG
{
    public class _RTFocusCamera : RTFocusCamera
    {
        private static string LOG_FORMAT = "<color=#1D9FF1><b>[_RTFocusCamera]</b></color> {0}";

        public static _RTFocusCamera Instance
        {
            get
            {
                return Get as _RTFocusCamera;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            Debug.Assert(TargetCamera != null);
            /*
            if (TargetCamera == null)
            {
                Debug.Break();
                Debug.LogErrorFormat(LOG_FORMAT, "RTCamera: No target camera was specified.");
            }
            */

            SetTargetCamera(TargetCamera);
            _worldTransformSnapshot.Snaphot(_targetTransform);

            _prjSwitchTranstion.TargetMono = this;
            _prjSwitchTranstion.TransitionBegin += OnPrjSwitchTransitionBegin;
            _prjSwitchTranstion.TransitionUpdate += OnPrjSwitchTransitionUpate;
            _prjSwitchTranstion.TransitionEnd += OnPrjSwitchTransitionEnd;
        }

        /*
        protected override void OnPrjSwitchTransitionBegin(CameraPrjSwitchTransition.Type transitionType)
        {
            if (PrjSwitchTransitionBegin != null)
                PrjSwitchTransitionBegin(transitionType);
        }
        */
    }
}
