// sample script to get points inside box collider and do something with them
// you can clone this and make your own


using PointCloudHelpers;
using pointcloudviewer.binaryviewer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloudViewer.Experimental
{
    public class GetPointsInsideBox : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to PointCloudViewerTilesDX11 component")]
        public PointCloudViewerTilesDX11 viewer;

        [Tooltip("Gameobject with Box collider")]
        public Transform selectionBox;

        [Header("Settings")]
        [Tooltip("If this is regular packed format (XYZ+RGB)")]
        public bool isPackedFormat = false;
        [Tooltip("If this is intensity packed format (XYZ+RGB+INTENSITY)")]
        public bool isIntensityPacked = false;
        [Tooltip("If this is intensity packed format (XYZ+RGB+INTENSITY+CLASSIFICATION)")]
        public bool isClassificationPacked = false;
        [Tooltip("If we want to get intensity from separate file (not packed) *.int")]
        public bool getIntensityFromFile = false;
        [Tooltip("If we want to get classificaion from separate file (not packed) *.cla")]
        public bool getClassificationFromFile = false;

        [Header("Modify Cloud")]
        public bool hidePoints = false;
        public bool setColors = false;
        public Color overrideColor = Color.red;
        [Tooltip("Moves points far away from current position, doesnt really hide them")]

        List<int> changedTilePositions = new List<int>();
        List<int> changedTileColors = new List<int>();

        void Start()
        {
            if (isPackedFormat == true || isIntensityPacked == true || isClassificationPacked == true)
            {
                if (setColors == true)
                {
                    Debug.LogWarning("setColors not supported for packed format");
                }
                if (hidePoints == true)
                {
                    Debug.LogWarning("hidePoints not supported for packed format");
                }
            }

            if (getIntensityFromFile == true)
            {
                if (isPackedFormat == true)
                {
                    Debug.LogWarning("getIntensityFromFile not supported for packed format");
                }
                if (isIntensityPacked == true)
                {
                    Debug.LogWarning("getIntensityFromFile not supported for intensity packed format");
                }
                if (isClassificationPacked == true)
                {
                    Debug.LogWarning("isClassificationPacked not supported for classification packed format");
                }
            }

            viewer.GotPointsInsideBoxEvent -= ResultsCallBack; // unsubscribe just in case
            viewer.GotPointsInsideBoxEvent += ResultsCallBack;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // call function, NOTE that method should be later run in separate thread
                //viewer.RunGetPointsInsideBoxThread(selectionBox); // default way

                viewer.RunGetPointsInsideBoxThread(this, selectionBox); // nick custom
            }
        }

        // TODO add separate method for modifying points
        void ResultsCallBack(List<Tuple<int, int>> data)
        {
            Debug.Log("Found " + data.Count + " points inside box");

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            changedTilePositions.Clear();
            changedTileColors.Clear();

            Vector3 point;
            Vector3 pointColor;
            float pointIntensity = 0;
            float pointClassification = 0;
            int tileIndex = 0;
            int pointIndex = 0;

            int loadedIntensityTileIndex = -1;
            bool hasLoadedIntensity = false;

            // iterate results (res = tuplee of point tile index and point index (in that tile))
            // NOTE this can be slow for large amount of points, could do in another thread
            foreach (var item in data)
            {
                tileIndex = item.Item1;
                pointIndex = item.Item2;
                //Debug.Log("tileIndex: "+tileIndex);

                // if we want load intensity from separate file
                if (getIntensityFromFile == true && isPackedFormat == false && loadedIntensityTileIndex != tileIndex)
                {
                    hasLoadedIntensity = viewer.LoadIntensityTile(tileIndex);
                    loadedIntensityTileIndex = tileIndex;
                }

                // get point from current tile
                if (viewer.useNativeArrays == true)
                {
                    // common values
                    point.x = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3]);
                    point.z = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1 + 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2 + 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3 + 4 + 4]);

                    if (isPackedFormat == true)
                    {
                        Vector3 xr; //class x red
                        Vector3 yg; //int green y
                        
                        Vector2 rx;

                        // r+class+x packed format
                        if (isClassificationPacked == true)
                        {
                            int intPointX = PointCloudMath.BytesToInt(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3]);
                            Vector3 res2 = PointCloudMath.SuperUnpacker(intPointX);

                            xr.x = res2.z; // red
                            xr.y = res2.x; // class
                            xr.z = res2.y; // x

                            rx.x = xr.x;
                            rx.y = xr.z;
                            pointClassification = xr.x;

                            int intPointY = PointCloudMath.BytesToInt(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3 + 4]);
                            Vector3 res3 = PointCloudMath.SuperUnpacker(intPointY);

                            yg.x = res3.x; // green 
                            yg.y = res3.z; // y
                            yg.z = res3.y; // int ** NOTE flipped yg order to keep .y as y

                            pointIntensity = yg.z;
                        }
                        // g+int+y packed format
                        else if (isIntensityPacked == true)
                        {
                            int intPointY = PointCloudMath.BytesToInt(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3 + 4]);
                            Vector3 res2 = PointCloudMath.SuperUnpacker(intPointY);

                            yg.x = res2.x; // green 
                            yg.y = res2.z; // y
                            yg.z = res2.y; // int ** NOTE flipped yg order to keep .y as y

                            pointIntensity = yg.z;
                            rx = PointCloudMath.SuperUnpacker(point.x, viewer.gridSizePackMagic); // x red
                        }
                        else // regular packed format
                        {
                            point.y = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3 + 4]);
                            Vector2 res2 = PointCloudMath.SuperUnpacker(point.y, viewer.gridSizePackMagic);

                            yg.x = res2.x; // green
                            yg.y = res2.y; // y

                            pointIntensity = yg.x;

                            rx = PointCloudMath.SuperUnpacker(point.x, viewer.gridSizePackMagic); // x red
                        }

                        Vector2 bz = PointCloudMath.SuperUnpacker(point.z, viewer.gridSizePackMagic); // z blue

                        point.x = rx.y + viewer.tiles[tileIndex].cellX * viewer.gridSize;
                        point.y = yg.y + viewer.tiles[tileIndex].cellY * viewer.gridSize;
                        point.z = bz.y + viewer.tiles[tileIndex].cellZ * viewer.gridSize;

                        pointColor.x = rx.x;
                        pointColor.y = yg.x;
                        pointColor.z = bz.x;
                    }
                    else // non packed format
                    {
                        point.y = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 1 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 2 + 4], viewer.tiles[tileIndex].pointsNative[pointIndex * 3 * 4 + 3 + 4]);

                        pointColor.x = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 1], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 2], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 3]);
                        pointColor.y = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 1 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 2 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 3 + 4]);
                        pointColor.z = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 4 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 1 + 4 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 2 + 4 + 4], viewer.tiles[tileIndex].colorsNative[pointIndex * 3 * 4 + 3 + 4 + 4]);

                        if (getIntensityFromFile == true && hasLoadedIntensity == true)
                        {
                            pointIntensity = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].intensityNative[pointIndex * 3 * 4], viewer.tiles[tileIndex].intensityNative[pointIndex * 3 * 4 + 1], viewer.tiles[tileIndex].intensityNative[pointIndex * 3 * 4 + 2], viewer.tiles[tileIndex].intensityNative[pointIndex * 3 * 4 + 3]);
                        }

                        // NOTE separate classificaion not supported for non packed format now
                        //if (getClassificationFromFile == true)
                        //{
                        //    pointClassification = PointCloudMath.BytesToFloat(viewer.tiles[tileIndex].classificationNative[pointIndex * 3 * 4], viewer.tiles[tileIndex].classificationNative[pointIndex * 3 * 4 + 1], viewer.tiles[tileIndex].classificationNative[pointIndex * 3 * 4 + 2], viewer.tiles[tileIndex].classificationNative[pointIndex * 3 * 4 + 3]);
                        //}
                    }
                }
                else  // non native array
                {
                    point = viewer.tiles[tileIndex].points[pointIndex];

                    if (isPackedFormat == true)
                    {
                        var rx = PointCloudMath.SuperUnpacker(point.x, viewer.gridSizePackMagic);
                        var gy = PointCloudMath.SuperUnpacker(point.y, viewer.gridSizePackMagic);
                        var bz = PointCloudMath.SuperUnpacker(point.z, viewer.gridSizePackMagic);

                        point.x = rx.y + viewer.tiles[tileIndex].cellX * viewer.gridSize;
                        point.y = gy.y + viewer.tiles[tileIndex].cellY * viewer.gridSize;
                        point.z = bz.y + viewer.tiles[tileIndex].cellZ * viewer.gridSize;

                        pointColor.x = rx.x;
                        pointColor.y = gy.x;
                        pointColor.z = bz.x;
                    }
                    else // non packed format
                    {
                        pointColor = viewer.tiles[tileIndex].colors[pointIndex];
                    }

                    if (getIntensityFromFile == true && hasLoadedIntensity == true)
                    {
                        pointIntensity = viewer.tiles[tileIndex].intensityColors[pointIndex].x;
                    }

                    // NOTE Classification not supported in separate file for non native arrays
                }

                // <<>>
                // NOTE: you can now use POINT and POINTCOLOR or POINTINTENSITY for your own purposes here (or tileIndex, pointIndex) too
                // <<>>

                // for debug only
                //Debug.DrawRay(point, Vector3.up * 1f, new Color(pointIntensity, pointIntensity, pointIntensity, 1), 4);
                //Debug.DrawRay(point, Vector3.up * 1f, new Color(pointClassification, pointClassification, pointClassification, 1), 4);
                //Debug.DrawRay(point, Vector3.up * 1f, new Color(pointColor.x, pointColor.y, pointColor.z, 1), 4);

                ModifyCloud(tileIndex, pointIndex);

            } // foreach found tiles/points

            RefreshTiles();

            stopwatch.Stop();
            if (viewer.showDebug) Debug.LogFormat("ResultsCallBack: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();

        } // ResultsCallBack


        private void ModifyCloud(int tileIndex, int pointIndex)
        {
            // not supported for packed format or native array now
            if (isPackedFormat == true || viewer.useNativeArrays == false) return;

            // if want to override selected point color
            if (setColors == true)
            {
                // set color for this point (directly editing viewer tile point colors array)
                viewer.tiles[tileIndex].colors[pointIndex] = new Vector3(overrideColor.r, overrideColor.g, overrideColor.b);
                changedTileColors.Add(tileIndex);
            }

            // if want to move selected points away from view
            if (hidePoints == true)
            {
                // move point away (directly editing viewer tile points)
                viewer.tiles[tileIndex].points[pointIndex] = Vector3.zero;
                changedTilePositions.Add(tileIndex);
            }
        }

        void RefreshTiles()
        {
            // refresh color tiles
            foreach (var index in changedTileColors)
            {
                viewer.ForceUpdateTileColors(index);
            }
            // refrehs point tiles
            foreach (var index in changedTileColors)
            {
                viewer.ForceUpdateTilePositions(index);
            }
        }

        void OnDestroy()
        {
            viewer.GotPointsInsideBoxEvent -= ResultsCallBack;
        }
    }
}

