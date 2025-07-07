using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTG
{
    [Serializable]
    public class _UniversalGizmo : UniversalGizmo
    {
        private static string LOG_FORMAT = "<color=#773B00><b>[_UniversalGizmo]</b></color> {0}";

        protected override void SetMoveHandlesVisible(bool visible)
        {
            Debug.LogFormat(LOG_FORMAT, "SetMoveHandlesVisible(), visible : " + visible);

            _mvDblSliders.SetVisible(visible, true);
            _mvAxesSliders.SetVisible(visible);
            _mvAxesSliders.Set3DCapsVisible(visible);
        }

        protected override void SetRotationHandlesVisible(bool visible)
        {
            Debug.LogFormat(LOG_FORMAT, "SetRotationHandlesVisible(), visible : " + visible);

            _rtAxesSliders.SetBorderVisible(visible);
            _rtMidCap.SetVisible(visible);
            _rtCamLookSlider.SetBorderVisible(visible);
        }

        protected override void SetScaleHandlesVisible(bool visible)
        {
            Debug.LogFormat(LOG_FORMAT, "SetScaleHandlesVisible(), visible : " + visible);

            _scMidCap.SetVisible(visible);
        }

        public virtual void SetVisible(bool visible)
        {
            //
        }
    }
}
