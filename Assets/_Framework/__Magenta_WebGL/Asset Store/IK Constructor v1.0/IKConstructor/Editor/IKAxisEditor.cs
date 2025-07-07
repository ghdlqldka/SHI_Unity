using UnityEngine;
using System.Collections;
using UnityEditor;



[CustomEditor(typeof(IKAxis))]
public class IKAxisEditor : Editor
{
	IKAxis      Axis;
	
	
	
	public override void OnInspectorGUI()
	{
		Axis=target as IKAxis;
		DrawDefaultInspector();
		
		GUILayout.BeginVertical();
		
		if (GUILayout.Button("Set Initial State"))
		{
			Axis.Init();
			EditorUtility.SetDirty(Axis);
			SceneView.RepaintAll();
		}
		
		GUILayout.BeginHorizontal();
//		Axis.runInEditMode=GUILayout.Toggle(Axis.runInEditMode,"Run in editor","button");
		Axis.AutoUpdate=GUILayout.Toggle(Axis.AutoUpdate,"Auto Update","button");
//		Axis.DrawGizmo=GUILayout.Toggle(Axis.DrawGizmo,"Draw Gizmo","button");
		Axis.DrawVisual=GUILayout.Toggle(Axis.DrawVisual,"Draw Visual","button");
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		// Окно редактирования параметров визуализации
//		Axis.VisualSettings=GUILayout.Toggle(Axis.VisualSettings,"Visual Settings","button");
//		EditorGUILayout.BeginFadeGroup((Axis.VisualSettings)?1:0.01f);
		Axis.VisualRadius=EditorGUILayout.FloatField("Radius",Axis.VisualRadius);
		Axis.VisualCircleSteps=EditorGUILayout.IntField("Circle Steps",Axis.VisualCircleSteps);
		Axis.VisualCircleColor=EditorGUILayout.ColorField("Circle",Axis.VisualCircleColor);
		Axis.VisualCompensatorColor=EditorGUILayout.ColorField("Compensator",Axis.VisualCompensatorColor);
//		EditorGUILayout.EndFadeGroup();

		EditorUtility.SetDirty(Axis);
		SceneView.RepaintAll();
	}
}
