// #define __USING_MUI__
using UnityEngine.UI;

#if false /////////////////////////
namespace UnityEditor.UI
{
    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Button Component.
    ///   Extend this class to write a custom editor for a component derived from Button.
    /// </summary>
    public class _ButtonEditor : SelectableEditor
    {
#if __USING_MUI__
        //
#else
        SerializedProperty m_OnClickProperty;
#endif

#if __USING_MUI__
        //
#else
        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
        }
#endif

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
#if __USING_MUI__
            //
#else
            EditorGUILayout.PropertyField(m_OnClickProperty);
#endif
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif /////////////////////////////////