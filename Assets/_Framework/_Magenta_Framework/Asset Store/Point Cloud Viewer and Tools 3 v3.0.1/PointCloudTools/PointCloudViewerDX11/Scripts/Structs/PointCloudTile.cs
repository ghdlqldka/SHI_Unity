using PointCloudViewer.Structs;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using Unity.Collections;
#endif
namespace PointCloudViewer.Structs
{
    // TODO compare as class vs struct
    public struct PointCloudTile
    {
        // bounds
        public float minX;
        public float minY;
        public float minZ;
        public float maxX;
        public float maxY;
        public float maxZ;

        // for packed data
        public int cellX;
        public int cellY;
        public int cellZ;

        public Vector3 center;

        public int totalPoints;
        public int loadedPoints;
        public int visiblePoints;

        // TODO no need both
        public bool isLoading;
        public bool isReady;
        public bool isInQueue;

        // viewing
        public Material material;
        public ComputeBuffer bufferPoints;
        public ComputeBuffer bufferColors;
#if UNITY_2019_1_OR_NEWER
    public NativeArray<byte> pointsNative;
    public NativeArray<byte> colorsNative;
    public NativeArray<byte> intensityNative; // TESTING
#endif
        public Vector3[] points;
        public Vector3[] colors;
        public Vector3[] intensityColors;
        //TODO add option to load these at the same time into memory, OR load them when requested in GetPointsInsideBox

        public int[] pointsBytePacked; // XYZRGB 2+2bytes

        // TEST if meshrendering
        public PointCloudMeshTile meshTile;

        // EXPERIMENTAL
        public double averageGPSTime;
        public float overlapRatio;

        // TODO no need these, just use index start-end from root
        //public string filename;
        //public string filenameRGB;
    }
}