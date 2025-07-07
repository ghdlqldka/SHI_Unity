// displays info about converted files, for debugging purposes

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;

namespace pointcloudviewer.helper
{
    public class _PointCloudDebugger : PointCloudDebugger
    {
        // create menu item and window
        [MenuItem("_PointCloudTools/PointCloud Debugger", false, 202)]
        static void Init()
        {
            _PointCloudDebugger window = (_PointCloudDebugger)EditorWindow.GetWindow(typeof(_PointCloudDebugger));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(400, 200);
            window.maxSize = new Vector2(400, 204);
        }

    } // class
} // namespace
