using UnityEngine;
using UnityEditor;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_HorizontalSelectorEx))]
    public class MUI_HorizontalSelectorExEditor : Michsky.MUIP._MUI_HorizontalSelectorEditor
    {
#if false //
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // base.OnEnable();
            hsTarget = (MUI_HorizontalSelectorEx)target;
        }
#endif
    }
}