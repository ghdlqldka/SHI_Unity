#if URP_INSTALLED

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace pointcloudviewer.urp
{
    public class MaterialConverterWindowURP : EditorWindow
    {
        // Menu item under Window/PointCloudTools/Convert Materials to URP.
        [MenuItem("Window/PointCloudTools/Convert Sample Materials to URP")]
        public static void ShowWindowMenu()
        {
            // Check if URP is active; if not, warn the user.
            if (!(GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset))
            {
                EditorUtility.DisplayDialog("URP Not Active",
                    "Universal Render Pipeline is not active in this project. Please switch to URP to use this conversion tool.",
                    "OK");
                return;
            }

            var window = GetWindow<MaterialConverterWindowURP>("Convert Materials to URP");
            window.minSize = new Vector2(600, 200);
        }

        private void OnGUI()
        {
            GUILayout.Label("URP Material Conversion", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("URP is currently active. Would you like to convert point cloud viewer sample scene materials to URP?");
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                ConvertMaterialsToURP();
                EditorUtility.DisplayDialog("Conversion Complete", "Materials have been converted to URP.", "OK");
                Close();
            }
            if (GUILayout.Button("No"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }

        private void ConvertMaterialsToURP()
        {
            // Only search for material assets within the specified folder.
            string searchFolder = "Assets/PointCloudTools";
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { searchFolder });
            int convertedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat != null && mat.shader != null)
                {
                    // Example conversion: change materials using the Standard shader to the URP Lit shader.
                    if (mat.shader.name == "Standard")
                    {
                        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                        if (urpShader != null)
                        {
                            mat.shader = urpShader;
                            EditorUtility.SetDirty(mat);
                            convertedCount++;
                        }
                    }
                }
            }

            Debug.Log($"Converted {convertedCount} materials to URP in folder: {searchFolder}");
            AssetDatabase.SaveAssets();
        }
    }
}

#endif