// Random Point Cloud Binary Generator
// Creates point cloud with given amount of points for testing

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Globalization;

namespace pointcloudviewer.generator
{
    public class _RandomCloudGenerator : RandomCloudGenerator
    {
        
        // create menu item and window
        [MenuItem("_PointCloudTools/Create test binary cloud", false, 200)]
        static void Init()
        {
            _RandomCloudGenerator window = (_RandomCloudGenerator)EditorWindow.GetWindow(typeof(_RandomCloudGenerator));
            window.titleContent = new GUIContent(appName);
            window.minSize = new Vector2(340, 280);
            window.maxSize = new Vector2(340, 284);

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
