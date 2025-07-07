// Reads text file and prints out few first lines (for debug purposes, so you can check what the file contains)

using UnityEditor;
using UnityEngine;
using System.IO;

namespace pointcloudviewer.helper
{
    public class _ShowHeaderDataHelper : ShowHeaderDataHelper
    {
        // create menu item and window
        [MenuItem("_PointCloudTools/View File Header", false, 201)]
        static void Init()
        {
            _ShowHeaderDataHelper window = (_ShowHeaderDataHelper)EditorWindow.GetWindow(typeof(_ShowHeaderDataHelper));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(340, 180);
            window.maxSize = new Vector2(340, 184);
        }

    } // class
} // namespace
