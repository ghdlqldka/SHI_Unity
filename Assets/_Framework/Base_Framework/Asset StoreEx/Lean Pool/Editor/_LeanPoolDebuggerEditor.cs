using UnityEngine;
using CW.Common;
using Lean.Pool.Examples;

#if UNITY_EDITOR
namespace Lean.Pool
{
    using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(_LeanPoolDebugger), true)]
	public class _LeanPoolDebuggerEditor : Examples.Editor.LeanPoolDebugger_Editor
    {
		protected SerializedProperty m_Script;

		protected virtual void OnEnable()
		{
			m_Script = serializedObject.FindProperty("m_Script");
		}

		protected override void OnInspector()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Script);
			EditorGUILayout.Space();

			_LeanPoolDebugger tgt; 
			_LeanPoolDebugger[] tgts; 
			GetTargets(out tgt, out tgts);

			Info("This component can be added to your prefab GameObject, and it will throw warnings if it is instantiated without the use of <b>LeanPool.Spawn</b>, or despawned without the use of <b>LeanPool.Despawn</b>.");
		}
	}
}
#endif