using UnityEngine;
using UnityEditor;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_SwitchEx))]
    public class MUI_SwitchExEditor : Michsky.MUIP._MUI_SwitchEditor
    {
#if false //
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // base.OnEnable();
            switchTarget = (MUI_SwitchEx)target;
        }
#endif
    }
}