// NOTE: Point cloud data should be randomized, to evenly hide points. Otherwise they are hidden in scan order

using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace PointCloudRuntimeViewer
{
    public class _PointCloudDynamicResolution : PointCloudDynamicResolution
    {
        private static string LOG_FORMAT = "<color=#DAEA1E><b>[_PointCloudDynamicResolution]</b></color> {0}";

        // public PointCloudViewerDX11 viewer;
        protected _PointCloudViewerDX11 _Viewer
        {
            get
            {
                return viewer as _PointCloudViewerDX11;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            showDebug = true; // forcelly set!!!!!

            if (useInitialSizeAsMin == true)
            {
                float? pointSize = _Viewer.GetPointSize();
                if (pointSize != null)
                {
                    minSize = Mathf.Abs((float)pointSize);
                }
                else
                {
                    if (adjustPointSize == true)
                    {
                        adjustPointSize = false;
                        Debug.LogFormat(LOG_FORMAT, "PointCloudDynamicResolution> Shader doesnt support point size, disabling size adjust", gameObject);
                    }
                }
            }

            // check viewer
            if (_Viewer.useCommandBuffer == true)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PointCloudDynamicResolution doesnt work when CommandBuffer is enabled!");
            }
        }

        protected override void Update()
        {
            if ((allowHoldKeyDown && Input.GetKey(decrease)) || Input.GetKeyDown(decrease))
            {
                if (_Viewer.isNewFormat == true) // .ucpc
                {
                    amount = (int)(_Viewer.initialPointsToRead * adjustSpeed);
                }
                else // old format
                {
                    amount = (int)(_Viewer.TotalMaxPoints * adjustSpeed);
                }

                _Viewer.AdjustVisiblePointsAmount(-amount);
                if (adjustPointSize == true)
                {
                    float l = _Viewer.TotalPoints / (float)_Viewer.TotalMaxPoints;
                    float pointSize = Mathf.Lerp(maxSize, minSize, l);
                    _Viewer.SetPointSize(pointSize);
                }

                // if (showDebug == true) 
                {
                    Debug.LogFormat(LOG_FORMAT, "Points= " + _Viewer.TotalPoints + " / " + _Viewer.TotalMaxPoints);
                }
            }

            if ((allowHoldKeyDown && Input.GetKey(increase)) || Input.GetKeyDown(increase))
            {
                if (_Viewer.isNewFormat == true) // .ucpc
                {
                    amount = (int)(_Viewer.initialPointsToRead * adjustSpeed);
                }
                else // old format
                {
                    amount = (int)(_Viewer.TotalMaxPoints * adjustSpeed);
                }

                _Viewer.AdjustVisiblePointsAmount(amount);
                if (adjustPointSize == true)
                {
                    float l = _Viewer.TotalPoints / (float)_Viewer.TotalMaxPoints;
                    float pointSize = Mathf.Lerp(maxSize, minSize, l);
                    _Viewer.SetPointSize(pointSize);
                }

                // if (showDebug == true)
                {
                    Debug.LogFormat(LOG_FORMAT, "PointCloudDynamicResolution: Points= " + _Viewer.TotalPoints + " / " + _Viewer.TotalMaxPoints);
                }
            }

        }
    }
}
