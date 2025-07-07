using PointCloudViewer;
using PointCloudViewer.Structs;
using System.Collections.Generic;
using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace pointcloudviewer
{
    public class _AreaSelectExample : AreaSelectExample
    {
        private static string LOG_FORMAT = "<color=#8BBA08><b>[_AreaSelectExample]</b></color> {0}";

        // protected PointCloudManager pointCloudManager;
        protected _PointCloudManager _pointCloudManager
        {
            get
            {
                return pointCloudManager as _PointCloudManager;
            }
        }

        // public PointCloudViewerDX11 tempPointCloudViewerDX11;
        protected _PointCloudViewerDX11 _pointCloudViewerDX11
        {
            get
            {
                return tempPointCloudViewerDX11 as _PointCloudViewerDX11;
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // use singleton from manager
            pointCloudManager = _PointCloudManager.Instance;

            // check separate viewer, needed if use separate cloud to show selected points
            if (createSeparateCloudFromSelection == true && _pointCloudViewerDX11 == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Missing _pointCloudViewerDX11 reference at " + gameObject.name, gameObject);
            }
        }

        protected override void Update()
        {
            // update area selection on F5
            if (Input.GetKeyDown(selectPointsKey))
            {
                // we need to keep track which clouds contributed into selection (since its possible to measure/select from multiple clouds)
                List<int> uniqueClouds = new List<int>();

                if (createSeparateCloudFromSelection == true)
                {
                    Debug.LogFormat(LOG_FORMAT, "@1");
                    // TODO clear previously generated cloud or just overwrite later?
                }
                else // use original points
                {
                    Debug.LogFormat(LOG_FORMAT, "@2");
                    // first invert old selected point colors back to original (if we had something selected)
                    if (selectedPoints != null && selectedPoints.Count > 0)
                    {
                        for (int i = 0, len = selectedPoints.Count; i < len; i++)
                        {
                            CollectedPoint pdata = selectedPoints[i];
                            int cloudIndex = _pointCloudManager._CloudList[pdata.cloudIndex].viewerIndex;
                            _PointCloudViewerDX11 _viewer = _pointCloudManager._Viewers[cloudIndex] as _PointCloudViewerDX11;
                            Vector3 c = _viewer._PointColors[pdata.pointIndex];
                            // restore inverted colors
                            c.x = 1 - c.x;
                            c.y = 1 - c.y;
                            c.z = 1 - c.z;
                            _viewer._PointColors[pdata.pointIndex] = c;

                            // collect clouds that we need to refresh colors for
                            if (uniqueClouds.Contains(cloudIndex) == false)
                            {
                                uniqueClouds.Add(cloudIndex);
                            }
                        }
                    }
                }

                // collect area selection points (child objects of the root)
                List<Vector3> points = new List<Vector3>();
                foreach (Transform t in selectionRoot)
                {
                    points.Add(t.position);
                }

                Debug.LogFormat(LOG_FORMAT, "points.Count : " + points.Count);
                // get selection results form BoxSelect
                selectedPoints = _pointCloudManager.ConvexHullSelectPoints(this.gameObject, points);

                if (selectedPoints != null)
                {
                    Debug.LogFormat(LOG_FORMAT, "Selected " + selectedPoints.Count + " points");
                    if (createSeparateCloudFromSelection == true)
                    {
                        // build new cloud from selection points
                        Vector3[] selectedPointsTemp = new Vector3[selectedPoints.Count];
                        for (int i = 0, len = selectedPoints.Count; i < len; i++)
                        {
                            CollectedPoint pdata = selectedPoints[i];
                            int cloudIndex = pdata.cloudIndex;
                            int viewerIndex = _pointCloudManager._CloudList[cloudIndex].viewerIndex;
                            Vector3 p = ((_PointCloudViewerDX11)_pointCloudManager._Viewers[viewerIndex])._Points[pdata.pointIndex];
                            selectedPointsTemp[i] = p;
                        }

                        // create new cloud on temporary viewer
                        _pointCloudViewerDX11.InitDX11Buffers();
                        // _pointCloudViewerDX11._Points = selectedPointsTemp;
                        _pointCloudViewerDX11.UpdatePointData(selectedPointsTemp);

                        // set custom color for points, NOTE should use special material/shader that uses fixed color AND zoffset?
                        _pointCloudViewerDX11.cloudMaterial.SetColor("_Color", selectedPointColor);
                        // NOTE we take pointsize from first cloud only
                        //_pointCloudViewerDX11.cloudMaterial.SetFloat("_Size", _pointCloudManager.viewers[0].cloudMaterial.GetFloat("_Size"));

                    }
                    else // we are using original cloud points
                    {
                        for (int i = 0, len = selectedPoints.Count; i < len; i++)
                        {
                            // set point colors (but really would be faster to create new owndata cloud for those usually..)
                            var pdata = selectedPoints[i];

                            // get point index from that cloudo
                            int cloudIndex = _pointCloudManager._CloudList[pdata.cloudIndex].viewerIndex;

                            // TODO, if want to use custom color create new cloud or point mesh, otherwise invert color or swap colors
                            _PointCloudViewerDX11 _viewer = _pointCloudManager._Viewers[cloudIndex] as _PointCloudViewerDX11;
                            var c = _viewer._PointColors[pdata.pointIndex];
                            c.x = 1 - c.x;
                            c.y = 1 - c.y;
                            c.z = 1 - c.z;
                            _viewer._PointColors[pdata.pointIndex] = c;

                            // get list of unique clouds that we need to update
                            if (uniqueClouds.Contains(cloudIndex) == false) 
                                uniqueClouds.Add(cloudIndex);
                        }
                    }
                }

                // refresh colors for each existing cloud
                foreach (int cloudIndex in uniqueClouds)
                {
                    ((_PointCloudViewerDX11)_pointCloudManager._Viewers[cloudIndex]).UpdateColorData();
                }


            }
        } // Update

    } // class

}
