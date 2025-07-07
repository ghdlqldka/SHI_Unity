// you can override certain functions in the viewer to get custom behavior

using PointCloudHelpers;
using pointcloudviewer.binaryviewer;
using pointcloudviewer.tools;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace pointcloudviewer.extras
{
    public class CustomTilesViewerV3 : PointCloudViewerTilesDX11
    {
        //[Header("Optional Override Settings")]
        //[Tooltip("If this is regular packed format (XYZ+RGB)")]
        //public bool isPackedFormat = false;
        //[Tooltip("If this is intensity packed format (XYZ+RGB+INTENSITY)")]
        //public bool isIntensityPacked = false;
        //[Tooltip("If we want to get intensity from separate file (not packed)")]
        //public bool getIntensityFromFile = false;

        public delegate void GotPointsInsideBoxCustom(object sender, List<System.Tuple<int, Vector3, Vector3, float, float>> results);
        public event GotPointsInsideBoxCustom GotPointsInsideBoxEventCustom;

        public override void RunGetPointsInsideBoxThread(object sender, Transform box)
        {
            // take renderer bounds and box collider
            var boxbounds = box.GetComponent<Renderer>().bounds;
            var boxCollider = box.GetComponent<BoxCollider>();

            var boxData = new BoxData();
            boxData.boxPos = box.position;
            boxData.boxRotation = box.rotation;
            boxData.boxScale = box.localScale;
            boxData.boxCenter = boxCollider.center;
            boxData.boxSize = boxCollider.size;
            boxData.boxBounds = boxbounds;

            RunGetPointsInBoundsThread(sender, boxData);
        }

        public override void RunGetPointsInBoundsThread(object sender, BoxData box)
        {
            //if (!Teleqo.World.mapIsLoaded)
            //{
            //    return;
            //}
            //PointCloudTools.DrawBounds(box.boxBounds, 15f);

            ParameterizedThreadStart start = new ParameterizedThreadStart(GetPointsInsideBox);
            pointAreaSearchThread = new Thread(start);
            pointAreaSearchThread.IsBackground = true;

            pointAreaSearchThread.Start(new System.Tuple<object, BoxData>(sender, box));
        }

        public override void GetPointsInsideBox(object bounds)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var tuple = (System.Tuple<object, BoxData>)bounds;
            object sender = tuple.Item1;
            BoxData boxData = tuple.Item2;

            // index, point, color, intensity
            List<System.Tuple<int, Vector3, Vector3, float, float>> resTiles = new List<System.Tuple<int, Vector3, Vector3, float, float>>();

            Vector3 pointColor = Vector3.zero;
            float pointIntensity = -1;
            float pointClassification = -1;

            //PointCloudTools.DrawBounds(boxbounds, 10);
            // loop all tiles
            for (int i = 0, len = tiles.Length; i < len; i++)
            {
                if (tiles[i].isReady == false) continue;

                // NOTE checks for AABB, not rotated box
                if (PointCloudMath.AABBIntersectsAABB(boxData.boxBounds, tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ))
                {
                    // check if transform box contains point
                    //var len2 = useNativeArrays ? tiles[i].pointsNative.Length : tiles[i].points.Length;

                    // NOTE your code used totalPoints, which might cause issues, if not all points are NOT loaded (but if you know that all points are loaded, then you can use that)
                    int len2 = tiles[i].loadedPoints;

                    for (int j = 0; j < len2; j += pointInsideBoxPrecision)
                    {
                        Vector3 point;
                        pointColor = Vector3.zero;
                        pointIntensity = -1;
                        pointClassification = -1;

#if UNITY_2019_1_OR_NEWER
                        if (useNativeArrays == true)
                        {
                            // TODO calculate index once
                            point.z = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4 + 4]);

                            if (isPackedColors == true)
                            {
                                Vector3 xr; //class x red
                                Vector3 yg;
                                //Vector2 rx;

                                // r+class+x packed format
                                if (isClassificationPacked == true)
                                {
                                    // get x
                                    int intPointX = PointCloudMath.BytesToInt(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                    Vector3 res2 = PointCloudMath.SuperUnpacker(intPointX);

                                    xr.x = res2.x; // red
                                    xr.y = res2.z; // x
                                    xr.z = res2.y; // classification ** NOTE flipped yg order to keep .y as y FOR Y only!! next

                                    pointColor.x = xr.x; // red
                                    pointClassification = xr.z;
                                    point.x = xr.y + tiles[i].cellX * gridSize; // x pos

                                    // get y
                                    int intPointY = PointCloudMath.BytesToInt(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                    Vector3 res3 = PointCloudMath.SuperUnpacker(intPointY);

                                    yg.x = res3.x; // green 
                                    yg.y = res3.z; // y
                                    yg.z = res3.y; // int ** NOTE flipped yg order to keep .y as y

                                    pointColor.y = xr.x; // green
                                    pointIntensity = yg.z; // int
                                    point.y = yg.y + tiles[i].cellY * gridSize; // y
                                }
                                else if (isIntPacked == true)
                                {
                                    int intPointY = PointCloudMath.BytesToInt(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);

                                    Vector3 res = PointCloudMath.SuperUnpacker(intPointY);
                                    yg.x = res.x; // green
                                    yg.y = res.y; // int

                                    point.y = res.z + tiles[i].cellY * gridSize; // y

                                    pointColor.y = yg.x; // green
                                    pointIntensity = yg.y;

                                    point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                    xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                    point.x = xr.y + tiles[i].cellX * gridSize;
                                    pointColor.x = xr.x; // red
                                }
                                else
                                {
                                    point.y = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                    Vector2 res = PointCloudMath.SuperUnpacker(point.y, gridSizePackMagic);
                                    yg.x = res.x; // green
                                    yg.y = res.y; // y

                                    pointColor.y = yg.x;
                                    point.y = yg.y + tiles[i].cellY * gridSize; // y

                                    point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                    xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                    point.x = xr.y + tiles[i].cellX * gridSize;
                                    pointColor.x = xr.x; // red
                                }

                                // need to unpack to get proper xyz
                                Vector2 zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                                pointColor.z = zb.x; // blue
                            }
                            else // not using packed colors
                            {
                                point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                point.y = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                point.z = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4 + 4]);

                                pointColor.x = PointCloudMath.BytesToFloat(tiles[i].colorsNative[j * 3 * 4], tiles[i].colorsNative[j * 3 * 4 + 1], tiles[i].colorsNative[j * 3 * 4 + 2], tiles[i].colorsNative[j * 3 * 4 + 3]);
                                pointColor.y = PointCloudMath.BytesToFloat(tiles[i].colorsNative[j * 3 * 4 + 4], tiles[i].colorsNative[j * 3 * 4 + 1 + 4], tiles[i].colorsNative[j * 3 * 4 + 2 + 4], tiles[i].colorsNative[j * 3 * 4 + 3 + 4]);
                                pointColor.z = PointCloudMath.BytesToFloat(tiles[i].colorsNative[j * 3 * 4 + 4 + 4], tiles[i].colorsNative[j * 3 * 4 + 1 + 4 + 4], tiles[i].colorsNative[j * 3 * 4 + 2 + 4 + 4], tiles[i].colorsNative[j * 3 * 4 + 3 + 4 + 4]);

                                // getting data from external file not supported here, unless add that loading part like in GetPointsInsideBox.cs 
                                //if (getIntensityFromFile == true && hasLoadedIntensity == true)
                                //{
                                //    pointIntensity = PointCloudMath.BytesToFloat(tiles[i].intensityNative[j * 3 * 4], tiles[i].intensityNative[j * 3 * 4 + 1], tiles[i].intensityNative[j * 3 * 4 + 2], tiles[i].intensityNative[j * 3 * 4 + 3]);
                                //}
                            }
                        }
                        else // not using native arrays
#endif
                        {
                            point = tiles[i].points[j];

                            if (isPackedColors == true)
                            {
                                Vector3 yg;

                                // TODO add classification here also!

                                if (isIntPacked == true)
                                {
                                    // NOTE this is not supported! (it is INT, but we have read it as float in regular array)
                                    Vector3 res = PointCloudMath.SuperUnpacker((int)point.y);
                                    yg.x = res.x; // green
                                    yg.y = res.y; // int
                                    point.y = res.z + tiles[i].cellY * gridSize; // y

                                    pointIntensity = yg.y;
                                }
                                else
                                {
                                    Vector2 res = PointCloudMath.SuperUnpacker(point.y, gridSizePackMagic);
                                    yg.x = res.x;
                                    yg.y = res.y;
                                    point.y = yg.y + tiles[i].cellY * gridSize;

                                    pointColor.y = yg.x;
                                }

                                var xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                var zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);

                                point.x = xr.y + tiles[i].cellX * gridSize;
                                point.z = zb.y + tiles[i].cellZ * gridSize;

                                pointColor.x = xr.x; // red
                                pointColor.z = zb.x; // blue
                            } // using packed colors
                        } // not using native arrays

                        if (PointCloudMath.IsPointInsideBoxCollider(point, boxData.boxPos, boxData.boxRotation, boxData.boxScale, boxData.boxCenter, boxData.boxSize))
                        {
                            resTiles.Add(new System.Tuple<int, Vector3, Vector3, float, float>(i, point, pointColor, pointIntensity, pointClassification));
                        }
                    }
                }
                else
                {
                    //PointCloudTools.DrawMinMaxBounds(tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ, 2);
                }
            }

            //MainThread.Call(GetPointsInsideBoxCallBack, resTiles);
            MainThread.Call(GetPointsInsideBoxCallBack, new System.Tuple<object, List<System.Tuple<int, Vector3, Vector3, float, float>>>(sender, resTiles));

            stopwatch.Stop();
            Debug.Log("(v3) GetPointsInsideBox: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Reset();

            if (pointAreaSearchThread != null && pointAreaSearchThread.IsAlive == true) pointAreaSearchThread.Abort();
        } // GetPointsInsideBox

        public override void GetPointsInsideBoxCallBack(object a)
        {

            // get data from thread (we are now in main thread)
            var tuple = (System.Tuple<object, List<System.Tuple<int, Vector3, Vector3, float, float>>>)a;
            object sender = tuple.Item1;
            List<System.Tuple<int, Vector3, Vector3, float, float>> resTiles = tuple.Item2;

            Debug.Log("GetPointsInsideBoxCallBack results count: " + resTiles.Count);

            // NOTE this call is not necessary, since you already have the data in resTiles (you can use it directly here, or even do that in GetPointsInsideBox earlier, if need to prcess it further)
            if (GotPointsInsideBoxEventCustom != null) GotPointsInsideBoxEventCustom(sender, resTiles);

            // example for how to use results data
            foreach (var item in resTiles)
            {
                var point = item.Item2;
                var pointColor = item.Item3;
                var pointIntensity = item.Item4;
                var pointClassification = item.Item5;

                //if (pointIntensity > -1) Debug.Log("Intensity val: " + ((byte)Mathf.Round(pointIntensity * 255)));
                if (pointClassification > -1) Debug.Log("Classification val: " + ((byte)Mathf.Round(pointClassification * 255)));

                // for debugging, show point colors: 
                Debug.DrawRay(point, Vector3.up * 1f, new Color(pointColor.x, pointColor.y, pointColor.z, 1), 4);
                //Debug.DrawRay(point, Vector3.up * 1f, new Color(pointIntensity, pointIntensity, pointIntensity, 1), 4);
                //Debug.DrawRay(point, Vector3.up * 1f, new Color(pointClassification, pointClassification, pointClassification, 1), 4);

            }

        } // GetPointsInsideBoxCallBack

    } // Overrider
}
