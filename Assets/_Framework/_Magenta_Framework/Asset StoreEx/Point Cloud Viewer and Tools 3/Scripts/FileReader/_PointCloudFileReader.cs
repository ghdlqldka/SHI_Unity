// Point Cloud Binary Viewer DX11
// reads custom binary file and displays it with dx11 shader
// http://unitycoder.com

#if !UNITY_WEBPLAYER && !UNITY_SAMSUNGTV

using UnityEngine;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using static pointcloudviewer.tools.MainThread;

namespace pointcloudviewer.binaryviewer
{
    //[ExecuteInEditMode] // ** You can enable this, if you want to see DX11 cloud inside editor, without playmode NOTE: only works with V1 .bin and with threading disabled **
    [System.Serializable]
    public class _PointCloudFileReader
    {
        private static string LOG_FORMAT = "<color=#27B7AD><b>[_PointCloudFileReader]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected string assetsPath;
        /*
        [ReadOnly]
        [SerializeField]
        protected string fileName = "StreamingAssets/PointCloudViewerSampleData/sample.bin";
        */
        [ReadOnly]
        [SerializeField]
        protected string filePath;

        [ReadOnly]
        [SerializeField]
        protected string extension;
        public string Extension
        {
            get
            {
                return extension;
            }
        }

        protected bool _abortThread = false;
        public bool AbortThread
        {
            get
            {
                return _abortThread;
            }
            set
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=red>************* AbortThread *************</color>");
                _abortThread = value;
            }
        }
        protected Thread importerThread;

        protected Stopwatch stopwatch;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool _isReady = false;
        public bool IsReady
        {
            get 
            { 
                return _isReady;
            }
            protected set
            {
                _isReady = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool _isLoading = false;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            protected set
            {
                _isLoading = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool _isInitializingBuffers = false;
        public bool IsInitializingBuffers
        {
            get
            {
                return _isInitializingBuffers;
            }
            set
            {
                _isInitializingBuffers = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool _isNewFormat = false; // .ucpc
        public bool IsNewFormat
        {
            get
            {
                return _isNewFormat;
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int _totalPoints = 0;
        public int TotalPoints
        {
            get
            {
                return _totalPoints;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected bool containsRGB = false;
        public bool ContainsRGB
        {
            get
            {
                return containsRGB;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected Bounds cloudBounds;
        public Bounds CloudBounds
        {
            get
            {
                return cloudBounds;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected Vector3[] points; // actual point cloud points array
        public Vector3[] _Points
        {
            get
            {
                return points;
            }

#if false //
            /*protected*/
            set
            {
                points = value;
                Debug.LogWarningFormat(LOG_FORMAT, "_Points has SET!!!!!!!!!!!!!!!!!!!!!!!");
            }
#endif
        }

        [ReadOnly]
        [SerializeField]
        protected Vector3[] pointColors;
        public Vector3[] _PointColors
        {
            get
            {
                return pointColors;
            }
            /*protected*/ set
            {
                pointColors = value;
            }
        }

#if DEBUG
        [Header("===========> DEBUG <===========")]
        [ReadOnly]
        [SerializeField]
        protected bool DEBUG_showDebug = true;
#endif

        protected const int sizeofInt32 = sizeof(System.Int32);

        protected class Options
        {
            public string fileFullPath;
            public bool randomizeArray;

            public Func OnInitDX11BuffersBack;
            public Function OnReadCompleteCallBack;
        }

        public _PointCloudFileReader()
        {
            // Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");
            IsReady = false;
            IsLoading = false;
            IsInitializingBuffers = false;
        }

        public virtual void Read(bool randomizeArray, Function OnReadCompleteCallBack)
        {
            Debug.LogFormat(LOG_FORMAT, "Read()");
        }

    } // class
} // namespace

#endif