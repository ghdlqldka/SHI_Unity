#if HDRP_INSTALLED
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace pointcloudviewer.hdrp
{
    public class MaterialConverterWindowHDRP : EditorWindow
    {
        // Menu item under Window/PointCloudTools/Convert Materials to HDRP.
        [MenuItem("Window/PointCloudTools/Convert Sample Materials to HDRP")]
        public static void ShowWindowMenu()
        {
            // Check if HDRP is active; if not, warn the user.
            if (!(GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset))
            {
                EditorUtility.DisplayDialog("HDRP Not Active",
                    "High Definition Render Pipeline is not active in this project. Please switch to HDRP to use this conversion tool.",
                    "OK");
                return;
            }

            var window = GetWindow<MaterialConverterWindowHDRP>("Convert Materials to HDRP");
            window.minSize = new Vector2(600, 200);
        }

        private void OnGUI()
        {
            GUILayout.Label("HDRP Material Conversion", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("HDRP is currently active.\nWould you like to convert point cloud viewer sample scene materials to HDRP?");
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                ConvertMaterialsToHDRP();
                EditorUtility.DisplayDialog("Conversion Complete", "Materials have been converted to HDRP.", "OK");
                Close();
            }
            if (GUILayout.Button("No"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }

        private void ConvertMaterialsToHDRP()
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
                    // Example conversion: change materials using the Standard shader to the HDRP Lit shader.
                    if (mat.shader.name == "Standard")
                    {
                        Shader hdrpShader = Shader.Find("HDRP/Lit");
                        if (hdrpShader != null)
                        {
                            mat.shader = hdrpShader;
                            EditorUtility.SetDirty(mat);
                            convertedCount++;
                        }
                    }
                }
            }

            Debug.Log($"Converted {convertedCount} materials to HDRP in folder: {searchFolder}");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif