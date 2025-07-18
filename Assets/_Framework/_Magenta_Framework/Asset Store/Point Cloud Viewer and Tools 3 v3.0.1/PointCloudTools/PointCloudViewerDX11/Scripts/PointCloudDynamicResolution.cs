﻿// NOTE: Point cloud data should be randomized, to evenly hide points. Otherwise they are hidden in scan order

using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace PointCloudRuntimeViewer
{
    public class PointCloudDynamicResolution : MonoBehaviour
    {
        public PointCloudViewerDX11 viewer;

        public float adjustSpeed = 0.01f;
        public KeyCode decrease = KeyCode.Alpha1;
        public KeyCode increase = KeyCode.Alpha2;
        public bool allowHoldKeyDown = true;

        public bool adjustPointSize = false;
        public float minSize = 0.01f;
        public float maxSize = 0.3f;
        public bool useInitialSizeAsMin = true;
        public bool showDebug = false;

        protected virtual void Awake()
        {
            if (useInitialSizeAsMin == true)
            {
                var pointSize = viewer.GetPointSize();
                if (pointSize != null)
                {
                    minSize = Mathf.Abs((float)pointSize);
                }
                else
                {
                    if (adjustPointSize == true)
                    {
                        adjustPointSize = false;
                        Debug.Log("PointCloudDynamicResolution> Shader doesnt support point size, disabling size adjust", gameObject);
                    }
                }
            }

            // check viewer
            if (viewer.useCommandBuffer == true)
            {
                Debug.LogError("PointCloudDynamicResolution doesnt work when CommandBuffer is enabled!");
            }

        }

        protected int amount = 0;

        protected virtual void Update()
        {
            if ((allowHoldKeyDown && Input.GetKey(decrease)) || Input.GetKeyDown(decrease))
            {
                if (viewer.isNewFormat)
                {
                    amount = (int)(viewer.initialPointsToRead * adjustSpeed);
                }
                else // old format
                {
                    amount = (int)(viewer.totalMaxPoints * adjustSpeed);
                }

                viewer.AdjustVisiblePointsAmount(-amount);
                if (adjustPointSize == true)
                {
                    float l = viewer.totalPoints / (float)viewer.totalMaxPoints;
                    //float l = viewer.totalPoints * 0.01f;
                    //                    viewer.SetPointSize(Mathf.Lerp(maxSize, minSize, l));
                    float pointSize = Mathf.Lerp(maxSize, minSize, l);
                    viewer.SetPointSize(pointSize);

                }
                if (showDebug == true) Debug.Log("Points= " + viewer.totalPoints + " / " + viewer.totalMaxPoints);
            }

            if ((allowHoldKeyDown && Input.GetKey(increase)) || Input.GetKeyDown(increase))
            {
                if (viewer.isNewFormat)
                {
                    amount = (int)(viewer.initialPointsToRead * adjustSpeed);
                }
                else // old format
                {
                    amount = (int)(viewer.totalMaxPoints * adjustSpeed);
                }

                viewer.AdjustVisiblePointsAmount(amount);
                if (adjustPointSize == true)
                {
                    float l = viewer.totalPoints / (float)viewer.totalMaxPoints;
                    float pointSize = Mathf.Lerp(maxSize, minSize, l);
                    viewer.SetPointSize(pointSize);
                }
                if (showDebug == true) Debug.Log("PointCloudDynamicResolution: Points= " + viewer.totalPoints + " / " + viewer.totalMaxPoints);
            }

        }
    }
}
