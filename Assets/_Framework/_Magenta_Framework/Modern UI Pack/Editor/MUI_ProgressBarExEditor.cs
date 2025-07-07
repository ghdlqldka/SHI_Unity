using UnityEngine;
using UnityEditor;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_ProgressBarEx))]
    public class MUI_ProgressBarExEditor : Michsky.MUIP._MUI_ProgressBarEditor
    {
#if false //
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // pbTarget = (ProgressBar)target;
            pbTarget = (MUI_ProgressBarEx)target;
        }
#endif
    }
}