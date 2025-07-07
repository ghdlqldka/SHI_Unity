using RuntimeSceneGizmo;
using UnityEditor;
using UnityEngine;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(GizmoMaxManagerEx))]
    public class GizmoMaxManagerExEditor : _Base_Framework._GizmoMaxManagerEditor
    {

#if true //
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_camParentTransform);
            EditorGUILayout.PropertyField(_camera);

            EditorGUILayout.Space();
            // EditorGUILayout.PropertyField(_gizmoListener);
            EditorGUILayout.PropertyField(_maxCamera);
            EditorGUILayout.PropertyField(_gizmoRenderer);

            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}