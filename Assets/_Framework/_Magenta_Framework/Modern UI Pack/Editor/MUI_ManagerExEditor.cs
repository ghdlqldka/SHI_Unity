using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

#if UNITY_EDITOR
namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_ManagerEx))]
    [System.Serializable]
    public class MUI_ManagerExEditor : Michsky.MUIP._MUI_ManagerEditor
    {
        //
    }
}
#endif