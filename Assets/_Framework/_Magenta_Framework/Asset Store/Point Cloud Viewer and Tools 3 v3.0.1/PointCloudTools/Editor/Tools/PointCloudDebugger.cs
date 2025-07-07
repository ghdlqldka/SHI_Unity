// displays info about converted files, for debugging purposes

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;

namespace pointcloudviewer.helper
{
    public class PointCloudDebugger : EditorWindow
    {
        protected static string appName = "PointCloud Debugger";

        private static Object sourceFile;
        private static int linesToRead = 25;

        // create menu item and window
        [MenuItem("Window/PointCloudTools/PointCloud Debugger", false, 202)]
        static void Init()
        {
            var window = (PointCloudDebugger)EditorWindow.GetWindow(typeof(PointCloudDebugger));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(400, 200);
            window.maxSize = new Vector2(400, 204);
        }

        // main loop
        void OnGUI()
        {
            GUILayout.Label("File to check", EditorStyles.boldLabel);
            sourceFile = EditorGUILayout.ObjectField(sourceFile, typeof(Object), true);
            EditorGUILayout.Space();

            GUI.enabled = sourceFile == null ? false : true;
            if (GUILayout.Button(new GUIContent("Show info", "Displays details about the file"), GUILayout.Height(40)))
            {
                PrintHeaderData(sourceFile);
            }
            GUI.enabled = true;
        } //ongui

        public static void PrintHeaderData(Object sourceFileParam)
        {
            string fileToRead = AssetDatabase.GetAssetPath(sourceFileParam);
            if (!File.Exists(fileToRead)) { Debug.LogError("File not found: " + fileToRead); return; }

            // check type
            var format = Path.GetExtension(fileToRead).ToLower();
            Debug.Log("fileformat = " + format);
            switch (format)
            {
                case ".pcroot": // v3 index file
                    AnalyzeV3(fileToRead);
                    break;
                case ".ucpc": // v2
                    break;
                case ".bin": // v1 or brekel
                    break;
                default:
                    break;
            }


        }

        private static void AnalyzeV3(string fileToRead)
        {
            var globalData = File.ReadAllLines(fileToRead);

            // read header
            var v3Version = int.Parse(globalData[0], CultureInfo.InvariantCulture);
            var gridSize = float.Parse(globalData[1], CultureInfo.InvariantCulture);
            var totalPointCount = long.Parse(globalData[2], CultureInfo.InvariantCulture);
            //Debug.Log("(Tiles Viewer) Total point count = " + totalPointCount + " (" + PointCloudTools.HumanReadableCount(totalPointCount) + ")");
            var minX = float.Parse(globalData[3], CultureInfo.InvariantCulture);
            var minY = float.Parse(globalData[4], CultureInfo.InvariantCulture);
            var minZ = float.Parse(globalData[5], CultureInfo.InvariantCulture);
            var maxX = float.Parse(globalData[6], CultureInfo.InvariantCulture);
            var maxY = float.Parse(globalData[7], CultureInfo.InvariantCulture);
            var maxZ = float.Parse(globalData[8], CultureInfo.InvariantCulture);
            //if (showDebug == true) Debug.Log("(Tiles Viewer) minXYZ to maxXYZ = " + minX + " - " + minY + " - " + minZ + " to " + maxX + " - " + maxY + " - " + maxZ);
            var center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
            var size = new Vector3(Mathf.Abs(maxX) + Mathf.Abs(minX), Mathf.Abs(maxY) + Mathf.Abs(minY), Mathf.Abs(maxZ) + Mathf.Abs(minZ));
            var cloudBounds = new Bounds(center, size);
            var offX = float.Parse(globalData[9], CultureInfo.InvariantCulture);
            var offY = float.Parse(globalData[10], CultureInfo.InvariantCulture);
            var offZ = float.Parse(globalData[11], CultureInfo.InvariantCulture);

            if (v3Version == 2)
            {
                var packMagic = int.Parse(globalData[12], CultureInfo.InvariantCulture);
            }
        }

        public static void PrintExternalHeaderData(string f)
        {
            if (!File.Exists(f)) { Debug.LogError("File not found: " + f); return; }
            using (StreamReader reader = new StreamReader(File.OpenRead(f)))
            {
                string output = "";
                for (int i = 0; i < linesToRead; i++)
                {
                    var line = reader.ReadLine();
                    output += line + "\n";
                }
                Debug.Log(output);
            }
        }


    } // class
} // namespace
