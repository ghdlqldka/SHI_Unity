#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_ProgressBarLoopManager))]
    public class _MUI_ProgressBarLoopManagerEditor : UIManagerProgressBarLoopEditor
    {
        //
    }
}
#endif