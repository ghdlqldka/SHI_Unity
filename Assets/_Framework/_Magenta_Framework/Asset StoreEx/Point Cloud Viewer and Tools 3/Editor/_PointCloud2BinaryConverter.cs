// Point Cloud to Binary Converter
// Saves pointcloud data into custom binary file, for faster viewing
// http://unitycoder.com

#pragma warning disable 0219 // disable unused var warnings (mostly in LAS converter)

using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using PointCloudHelpers;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using PointCloudViewer.Structs;
using pointcloudviewer.helper;
using pointcloudviewer.helpers;

namespace pointcloudviewer.pointcloud2binary
{
    public class _PointCloud2BinaryConverter : PointCloud2BinaryConverter
    {
        
        // create menu item and window
        [MenuItem("_PointCloudTools/Convert Point Cloud To Binary (DX11)", false, 1)]
        static void Init()
        {
            _PointCloud2BinaryConverter window = (_PointCloud2BinaryConverter)EditorWindow.GetWindow(typeof(_PointCloud2BinaryConverter));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(380, 564);
            window.maxSize = new Vector2(380, 568);
            //SceneView.onSceneGUIDelegate += OnSceneUpdate;

            // force dot as decimal separator
            string CultureName = Thread.CurrentThread.CurrentCulture.Name;
            CultureInfo ci = new CultureInfo(CultureName);
            if (ci.NumberFormat.NumberDecimalSeparator != ".")
            {
                ci.NumberFormat.NumberDecimalSeparator = ".";
                Thread.CurrentThread.CurrentCulture = ci;
            }
        }

    } // class
} // namespace
