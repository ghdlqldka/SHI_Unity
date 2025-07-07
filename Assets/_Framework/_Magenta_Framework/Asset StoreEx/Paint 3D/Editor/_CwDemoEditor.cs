using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
namespace CW.Common
{
	using UnityEditor;
	using TARGET = Paint3D._CwDemo;

	[CustomEditor(typeof(TARGET))]
	public class _CwDemoEditor : CwDemo_Editor
    {
		protected override void OnInspector()
		{
			TARGET tgt; 
			TARGET[] tgts; 
			GetTargets(out tgt, out tgts);

			Draw("upgradeInputModule", "If you enable this setting and your project is running with the new InputSystem then the EventSystem's InputModule component will be upgraded.");

			Separator();

			Draw("changeExposureInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to this GameObject that adjusts the camera exposure to match the other pipelines.");
			Draw("changeVisualEnvironmentInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to this GameObject that adjusts the background to match the other pipelines.");
			Draw("changeFogInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to the scene that adjusts the fog to match the other pipelines.");
			Draw("changeCloudsInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to the scene that adjusts the clouds to match the other pipelines.");
			Draw("changeMotionBlurInHDRP", "If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the motion blur to match the other pipelines.");
			Draw("upgradeLightsInHDRP", "If you enable this setting and your project is running with HDRP then any lights missing the HDAdditionalLightData component will have it added.");
			Draw("upgradeCamerasInHDRP", "If you enable this setting and your project is running with HDRP then any cameras missing the HDAdditionalCameraData component will have it added.");
		}
	}
}
#endif