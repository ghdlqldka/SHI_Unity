using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PointCloudRuntimeViewer
{
    public class _UI_RuntimeHelper : RuntimeUIHelper
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_RuntimeHelper]</b></color> {0}";

        // public RuntimeViewerDX11 runtimeViewerDX11;
        protected _RuntimeViewerDX11 _runtimeViewerDX11
        {
            get
            { 
                return runtimeViewerDX11 as _RuntimeViewerDX11;
            }
        }

        public override void StartLoadingPointCloud()
        {
            Debug.LogFormat(LOG_FORMAT, "StartLoadingPointCloud(), filePathField.text : " + filePathField.text);

#if !UNITY_SAMSUNGTV && !UNITY_WEBGL

            // Set variables that you want to use for loading
            _runtimeViewerDX11.fullPath = filePathField.text;

            // we could override other loader settings here also (for example if UI allows setting them)
            /*
//            _runtimeViewerDX11.enablePicking = false;
            _runtimeViewerDX11.fileFormat = 0; // 0="XYZ", 1="XYZRGB", 2="CGO", 3="ASC", 4="CATIA ASC", 5="PLY (ASCII)", 6="LAS", 7="PTS"
            _runtimeViewerDX11.readRGB = true; // this will be automatically disabled, if the file doesnt contain RGB data
            _runtimeViewerDX11.readIntensity = false;
            _runtimeViewerDX11.useUnitScale = false;
            _runtimeViewerDX11.unitScale = 0.001f;
            _runtimeViewerDX11.flipYZ = true;
            _runtimeViewerDX11.autoOffsetNearZero = true;
            _runtimeViewerDX11.useManualOffset = true;
            _runtimeViewerDX11.manualOffset = Vector3.zero;
            _runtimeViewerDX11.plyHasNormals = false;*/

            // call actual loader

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            //_runtimeViewerDX11.LoadRawPointCloud(); // non-threaded reader

            if (_runtimeViewerDX11.IsLoading() == true)
            {
                _runtimeViewerDX11.CallImporterThreaded(filePathField.text);
            }
            else
            {
                Debug.LogError("Its already loading..");
            }

            stopwatch.Stop();
            //Debug.Log("Loaded in: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Reset();
#endif        
        }

    }
}
