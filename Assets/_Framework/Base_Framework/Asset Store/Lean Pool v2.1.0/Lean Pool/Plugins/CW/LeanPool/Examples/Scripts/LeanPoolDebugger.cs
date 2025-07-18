﻿using UnityEngine;
using CW.Common;

namespace Lean.Pool.Examples
{
	/// <summary>This component can be added to your prefab GameObject, and it will throw warnings if it is instantiated without the use of <b>LeanPool.Spawn</b>, or despawned without the use of <b>LeanPool.Despawn</b>.</summary>
	[HelpURL(LeanPool.HelpUrlPrefix + "LeanPoolDebugger")]
	[AddComponentMenu(LeanPool.ComponentPathPrefix + "Pool Debugger")]
	public class LeanPoolDebugger : MonoBehaviour
	{
		[SerializeField]
		protected LeanGameObjectPool cachedPool;

		[System.NonSerialized]
		private bool skip;

		protected virtual void Start()
		{
			if (Exists() == false)
			{
				Debug.LogWarning("This clone was NOT spawned using LeanPool.Spawn?!\n" + name, this);

				enabled = false;
			}
		}
#if UNITY_EDITOR
		protected virtual void OnEnable()
		{
			UnityEditor.EditorApplication.playModeStateChanged += Changed;
		}

		protected virtual void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= Changed;
		}

		private void Changed(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
			{
				skip = true;
			}
		}
#endif
		protected virtual void Update()
		{
			if (cachedPool == null)
			{
				Debug.LogWarning("The pool this prefab was spawned using has been destroyed.\n" + name, this);

				enabled = false;
			}
			else if (Exists() == false)
			{
				Debug.LogWarning("This clone was despawned using LeanPool.Despawn, but it's still active?!\n" + name, this);

				enabled = false;
			}
		}

		protected virtual void OnApplicationQuit()
		{
			skip = true;
		}

		protected virtual void OnDestroy()
		{
			if (skip == true)
			{
				return;
			}

			if (Exists() == true)
			{
				Debug.LogWarning("This clone has been destroyed, and it was NOT despawned using LeanPool.Despawn?!\n" + name, this);
			}
		}

        protected virtual bool Exists()
		{
			if (LeanPool.Links.TryGetValue(gameObject, out cachedPool) == true)
			{
				return true;
			}

			if (LeanGameObjectPool.TryFindPoolByClone(gameObject, ref cachedPool) == true)
			{
				return true;
			}

			return false;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Pool.Examples.Editor
{
	using UnityEditor;
	using TARGET = LeanPoolDebugger;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET), true)]
	public class LeanPoolDebugger_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component can be added to your prefab GameObject, and it will throw warnings if it is instantiated without the use of <b>LeanPool.Spawn</b>, or despawned without the use of <b>LeanPool.Despawn</b>.");
		}
	}
}
#endif