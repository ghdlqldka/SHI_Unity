using System.Collections.Generic;
using UnityEngine;
using static EA.Line3D._LineRenderer3D;

namespace _SHI_BA
{
    public enum PathType
    {
        Zigzag,
        Snake,
        Unknown
    }

    [System.Serializable]
    public class BA_MotionPath
    {
        public PathType Type { get; }
        public float NormalOffset { get; }
        public int FaceIndex { get; }
        // public List<Vector3> Positions { get; }
        // public List<Quaternion> Rotations { get; }

        [ReadOnly]
        [SerializeField]
        protected List<_LinePoint> _pointList = new List<_LinePoint>();
        public List<_LinePoint> PointList 
        { 
            get
            {
                return _pointList;
            }
            set
            {
                _pointList = value;
            }
        }

        public Vector3 FaceNormal { get; }
        public Vector3 StartPoint { get; }
        public Vector3 EndPoint { get; }
        public Vector3 WidthDir { get; }
        public Vector3 HeightDir { get; }

        public BA_MotionPath(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir)
        {
            FaceIndex = faceIndex;
            Type = type;
            NormalOffset = normalOffset;

            Debug.Assert(positions.Count == rotations.Count);
            PointList.Clear();
            for (int i = 0; i < positions.Count; i++)
            {
                _LinePoint point = new _LinePoint(positions[i], rotations[i].eulerAngles);
                PointList.Add(point);
            }

            // Rotations = rotations;
            FaceNormal = faceNormal;
            StartPoint = startPoint;
            EndPoint = endPoint;
            WidthDir = widthDir;
            HeightDir = heightDir;
        }
    }

    [System.Serializable]
    public class BA_WeavingPath : BA_MotionPath
    {
        public float WeavingAngle { get; }

        public BA_WeavingPath(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir, float weavingAngle)
            : base(faceIndex, type, normalOffset, positions, rotations, faceNormal, startPoint, endPoint, widthDir, heightDir)
        {
            WeavingAngle = weavingAngle;
        }
    }
}
