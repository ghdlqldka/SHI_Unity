using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using static EA.Line3D._LineRenderer3D;

namespace _SHI_BA
{
    public class BA_PathDataManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[BA_PathDataManager]</b></color> {0}";

        private static BA_PathDataManager _instance;
        public static BA_PathDataManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }


        [ReadOnly]
        [SerializeField]
        protected BA_MotionPath _w1Path;
        public BA_MotionPath w1Path
        {
            get
            {
                return _w1Path;
            }
            set
            {
                _w1Path = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected List<BA_MotionPath> _rPaths;
        public List<BA_MotionPath> rPaths
        {
            get
            {
                return _rPaths;
            }
            set
            {
                _rPaths = value;
                if (_rPaths != null)
                {
                    Debug.Assert(_rPaths.Count == 10);
                }
            }
        }

        public List<BA_MotionPath> generatedPaths;






        public List<_LinePoint> path_stiffnerBot = new List<_LinePoint>();
        public List<_LinePoint> path_stiffnerEdge = new List<_LinePoint>();

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}