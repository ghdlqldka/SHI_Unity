using UnityEngine;
using UnityEditor;

namespace EA
{
    /// <summary>
    /// Property drawer for inspector fields which shouldn't be interactable
    /// </summary>
    [CustomPropertyDrawer(typeof(GUIDisabledAttribute))]
    public class GUIDisabledAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool enabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            //base.OnGUI(position, property, label);
            GUI.enabled = enabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}