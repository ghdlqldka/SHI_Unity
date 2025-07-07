using UnityEngine;
// using _Magenta_Framework;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

#if UNITY_EDITOR
namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_MUI_Manager))]
    [System.Serializable]
    public class MWGL_MUI_ManagerEditor : _Magenta_Framework.MUI_ManagerExEditor
    {
        //
    }
}
#endif