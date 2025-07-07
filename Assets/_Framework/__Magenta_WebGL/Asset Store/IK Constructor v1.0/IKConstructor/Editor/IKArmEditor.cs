using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(IKArm))]
public class IKArmEditor : Editor
{
	IKArm       Arm;
	
	
	
	public override void OnInspectorGUI()
	{
		Arm=target as IKArm;
		DrawDefaultInspector();
	
		GUILayout.BeginVertical();
		if (GUILayout.Button("Set Initial State"))
		{
			Arm.Init();
			EditorUtility.SetDirty(Arm);
			SceneView.RepaintAll();
		}
		
		GUILayout.BeginHorizontal();
//		Arm.runInEditMode=GUILayout.Toggle(Arm.runInEditMode,"Run in editor","button");
		Arm.AutoUpdate=GUILayout.Toggle(Arm.AutoUpdate,"Auto Update","button");
		Arm.DrawVisual=GUILayout.Toggle(Arm.DrawVisual,"Draw Visual","button");
		GUILayout.EndHorizontal();

		Arm.VisualSize=EditorGUILayout.FloatField("Marker Size",Arm.VisualSize);
		Arm.VisualBaseMarker=EditorGUILayout.Toggle("Base Marker",Arm.VisualBaseMarker);
		Arm.VisualElbowMarker=EditorGUILayout.Toggle("ElbowMarker",Arm.VisualElbowMarker);
		Arm.VisualStartOrientationColor=EditorGUILayout.ColorField("Start Marker",Arm.VisualStartOrientationColor);
		Arm.VisualLineColor=EditorGUILayout.ColorField("Line Color",Arm.VisualLineColor);
		GUILayout.EndVertical();
		
		EditorUtility.SetDirty(Arm);
		SceneView.RepaintAll();
	}
}
