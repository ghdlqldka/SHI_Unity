using UnityEngine;
using UnityEditor;
using pointcloudviewer.binaryviewer;

namespace pointcloudviewer.extras
{
    [CustomPropertyDrawer(typeof(ITileAction))]
    public class InterfacePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetType = typeof(ITileAction);
            var obj = property.objectReferenceValue as MonoBehaviour;

            if (obj != null && targetType.IsAssignableFrom(obj.GetType()))
            {
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(MonoBehaviour), true);
            }
            else
            {
                EditorGUI.HelpBox(position, "Select an object that implements ITileAction", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
