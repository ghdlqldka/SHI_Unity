using UnityEngine;
using UnityEngine.UI;
using CW.Common;

namespace CW.Common
{
	/// <summary>This component allows you to quickly build a UI button to activate only this GameObject when clicked.</summary>
	// [HelpURL(CwShared.HelpUrlPrefix + "CwDemoButtonBuilder")]
	// [AddComponentMenu(CwShared.ComponentMenuPrefix + "Demo Button Builder")]
	public class _CwDemoButtonBuilder : CwDemoButtonBuilder
    {
		//
	}
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwDemoButtonBuilder;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwDemoButtonBuilder_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("buttonPrefab", "The built button will be based on this prefab.");
			Draw("buttonRoot", "The built button will be placed under this transform.");

			Separator();

			Draw("icon", "The icon given to this button.");
			Draw("color", "The icon will be tinted by this.");
			Draw("overrideName", "Use a different name for the button text?");

			Separator();

			if (Button("Build All") == true)
			{
				Undo.RecordObjects(tgts, "Build All");

				tgt.BuildAll();
			}
		}
	}
}
#endif
#endif