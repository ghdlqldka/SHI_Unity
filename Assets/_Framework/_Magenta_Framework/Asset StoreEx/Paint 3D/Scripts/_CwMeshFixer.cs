using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using static PaintCore.CwCoordCopier;

namespace PaintIn3D
{
	/// <summary>This tool allows you to process any mesh so that it can be painted.
	/// The fixed meshes will be placed as a child of this tool in your Project window.
	/// To use the fixed mesh, drag and drop it into your MeshFilter or SkinnedMeshRenderer.
	/// This tool can be accessed from the context menu (⋮ button at top right) of any mesh/model inspector.</summary>
	[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwMeshFixer")]
	public class _CwMeshFixer : CwMeshFixer
    {
        private static string LOG_FORMAT = "<color=#0FF8D3><b>[_CwPaintableMesh]</b></color> {0}";

        [ContextMenu("Generate")]
        public override void Generate()
        {
			Debug.LogFormat(LOG_FORMAT, "Generate()");
			// base.Generate();

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Generate Seam Fix");

            UnwrapParams.angleError = AngleError;
            UnwrapParams.areaError = AreaError;
            UnwrapParams.hardAngle = HardAngle;
            UnwrapParams.packMargin = PackMargin;
#endif

            Debug.LogFormat(LOG_FORMAT, "Meshes : " + Meshes);
            if (Meshes != null)
            {
                foreach (Pair pair in Meshes)
                {
                    if (pair.Source != null)
                    {
                        if (pair.Output == null)
                        {
                            pair.Output = new Mesh();
                        }

                        pair.Output.name = pair.Source.name + " (Fixed)";

                        Generate(pair.Source, pair.Output, GenerateUV, FixOverflow, FixSeams, Coord, Border);
                    }
                    else
                    {
                        DestroyImmediate(pair.Output);

                        pair.Output = null;
                    }
                }
            }

#if UNITY_EDITOR
            if (CwHelper.IsAsset(this) == true)
            {
				string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                Debug.LogFormat(LOG_FORMAT, "@1, assetPath : " + assetPath);
                Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);

                for (int i = 0; i < assets.Length; i++)
                {
                    Mesh assetMesh = assets[i] as Mesh;

                    if (assetMesh != null)
                    {
                        if (Meshes == null || Meshes.Exists(p => p.Output == assetMesh) == false)
                        {
                            DestroyImmediate(assetMesh, true);
                        }
                    }
                }

                if (Meshes != null)
                {
                    foreach (Pair pair in Meshes)
                    {
                        if (pair.Output != null && CwHelper.IsAsset(pair.Output) == false)
                        {
                            UnityEditor.AssetDatabase.AddObjectToAsset(pair.Output, this);

                            UnityEditor.AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            if (CwHelper.IsAsset(this) == true)
            {
                CwHelper.ReimportAsset(this);
            }

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = _CwMeshFixer;

	[CustomEditor(typeof(TARGET))]
	public class _CwMeshFixerEditor : CwMeshFixer_Editor
    {
		private Texture2D sourceTexture;

		private enum SquareSizes
		{
			Square64    =    64,
			Square128   =   128,
			Square256   =   256,
			Square512   =   512,
			Square1024  =  1024,
			Square2048  =  2048,
			Square4096  =  4096,
			Square8192  =  8192,
			Square16384 = 16384,
		}

		private SquareSizes newSize = SquareSizes.Square1024;

		private Dictionary<Mesh, Mesh> pairs = new Dictionary<Mesh, Mesh>();

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			EditorGUILayout.HelpBox("This tool allows you to process any mesh so that it can be painted. The fixed meshes will be placed as a child of this tool in your Project window. To use the fixed mesh, drag and drop it into your MeshFilter or SkinnedMeshRenderer.", MessageType.Info);

			Separator();

			Each(tgts, t => t.ConvertLegacy()); serializedObject.Update();

			var sMeshes = serializedObject.FindProperty("meshes");
			var sDel    = -1;
			var missing = false;

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Meshes");
				if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
				{
					sMeshes.InsertArrayElementAtIndex(sMeshes.arraySize);
				}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
				for (var i = 0; i < tgt.Meshes.Count; i++)
				{
					var sSource = sMeshes.GetArrayElementAtIndex(i).FindPropertyRelative("Source");

					EditorGUILayout.BeginHorizontal();
						BeginError(sSource.objectReferenceValue == null);
							EditorGUILayout.PropertyField(sSource, GUIContent.none);
						EndError();
						var sourceMesh = tgt.Meshes[i].Source;
						BeginDisabled(sourceMesh == null);
							if (GUILayout.Button("Analyze Old", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								CwMeshAnalysis.OpenWith(sourceMesh, 0);
							}
						EndDisabled();
						var outputMesh = tgt.Meshes[i].Output;
						if (outputMesh == null)
						{
							missing = true;
						}
						BeginDisabled(outputMesh == null);
							if (GUILayout.Button("Analyze New", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								CwMeshAnalysis.OpenWith(outputMesh, 0);
							}
							//EditorGUILayout.ObjectField(GUIContent.none, outputMesh, typeof(Mesh), false, GUILayout.Width(80));
						EndDisabled();
						if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
						{
							sDel = i;
						}
					EditorGUILayout.EndHorizontal();
				}
			EditorGUI.indentLevel--;

			if (sDel >= 0)
			{
				sMeshes.DeleteArrayElementAtIndex(sDel);
			}

			Separator();

			Draw("coord", "The UV channel whose seams will be fixed.");

			Separator();

			BeginDisabled();
				EditorGUILayout.Toggle(new GUIContent("Recenter UV", "If UV data is shifted out of the 0..1 range (e.g. 3..4), it will be wrapped back to 0..1."), true);
			EndDisabled();

			Separator();

			Draw("generateUV", "Generate UV data for the meshes?");
			if (Any(tgts, t => t.GenerateUV == true))
			{
				BeginIndent();
					Draw("angleError", "Maximum allowed angle distortion (0..1).");
					Draw("areaError", "Maximum allowed area distortion (0..1).");
					Draw("hardAngle", "This angle (in degrees) or greater between triangles will cause seam to be created.");
					Draw("packMargin", "How much uv-islands will be padded.");
				EndIndent();
			}

			//Separator();

			//Draw("fixOverflow", "If UV data is shifted out of the 0..1 range (e.g. 3..4), it will be wrapped back to 0..1.\n\nHowever, if these wrapped triangles still go outside the 0..1 range, should they be wrapped to the other side so it can still be fully painted?");

			Separator();

			Draw("fixSeams", "Fix the seams of the meshes?");
			if (Any(tgts, t => t.FixSeams == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.Border <= 0.0f));
						Draw("border", "The thickness of the UV borders in the fixed mesh.");
					EndError();
				EndIndent();
			}

			Separator();

			BeginColor(Color.green, missing);
				if (Button("Generate") == true)
				{
					Each(tgts, t => t.Generate());
				}
			EndColor();

			Separator();
			Separator();

			EditorGUILayout.LabelField("REMAP TEXTURE", EditorStyles.boldLabel);

			sourceTexture = (Texture2D)EditorGUILayout.ObjectField(sourceTexture, typeof(Texture2D), false);

			if (sourceTexture != null)
			{
				newSize = (SquareSizes)EditorGUILayout.EnumPopup("New Size", newSize);

				Separator();

				EditorGUILayout.LabelField("REMAP WITH MESH", EditorStyles.boldLabel);

				for (var i = 0; i < tgt.Meshes.Count; i++)
				{
					var pair = tgt.Meshes[i];

					if (pair != null && pair.Source != null && pair.Output != null)
					{
						if (GUILayout.Button(pair.Source.name) == true)
						{
							Remap(sourceTexture, pair.Source, pair.Output, (int)newSize);
						}
					}
				}
			}

			if (tgts.Length == 1)
			{
				Separator();
				Separator();

				EditorGUILayout.LabelField("SWAP MESHES", EditorStyles.boldLabel);

				pairs.Clear();

				foreach (var pair in tgt.Meshes)
				{
					if (pair.Source != null && pair.Output != null)
					{
						pairs.Add(pair.Source, pair.Output);
					}
				}

				Mesh output;

				var count = 0;

#if UNITY_6000_0_OR_NEWER
				foreach (var mm in FindObjectsByType<CwMeshModel>(FindObjectsSortMode.InstanceID))
#else
				foreach (var mm in FindObjectsOfType<CwMeshModel>())
#endif
				{
					var mf  = mm.GetComponent<MeshFilter>();
					var smr = mm.GetComponent<SkinnedMeshRenderer>();
					var m   = mf != null ? mf.sharedMesh : (smr != null ? smr.sharedMesh : null);

					if (m != null && pairs.TryGetValue(m, out output) == true)
					{
						EditorGUILayout.BeginHorizontal();
							BeginDisabled();
								EditorGUILayout.ObjectField(mm.gameObject, typeof(GameObject), true);
							EndDisabled();
							BeginColor(Color.green);
								if (GUILayout.Button("Swap", GUILayout.ExpandWidth(false)) == true)
								{
									if (mf != null)
									{
										Undo.RecordObject(mf, "Swap Mesh"); mf.sharedMesh = output; EditorUtility.SetDirty(mf);
									}
									else if (smr != null)
									{
										Undo.RecordObject(smr, "Swap Mesh"); smr.sharedMesh = output; EditorUtility.SetDirty(smr);
									}
								}
							EndColor();
						EditorGUILayout.EndHorizontal();

						count += 1;
					}
				}

				if (count == 0)
				{
					Info("If your scene contains any P3dPaintableMesh/P3dPaintableMeshAtlas components using the original non-fixed mesh, then they will be listed here.");
				}
			}
		}

		private static void Remap(Texture2D sourceTexture, Mesh oldMesh, Mesh newMesh, int newSize)
		{
			var path = AssetDatabase.GetAssetPath(sourceTexture);
			var name = sourceTexture.name;
			var dir  = string.IsNullOrEmpty(path) == false ? System.IO.Path.GetDirectoryName(path) : "Assets";

			if (string.IsNullOrEmpty(path) == false)
			{
				name = System.IO.Path.GetFileNameWithoutExtension(path);
			}

			name += " (Remapped)";

			path = EditorUtility.SaveFilePanelInProject("Export Texture", name, "png", "Export Your Texture", dir);

			if (string.IsNullOrEmpty(path) == false)
			{
				var remapTexture = CwRemap.Remap(sourceTexture, oldMesh, newMesh, newSize);

				CwRemap.Export(remapTexture, path, sourceTexture);

				DestroyImmediate(remapTexture);
			}
		}

		[MenuItem("CONTEXT/Mesh/Fix Mesh (Paint in 3D)")]
		[MenuItem("CONTEXT/ModelImporter/Fix Mesh (Paint in 3D)")]
		public static void Create(MenuCommand menuCommand)
		{
			var sources = new List<Mesh>();
			var mesh    = menuCommand.context as Mesh;
			var name    = "";

			if (mesh != null)
			{
				sources.Add(mesh);

				name = mesh.name;
			}
			else
			{
				var modelImporter = menuCommand.context as ModelImporter;

				if (modelImporter != null)
				{
					var assets = AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath);

					for (var i = 0; i < assets.Length; i++)
					{
						var assetMesh = assets[i] as Mesh;

						if (assetMesh != null)
						{
							sources.Add(assetMesh);
						}
					}

					name = System.IO.Path.GetFileNameWithoutExtension(modelImporter.assetPath);
				}
			}
			
			if (sources.Count > 0)
			{
				var path = AssetDatabase.GetAssetPath(menuCommand.context);

				if (string.IsNullOrEmpty(path) == false)
				{
					path = System.IO.Path.GetDirectoryName(path);
				}
				else
				{
					path = "Assets";
				}

				path += "/Mesh Fixer (" + name + ").asset";

				var instance = CreateInstance<CwMeshFixer>();

				foreach (var source in sources)
				{
					instance.AddMesh(source);
				}

				ProjectWindowUtil.CreateAsset(instance, path);
			}
		}

		[MenuItem("Assets/Create/CW/Paint in 3D/Mesh Fixer")]
		private static void CreateAsset()
		{
			var guids = Selection.assetGUIDs;

			CreateMeshFixerAsset(default(Mesh), guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : default(string));
		}

		public static void CreateMeshFixerAsset(Mesh mesh)
		{
			CreateMeshFixerAsset(mesh, AssetDatabase.GetAssetPath(mesh));
		}

		public static void CreateMeshFixerAsset(Mesh mesh, string path)
		{
			var asset = CreateInstance<CwMeshFixer>();
			var name  = "Mesh Fixer";

			if (string.IsNullOrEmpty(path) == true || path.StartsWith("Library/", System.StringComparison.InvariantCultureIgnoreCase))
			{
				path = "Assets";
			}
			else if (AssetDatabase.IsValidFolder(path) == false)
			{
				path = System.IO.Path.GetDirectoryName(path);
			}

			if (mesh != null)
			{
				var meshPath      = AssetDatabase.GetAssetPath(mesh);
				var modelImporter = AssetImporter.GetAtPath(meshPath);

				if (modelImporter != null)
				{
					foreach (var o in AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath))
					{
						var assetMesh = o as Mesh;

						if (assetMesh is Mesh)
						{
							asset.AddMesh(assetMesh);
						}
					}

					name += " (" + System.IO.Path.GetFileNameWithoutExtension(modelImporter.assetPath) + ")";
				}
				else
				{
					name += " (" + mesh.name + ")";

					asset.AddMesh(mesh);
				}
			}

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

			AssetDatabase.CreateAsset(asset, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset; EditorGUIUtility.PingObject(asset);
		}
	}
}
#endif
#endif