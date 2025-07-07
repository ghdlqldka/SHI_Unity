#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CW.Common;

namespace PaintIn3D
{
	/// <summary>This window allows you to examine the UV data of a mesh. This can be accessed from the context menu (⋮ button at top right) of any mesh in the inspector.</summary>
	public class _CwMeshAnalysisWindow : CwMeshAnalysis// EditorWindow
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[_CwMeshAnalysisWindow]</b></color> {0}";

        public static new void OpenWith(GameObject gameObj, Mesh mesh = null, int channel = 0)
        {
            if (mesh == null && gameObj != null)
            {
                MeshFilter mf = gameObj.GetComponent<MeshFilter>();

                if (mf != null && mf.sharedMesh != null)
                {
                    OpenWith(mf.sharedMesh, channel);
                    return;
                }

                SkinnedMeshRenderer smr = gameObj.GetComponent<SkinnedMeshRenderer>();

                if (smr != null && smr.sharedMesh != null)
                {
                    OpenWith(smr.sharedMesh, channel);
                    return;
                }
            }

            OpenWith(null, 0);
        }

        public static new void OpenWith(Mesh mesh, int channel)
        {
            Debug.LogFormat(LOG_FORMAT, "OpenWith(), mesh : <b>" + mesh.name + "</b>, channel : " + channel);

            _CwMeshAnalysisWindow window = GetWindow<_CwMeshAnalysisWindow>("_Mesh Analysis", true);

            window.mesh = mesh;
            window.ready = false;
            window.coord = channel;
        }

        protected override void OnGUI()
        {
            CwEditor.ClearStacks();

            EditorGUILayout.BeginHorizontal();
            Mesh newMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false);
            EditorGUI.BeginDisabledGroup(newMesh == null);
            if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60)) == true)
            {
                ready = false;
            }
            if (GUILayout.Button("Fix", GUILayout.ExpandWidth(false)) == true)
            {
                Debug.LogFormat(LOG_FORMAT, "Click \"Fix\" Button");
                // CwMeshFixer_Editor.CreateMeshFixerAsset(mesh);
                _CwMeshFixerEditor.CreateMeshFixerAsset(mesh);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (newMesh != mesh)
            {
                Debug.LogFormat(LOG_FORMAT, "@1");
                ready = false;
                mesh = newMesh;
            }

            Debug.Assert(mesh != null);
            // if (mesh != null)
            {
                Debug.LogFormat(LOG_FORMAT, "mesh.subMeshCount : " + mesh.subMeshCount + ", submeshNames.Length : " + submeshNames.Length);
                if (mesh.subMeshCount != submeshNames.Length)
                {
                    List<string> submeshNamesList = new List<string>();

                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        submeshNamesList.Add(i.ToString());
                    }
                    submeshNames = submeshNamesList.ToArray();
                }

                EditorGUILayout.Separator();

                int newSubmesh = EditorGUILayout.Popup("Submesh", submesh, submeshNames);
                int newCoord = EditorGUILayout.Popup("Coord", coord, new string[] { "UV0", "UV1", "UV2", "UV3" });
                int newMode = EditorGUILayout.Popup("Mode", mode, new string[] { "Texcoord", "Triangles" });

                // Debug.LogFormat(LOG_FORMAT, "mode : " + mode);
                if (mode == 1) // Triangles
                {
                    EditorGUILayout.BeginHorizontal();
                    float newPitch = EditorGUILayout.FloatField("Pitch", pitch);
                    float newYaw = EditorGUILayout.FloatField("Yaw", yaw);
                    EditorGUILayout.EndHorizontal();

                    if (newPitch != pitch || newYaw != yaw)
                    {
                        ready = false;
                        pitch = newPitch;
                        yaw = newYaw;
                    }
                }

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Triangles", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntField("Total", triangleCount);
                CwEditor.BeginError(invalidCount > 0);
                EditorGUILayout.IntField("With No UV", invalidCount);
                CwEditor.EndError();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CwEditor.BeginError(outsideCount > 0);
                EditorGUILayout.IntField("Out Of Bounds", outsideCount);
                CwEditor.EndError();
                CwEditor.BeginError(partiallyCount > 0);
                EditorGUILayout.IntField("Partially Out Of Bounds", partiallyCount);
                CwEditor.EndError();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CwEditor.BeginError(utilizationPercent < 40.0f);
                EditorGUILayout.FloatField("Utilization %", utilizationPercent);
                CwEditor.EndError();
                CwEditor.BeginError(overlapPercent > 0);
                EditorGUILayout.FloatField("Overlap %", overlapPercent);
                CwEditor.EndError();
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                if (coord != newCoord || newSubmesh != submesh || newMode != mode || ready == false)
                {
                    ready = true;
                    coord = newCoord;
                    submesh = newSubmesh;
                    mode = newMode;

                    listA.Clear();
                    listB.Clear();
                    ratioList.Clear();
                    mesh.GetTriangles(indices, submesh);
                    mesh.GetVertices(positions);
                    mesh.GetUVs(coord, coords);

                    triangleCount = indices.Count / 3;
                    invalidCount = 0;
                    outsideCount = 0;
                    partiallyCount = 0;
                    overlapTex = CwHelper.Destroy(overlapTex);
                    utilizationPercent = 0.0f;
                    overlapPercent = 0.0f;

                    if (coords.Count > 0)
                    {
                        if (mode == 0) // Texcoord
                        {
                            BakeOverlap();
                        }

                        Quaternion rot = Quaternion.Euler(pitch, yaw, 0.0f);
                        Vector3 off = -mesh.bounds.center;
                        float mul = CwHelper.Reciprocal(mesh.bounds.size.magnitude);
                        Vector3 half = Vector3.one * 0.5f;

                        for (int i = 0; i < indices.Count; i += 3)
                        {
                            Vector3 positionA = positions[indices[i + 0]];
                            Vector3 positionB = positions[indices[i + 1]];
                            Vector3 positionC = positions[indices[i + 2]];
                            Vector2 coordA = coords[indices[i + 0]];
                            Vector2 coordB = coords[indices[i + 1]];
                            Vector2 coordC = coords[indices[i + 2]];
                            int outside = 0; 
                            outside += IsOutside(coordA) ? 1 : 0; 
                            outside += IsOutside(coordB) ? 1 : 0; 
                            outside += IsOutside(coordC) ? 1 : 0;

                            float area = Vector3.Cross(coordA - coordB, coordA - coordC).sqrMagnitude;
                            bool invalid = area <= float.Epsilon;

                            if (invalid == true)
                            {
                                invalidCount++;
                            }

                            if (outside == 3)
                            {
                                outsideCount++;
                            }

                            if (outside == 1 || outside == 2)
                            {
                                partiallyCount++;
                            }

                            if (mode == 0) // Texcoord
                            {
                                listA.Add(coordA); listA.Add(coordB);
                                listA.Add(coordB); listA.Add(coordC);
                                listA.Add(coordC); listA.Add(coordA);
                            }

                            if (mode == 1) // Triangles
                            {
                                positionA = half + rot * (off + positionA) * mul;
                                positionB = half + rot * (off + positionB) * mul;
                                positionC = half + rot * (off + positionC) * mul;

                                positionA.z = positionB.z = positionC.z = 0.0f;

                                listA.Add(positionA); 
                                listA.Add(positionB);
                                listA.Add(positionB);
                                listA.Add(positionC);
                                listA.Add(positionC);
                                listA.Add(positionA);

                                if (invalid == true)
                                {
                                    listB.Add(positionA);
                                    listB.Add(positionB);
                                    listB.Add(positionB);
                                    listB.Add(positionC);
                                    listB.Add(positionC);
                                    listB.Add(positionA);
                                }
                            }
                        }
                    }
                    else
                    {
                        invalidCount = triangleCount;
                    }

                    arrayA = listA.ToArray();
                    arrayB = listB.ToArray();
                }

                Rect rect = EditorGUILayout.BeginVertical(); 
                GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndVertical();
                Vector2 pos = rect.min;
                Vector2 siz = rect.size;
                GUI.Box(rect, "");

                if (mode == 0 && overlapTex != null) // Texcoord
                {
                    GUI.DrawTexture(rect, overlapTex);
                }

                Handles.BeginGUI();
                if (listA.Count > 0)
                {
                    for (int i = listA.Count - 1; i >= 0; i--)
                    {
                        Vector3 coord = listA[i];

                        coord.x = pos.x + siz.x * coord.x;
                        coord.y = pos.y + siz.y * (1.0f - coord.y);

                        arrayA[i] = coord;
                    }

                    Handles.DrawLines(arrayA);

                    for (int i = listB.Count - 1; i >= 0; i--)
                    {
                        Vector3 coord = listB[i];

                        coord.x = pos.x + siz.x * coord.x;
                        coord.y = pos.y + siz.y * (1.0f - coord.y);

                        arrayB[i] = coord;
                    }

                    Handles.color = Color.red;
                    Handles.DrawLines(arrayB);
                }
                Handles.EndGUI();
            }
            /*
            else
            {
                EditorGUILayout.HelpBox("No Mesh Selected.\nTo select a mesh, click Analyze Mesh from the CwPaintable component, or from the Mesh inspector context menu (gear icon at top right).", MessageType.Info);
            }
            */
        }
    }
}
#endif