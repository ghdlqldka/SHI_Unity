using UnityEngine;
using System.Collections.Generic;
using CW.Common;

#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using UnityEditor;
	using TARGET = MWGL_CwPaintableManager;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwPaintableManager))]
	public class MWGL_CwPaintableManagerEditor : PaintCore._CwPaintableManagerEditor
    {
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
		{
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            MWGL_CwPaintableManager tgt;
            MWGL_CwPaintableManager[] tgts; 
			GetTargets(out tgt, out tgts);

			Info("This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.");

			Draw("readPixelsBudget", "If the current GPU doesn't support async texture reading, this setting allows you to limit how many pixels are read per frame to reduce lag.");
		}
	}
}
#endif
