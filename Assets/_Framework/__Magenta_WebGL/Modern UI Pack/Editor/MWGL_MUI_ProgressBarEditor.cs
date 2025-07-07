using UnityEngine;
using UnityEditor;

namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_MUI_ProgressBar))]
    public class MWGL_MUI_ProgressBarEditor : _Magenta_Framework.MUI_ProgressBarExEditor
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