using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = _CwMeshFixer;

	[CustomEditor(typeof(TARGET))]
	public class _CwMeshFixerEditor : CwMeshFixer_Editor
    {
        private static string LOG_FORMAT = "<color=#00E1FF><b>[_CwMeshFixerEditor]</b></color> {0}";

        public static new void CreateMeshFixerAsset(Mesh mesh)
        {
            string meshPath = AssetDatabase.GetAssetPath(mesh);
            Debug.LogFormat(LOG_FORMAT, "CreateMeshFixerAsset(), mesh : " + mesh + ", meshPath : <b>" + meshPath + "</b>");

            _CreateMeshFixerAsset(mesh, meshPath);
        }

        public static void _CreateMeshFixerAsset(Mesh mesh, string path)
        {
            _CwMeshFixer asset = CreateInstance<_CwMeshFixer>();
            string _name = "_Mesh Fixer";

            if (string.IsNullOrEmpty(path) == true || path.StartsWith("Library/", System.StringComparison.InvariantCultureIgnoreCase))
            {
                path = "Assets";
            }
            else if (AssetDatabase.IsValidFolder(path) == false)
            {
                path = System.IO.Path.GetDirectoryName(path);
            }

            Debug.Assert(mesh != null);
            // if (mesh != null)
            {
                string meshPath = AssetDatabase.GetAssetPath(mesh);
                AssetImporter modelImporter = AssetImporter.GetAtPath(meshPath);

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

                    _name += " (" + System.IO.Path.GetFileNameWithoutExtension(modelImporter.assetPath) + ")";
                }
                else
                {
                    _name += " (" + mesh.name + ")";

                    asset.AddMesh(mesh);
                }
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + _name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif