namespace Lean.Pool
{
	using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects]
	[CustomEditor(typeof(_LeanGameObjectPool))]
	public class _LeanGameObjectPoolEditor : Editor.LeanGameObjectPool_Editor
    {
		protected SerializedProperty m_Script;

		protected virtual void OnEnable()
		{
			m_Script = serializedObject.FindProperty("m_Script");
		}

        public static bool _Draw(string propertyPath, string overrideTooltip = null, string overrideText = null)
        {
            SerializedProperty property = _GetPropertyAndSetCustomContent(propertyPath, overrideTooltip, overrideText);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(property, customContent, true);

            return EditorGUI.EndChangeCheck();
        }

        protected static SerializedProperty _GetPropertyAndSetCustomContent(string propertyPath, string overrideTooltip, string overrideText)
        {
            SerializedProperty property = GetProperty(propertyPath);

            // customContent.text = string.IsNullOrEmpty(overrideText) == false ? overrideText : property.displayName;
			if (string.IsNullOrEmpty(overrideText) == false)
			{
                customContent.text = overrideText;
            }
            else
            {
                customContent.text = property.displayName;
            }
			customContent.tooltip = string.IsNullOrEmpty(overrideTooltip) == false ? overrideTooltip : property.tooltip;
            customContent.tooltip = StripRichText(customContent.tooltip); // Tooltips can't display rich text for some reason, so strip it

            return property;
        }

        public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Script);
			EditorGUILayout.Space();

			BeginData(serializedObject);

			ClearStacks();

			Separator();

			OnInspector();

			Separator();

			serializedObject.ApplyModifiedProperties();

			EndData();
		}

		[System.NonSerialized]
		_LeanGameObjectPool _target;
		[System.NonSerialized]
		_LeanGameObjectPool[] _targets;
		protected override void OnInspector()
		{
			GetTargets(out _target, out _targets);

			BeginError(Any(_targets, t => t.Prefab == null));
			if (Draw("prefab", "The prefab this pool controls.") == true)
			{
				Each(_targets, t => { t.Prefab = (GameObject)serializedObject.FindProperty("prefab").objectReferenceValue; }, true);
			}
			EndError();
			// Draw("notification", "If you need to perform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed. None = If you use this then you must rely on the OnEnable and OnDisable messages. SendMessage = The prefab clone is sent the OnSpawn and OnDespawn messages. BroadcastMessage = The prefab clone and all its children are sent the OnSpawn and OnDespawn messages. IPoolable = The prefab clone's components implementing IPoolable are called. Broadcast IPoolable = The prefab clone and all its child components implementing IPoolable are called.");
			// Draw("strategy", "This allows you to control how spawned/despawned GameObjects will be handled. The DeactivateViaHierarchy mode should be used if you need to maintain your prefab's de/activation state.\n\nActivateAndDeactivate = Despawned clones will be deactivated and placed under this GameObject.\n\nDeactivateViaHierarchy = Despawned clones will be placed under a deactivated GameObject and left alone.");
			_Draw("preload", "Should this pool preload some clones?");
			// Draw("capacity", "Should this pool have a maximum amount of spawnable clones?");
			// Draw("recycle", "If the pool reaches capacity, should new spawns force older ones to despawn?");
			// Draw("persist", "Should this pool be marked as DontDestroyOnLoad?");
			// Draw("stamp", "Should the spawned clones have their clone index appended to their name?");
			// Draw("warnings", "Should detected issues be output to the console?");

			Separator();

			BeginDisabled();
			DrawClones("Total", true, true, "preload");
			EditorGUILayout.Space(5);
			DrawClones("Spawned", true, false, "notification");
			DrawClones("Despawned", false, true, "strategy");
			EndDisabled();

			if (Application.isPlaying == false)
			{
				if (Any(_targets, t => t.DespawnedClonesMatch == false))
				{
					Warning("Your preloaded clones no longer match the Prefab.");
				}
			}
		}

		protected override void DrawClones(string title, bool spawned, bool despawned, string propertyName)
		{
			var property = serializedObject.FindProperty(propertyName);
			var rect = EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Height(EditorGUI.GetPropertyHeight(property)));
			EditorGUILayout.EndVertical();
			var rectF = rect;

			rectF.height = 16;

			_target.GetClones(tempClones, spawned, despawned);

			// property.isExpanded = EditorGUI.Foldout(rectF, property.isExpanded, GUIContent.none);

			UnityEditor.EditorGUI.IntField(rect, title, tempClones.Count);

			// if (property.isExpanded == true)
			{
				foreach (var clone in tempClones)
				{
					EditorGUILayout.ObjectField(GUIContent.none, clone, typeof(GameObject), true);
				}
			}
		}
	}
}