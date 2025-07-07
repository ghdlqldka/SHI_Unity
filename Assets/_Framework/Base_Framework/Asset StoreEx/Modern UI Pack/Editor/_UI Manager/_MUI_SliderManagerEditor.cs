#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using TMPro;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_SliderManager))]
    public class _MUI_SliderManagerEditor : UIManagerSliderEditor
    {
        //
    }
}
#endif