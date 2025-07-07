// example code for getting min/max gps time and overlap ratio, and drawing bounds with color based on those values
// press 1 to show gps time, press 2 to show overlap ratio

using PointCloudHelpers;
using pointcloudviewer.binaryviewer;
using UnityEngine;

namespace PointCloudViewer.Extras
{
    public class GPSTimeSample : MonoBehaviour, ITileAction
    {
        public PointCloudViewerTilesDX11 viewer;

        public Gradient colorGradient;

        public bool validateGPSTime = true;
        public double maxGPSTime = 238668;
        public bool validateOverlap = true;
        public float maxOverlapRatio = 0.5f;

        public bool ValidateTile(double avgGPSTime, float overlapRatio)
        {
            bool res = validateGPSTime && (avgGPSTime <= maxGPSTime);
            res = res || (validateOverlap && (overlapRatio <= maxOverlapRatio));
            res = res || (!validateGPSTime && !validateOverlap);
            return res;
        }

        void Start()
        {
            if (viewer == null)
            {
                Debug.LogError("Viewer is not set", gameObject);
                return;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var len = viewer.tiles.Length;

                double minGPSTime = double.MaxValue;
                double maxGPSTime = double.MinValue;

                // loop all tiles to get min/max gps time
                for (int i = 0; i < len; i++)
                {
                    var tile = viewer.tiles[i];
                    var gpsTime = tile.averageGPSTime;
                    if (gpsTime < minGPSTime) minGPSTime = gpsTime;
                    if (gpsTime > maxGPSTime) maxGPSTime = gpsTime;
                    //Debug.Log($"Tile {i} has gpsTime {gpsTime}");
                }

                Debug.Log("minGPSTime: " + minGPSTime + " , maxGPSTime: " + maxGPSTime);

                // remap gps time to color
                for (int i = 0; i < len; i++)
                {
                    var tile = viewer.tiles[i];
                    var gpsTime = tile.averageGPSTime;
                    var t = (float)((gpsTime - minGPSTime) / (maxGPSTime - minGPSTime));
                    var color = colorGradient.Evaluate(t);

                    var tileBounds = new Bounds(tile.center, new Vector3(tile.maxX - tile.minX, tile.maxY - tile.minY, tile.maxZ - tile.minZ));

                    PointCloudTools.DrawBounds(tileBounds, color, 64);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var len = viewer.tiles.Length;

                double minOverlap = double.MaxValue;
                double maxOverlap = double.MinValue;

                // loop all tiles to get min/max gps time
                for (int i = 0; i < len; i++)
                {
                    var tile = viewer.tiles[i];
                    var ratio = tile.overlapRatio;
                    if (ratio < minOverlap) minOverlap = ratio;
                    if (ratio > maxOverlap) maxOverlap = ratio;
                    Debug.Log($"Tile {i} has overlapRatio {ratio}");
                }

                // remap ratio time to color
                for (int i = 0; i < len; i++)
                {
                    var tile = viewer.tiles[i];
                    var ratio = tile.overlapRatio;
                    var t = (float)((ratio - minOverlap) / (maxOverlap - minOverlap));
                    //Debug.Log("r: " + ratio + ", " + minOverlap + ", " + maxOverlap + ", t:" + t);
                    var color = colorGradient.Evaluate(t);
                    var tileBounds = new Bounds(tile.center, new Vector3(tile.maxX - tile.minX, tile.maxY - tile.minY, tile.maxZ - tile.minZ));
                    PointCloudTools.DrawBounds(tileBounds, color, 64);
                }
            }

        } // update
    } // class
}
