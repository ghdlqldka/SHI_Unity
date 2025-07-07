// Point Cloud to Unity Mesh Converter
// Converts pointcloud data into multiple mesh assets
// http://unitycoder.com

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Rendering;
using System.Threading;
using System.Globalization;
using pointcloudviewer.helper;

#pragma warning disable 0219 // disable unused var warnings


namespace pointcloudviewer.pointcloud2mesh
{

    public class _PointCloud2MeshConverter : PointCloud2MeshConverter
    {
        
        // create menu item and window
        [MenuItem("_PointCloudTools/Convert Point Cloud To Unity Meshes", false, 2)]
        static void Init()
        {
            _PointCloud2MeshConverter window = (_PointCloud2MeshConverter)EditorWindow.GetWindow(typeof(_PointCloud2MeshConverter));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(340, 700);
            window.maxSize = new Vector2(340, 704);

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
