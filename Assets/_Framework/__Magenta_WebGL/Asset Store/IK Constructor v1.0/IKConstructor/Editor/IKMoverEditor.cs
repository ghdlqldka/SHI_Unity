using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(IKMover))]
public class IKMoverEditor : Editor
{
	IKMover     Mover;
	
	
	
	public override void OnInspectorGUI()
	{
		Mover=target as IKMover;
		DrawDefaultInspector();

		if (GUILayout.Button("Set Initial State"))
		{
			Mover.Init();
			EditorUtility.SetDirty(Mover);
			SceneView.RepaintAll();
		}
		
		GUILayout.BeginHorizontal();
//		Mover.runInEditMode=GUILayout.Toggle(Mover.runInEditMode,"Run in editor","button");
		Mover.AutoUpdate=GUILayout.Toggle(Mover.AutoUpdate,"Auto Update","button");
		Mover.DrawVisual=GUILayout.Toggle(Mover.DrawVisual,"Draw Visual","button");
		GUILayout.EndHorizontal();
		
//		Mover.VisualSettings=GUILayout.Toggle(Mover.VisualSettings,"Visual Settings","button");
//		EditorGUILayout.BeginFadeGroup((Mover.VisualSettings)?1:0.00001f);
		Mover.VisualSize=EditorGUILayout.FloatField("Size",Mover.VisualSize);
		Mover.VisualLineColor=EditorGUILayout.ColorField("Line Color",Mover.VisualLineColor);
		Mover.VisualMarkerColor=EditorGUILayout.ColorField("Marker Color",Mover.VisualMarkerColor);
//		EditorGUILayout.EndFadeGroup();
		
		EditorUtility.SetDirty(Mover);
		SceneView.RepaintAll();
	}
}
