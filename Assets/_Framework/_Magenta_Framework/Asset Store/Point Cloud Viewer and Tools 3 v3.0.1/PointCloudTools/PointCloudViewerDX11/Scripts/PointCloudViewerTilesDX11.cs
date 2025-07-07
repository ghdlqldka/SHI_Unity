// Point Cloud Binary Viewer DX11 with Tiles (v3)
// http://unitycoder.com

using UnityEngine;
using System.IO;
using System.Threading;
using PointCloudHelpers;
using System;
using System.Collections;
using Priority_Queue;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections.Generic;
using PointCloudViewer.Structs;
using UnityEngine.Events;
using pointcloudviewer.tools;



#if UNITY_2019_1_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace pointcloudviewer.binaryviewer
{
    public class PointCloudViewerTilesDX11 : MonoBehaviour
    {
        [Header("Load")]
        public string rootFile = "";

        [Header("Settings")]
        public bool loadAtStart = true;
        [Tooltip("Use PointCloudColorSizeDX11v2.mat to get started, then experiment with other materials if needed")]
        public Material cloudMaterial;
        [Tooltip("Create copy of the material. Must enable if using multiple viewers with the same material")]
        public bool instantiateMaterial = false; // set True if using multiple viewers
#if UNITY_2019_1_OR_NEWER
        [Tooltip("Use native arrays (2019.1 or newer only)")]
        public bool useNativeArrays = false;
        [Tooltip("If using native arrays, memory can be released for far away tiles")]
        public bool releaseTileMemory = false;
#endif
        [Header("Visibility")]
        [Tooltip("Enable this if you have multiple cameras and only want to draw in MainCamera")]
        public bool renderOnlyMainCam = false;
        [Tooltip("Disable this, if you want to hide all points")]
        public bool renderPoints = true;

        protected Camera cam;
        protected string applicationStreamingAssetsPath;

        // cullingmanager
        [Header("LOD")]
        [Tooltip("All tiles below this distance to camera will have 100% points")]

        public float startDist = 10;
        [Tooltip("All tiles after this distance will be culled out (and tiles in between this and startdist will have % of points reduced")]
        public float endDist = 250;
        int lodSteps = 100;
        [Tooltip("If disabled, uses Linear falloff, if enabled uses faster falloff (points disappear faster, good for dense clouds)")]
        public bool useStrongFalloff = true;

        // TODO allow setting better distances, or use grid size?
        float[] cullingBandDistances = new float[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200, float.PositiveInfinity };
        //float[] cullingBandDistances = new float[] { 8, 16, 32, 64, 128, 256, float.PositiveInfinity };
        CullingGroup cullGroup;

        [Tooltip("Make this 0 if you want to render all tiles, even if it has only 1 point. Make this value bigger to ignore tiles with less than x points")]
        public int minimumTilePointCount = 256;

        [Header("Rendering")]
        [Tooltip("1 = send data in big chunk (can cause spike), 32=send within 32 slices (smaller or non-noticeable spikes, but tiles appear bit slower)")]
        [Range(0, 48)]
        public int gpuUploadSteps = 16; // bigger value means, point data upload is spread to more frames (less laggy, but takes longer to appear), good values: 1 - 18
        [Tooltip("Global tile resolution multiplier: 1 = Keep original resolution, 0.5 = half resolution, 0 = 0 points visible. Resolution is updated only during tile update (not instantly)")]
        [Range(0f, 1f)]
        public float tileResolution = 1f;
        [Tooltip("Enable global point size multiplier (Not supported by most shaders, requires _SizeMultiplier variable in shader)")]
        public bool useSizeMultiplier = false;
        [Tooltip("Global point size multiplier: 1 = Keep original size, 0.5 = half size. NOTE: Requires shader with SizeMultiplier parameter!")]
        public float pointSizeMultiplier = 1f;

        [Header("SRP")]
        public bool useURPCustomPass = false;
        public bool useHDRPCustomPass = false;

        [Header("Advanced")]
        [Tooltip("How many threads to use for loading tiles. Warning: doesnt check available thread counts. Default value is 2")]
        [Range(1, 16)]
        public int loaderThreadCount = 2;
        [Tooltip("Force Garbage Collection after loading")]
        public bool forceGC = false;
        [Tooltip("1 = full resolution, 10 = iterate every 10th point")]
        [Range(1, 512)]
        public int pointPickingResolution = 1;
        // how far can pick points
        public int maxPickDistance = 256;
        [Tooltip("Show extra information in the console")]
        public bool showDebug = false;
        public bool measureInitialLoadTime = true;
        [Header("LOD Tiles")]
        [Tooltip("Maximum tile point count (only this amount will be loaded for tile)")]
        public bool limitTilePoints = false;
        public int maxPointsPerTile = 64000;
        [Tooltip("Limit tile point count")]
        public bool clampLodTilePointCounts = false;
        [Tooltip("How many max points nearest tiles can have")]
        public int maxLodNearTilePointCount = 10000;
        [Tooltip("How many max points further away tiles can have")]
        public int maxLodTilePointCount = 1000000;

        [Header("Experimental")]
        [Tooltip("Warning: Does not support colors! Use regular mesh renderers. Remember to use assign mesh prefab, with suitable material (CloudMaterial is not used)")]
        // NOTE: not enabled in this version
        public bool useMeshRendering = false;
        [Tooltip("Prefab for mesh tiles")]
        public MeshFilter meshPrefab;
        [Tooltip("How many mesh tiles to update per frame")]
        public int meshUpdatesPerFrame = 64;
        //List<PointCloudMeshTile> meshPool;
        //int maxTiles = 32;

        SimplePriorityQueue<int> meshUpdateQueue = new SimplePriorityQueue<int>();
        protected int[] precalculatedIndices;

        public bool useIntensityFile = false;
        public bool useClassificationFile = false;

        public bool useOnTileAppearEvent = false; // TESTING: call event when tile is ready to appear next frame
        [SerializeField] private MonoBehaviour ITileActionInstance;

        private ITileAction MyInterfaceInstance => ITileActionInstance as ITileAction;


        // events
        public delegate void OnLoadComplete(string filename);
        public event OnLoadComplete OnLoadingComplete;

        public UnityEvent<int> LoaderQueueStatus;
        bool initialLoaderTimerEnded = false;

        [HideInInspector]
        public PointCloudTile[] tiles;

        public float gridSize = 5;
        protected int packMagic = 64; //64 = is default in packer

        // Threading
        protected bool abortReaderThread = false;


        struct ThreadInfo
        {
            public int tempIndex;
            public bool isInitializingBuffers;
            public Thread importerThread;
            public SimplePriorityQueue<int> loadQueue;
            public bool threadRunning;
            public byte[] dataPoints;
        };
        ThreadInfo[] threadInfos;
        internal Thread pointAreaSearchThread;

        // points inside box
        internal int pointInsideBoxPrecision = 1;

        public delegate void GotPointsInsideBox(List<Tuple<int, int>> results);
        public virtual event GotPointsInsideBox GotPointsInsideBoxEvent;

        public struct BoxData
        {
            public Vector3 boxPos;
            public Quaternion boxRotation;
            public Vector3 boxScale;
            public Vector3 boxCenter;
            public Vector3 boxSize;
            public Bounds boxBounds;
        }

        protected void InitThreading()
        {
            threadInfos = new ThreadInfo[loaderThreadCount];

            for (int n = 0; n < loaderThreadCount; n++)
            {
                threadInfos[n].tempIndex = 0;
                threadInfos[n].loadQueue = new SimplePriorityQueue<int>();
                threadInfos[n].isInitializingBuffers = false;
                threadInfos[n].threadRunning = false;
                threadInfos[n].dataPoints = null;
            }
        }

        protected System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        int bufID = Shader.PropertyToID("buf_Points");
        int bufColorID = Shader.PropertyToID("buf_Colors");

        protected const char sep = '|';

        protected Bounds cloudBounds;
        protected Vector3 cloudOffset;
        BoundingSphere[] boundingSpheres;

        protected int v3Version = -1;

        protected long totalPointCount = 0;
        [HideInInspector]
        public int tilesCount = 0;

        protected bool rootLoaded = false;
        protected bool initIsReady = false; // not really fully ready, but done init stuff and probably still loading tiles

        // point picking
        public delegate void PointSelected(Vector3 pointPos);
        public event PointSelected PointWasSelected;

        // SRP
        public static List<PointCloudViewerTilesDX11> RegisteredViewers = new List<PointCloudViewerTilesDX11>();

        protected virtual void Awake()
        {
            applicationStreamingAssetsPath = Application.streamingAssetsPath;
            FixMainThreadHelper();
        }

        // init
        protected virtual void Start()
        {
            cam = Camera.main;

            if (cam == null) { Debug.LogError("Camera Main is missing..", gameObject); }

            // validate URP and HDRP
            if (useURPCustomPass == true && useHDRPCustomPass == true)
            {
                Debug.LogError("URP and HDRP custom passes cannot be used at the same time, please disable one of them");
            }

            //            if (useURPCustomPass == true)
            //            {
            //#if !URP_INSTALLED
            //                Debug.LogError("URP Custom Pass is enabled, but URP is not installed. Please disable URP Custom Pass or install URP");
            //#endif
            //            }

            //            if (useHDRPCustomPass == true)
            //            {
            //#if !HDRP_INSTALLED
            //                Debug.LogError("HDRP Custom Pass is enabled, but HDRP is not installed. Please disable HDRP Custom Pass or install HDRP");
            //#endif
            //            }



            InitThreading();

            // create material clone, so can view multiple clouds
            if (instantiateMaterial == true)
            {
                cloudMaterial = new Material(cloudMaterial);
            }

            if (useMeshRendering == true)
            {
                if (useNativeArrays == true)
                {
                    Debug.LogError("Native arrays are not supported with mesh rendering!");
                }

                if (overridePosition == true)
                {
                    Debug.LogError("Override Position is not yet supported with mesh rendering");
                }

                // TODO no need? if they are updated on demand in oncull..?
                StartCoroutine(MeshUpdater());
            }

            if (loadAtStart == true)
            {
                abortReaderThread = false;

                // all these are required to do
                rootLoaded = LoadRootFile(rootFile);
                if (rootLoaded == true) StartCoroutine(InitCullingSystem());
            }

            // add warnings if bad setttings
#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == false && releaseTileMemory == true)
            {
                Debug.LogWarning("useNativeArrays is not enabled, but releaseTileMemory is enabled - Cannot release memory for managed memory tiles");
            }
#endif
        } // Start

        //public MeshPrefabPool poolManager;
        // TODO use thread and new mesh stuff, only works with packed color for now
        // TODO use thread and new mesh stuff, doesnt work yet until mesh layout uses vector3 for color
        protected IEnumerator MeshUpdater()
        {
            while (true)
            {
                for (int i = 0; i < meshUpdatesPerFrame; i++)
                {

                    if (meshUpdateQueue.Count > 0)
                    {
                        // FIXME problem here if its still loading or processing?

                        var meshIndex = meshUpdateQueue.Dequeue();

                        //if (tiles[meshIndex].visiblePoints == 0)
                        //{
                        //    continue;
                        //}

                        tiles[meshIndex].meshTile.mesh.vertices = tiles[meshIndex].points;

                        // TODO handle these
                        //if (tiles[meshIndex].isInQueue || tiles[meshIndex].isLoading || tiles[meshIndex].isReady == false) Debug.Log(meshIndex + " is loading or in queue or not ready");


                        // TODO handle native array colors
                        // TODO try to replace with some better array copy, or convert in load? or use shader array for colors?

                        // FIXME too slow
                        //tiles[meshIndex].meshTile.mesh.colors = new Color[tiles[meshIndex].points.Length];
                        //for (int i = 0, length = tiles[meshIndex].points.Length; i < length; i++)
                        //{
                        //    var v = tiles[meshIndex].colors[i];
                        //    tiles[meshIndex].meshTile.mesh.colors[i] = new Color(v.x, v.y, v.z, 1);
                        //}

                        //tiles[meshIndex].meshTile.mesh.colors = tiles[meshIndex].colors; // NOTE cannot have colors, since it wants Color[] we have Vector3[]

                        // TODO no need to set these everytime? or slice, or they are preset?? NOTE can be just 3?? bug this resets bounds
                        // NOTE settings indices count affects how many points get drawn!
                        //tiles[meshIndex].meshTile.mesh.SetIndices(indices.Take(tiles[meshIndex].loadedPoints).ToArray(), MeshTopology.Points, 0, false);
                        // TODO could also cull vertex in shader, if vertexid is > visible points

                        //tempIndices = new int[tiles[meshIndex].visiblePoints];
                        //tempIndices = new int[tiles[meshIndex].visiblePoints];
                        // TODO dont reinit if no need local array
                        int[] tempIndices = new int[tiles[meshIndex].visiblePoints];
                        //int[] tempIndices = new int[tiles[meshIndex].points.Length];

                        // broken?
                        //Buffer.BlockCopy(indices, 0, tempIndices, 0, tiles[meshIndex].visiblePoints);
                        // NOTE this works, visible points is broken?
                        Buffer.BlockCopy(precalculatedIndices, 0, tempIndices, 0, tempIndices.Length);

                        //tiles[meshIndex].meshTile.mesh.SetIndices(tempIndices, MeshTopology.Points, 0, false);
                        tiles[meshIndex].meshTile.mesh.SetIndices(tempIndices, MeshTopology.Points, 0, true);

                        // needs this or cull off?
                        tiles[meshIndex].meshTile.mesh.bounds = new Bounds(tiles[meshIndex].center, new Vector3(tiles[meshIndex].maxX - tiles[meshIndex].minX, tiles[meshIndex].maxY - tiles[meshIndex].minY, tiles[meshIndex].maxZ - tiles[meshIndex].minZ));

                        //if (meshIndex == 0)
                        //{
                        //    Debug.Log("loadedpoints:" + tiles[meshIndex].loadedPoints + " tempIndices " + tempIndices.Length + " points: " + tiles[meshIndex].points.Length);

                        //    //for (int i = 0; i < 100; i++)
                        //    //{
                        //    //    Debug.Log("vert: " + i + " " + tiles[meshIndex].meshTile.mesh.vertices[i]);
                        //    //    //Debug.DrawRay(tiles[meshIndex].meshTile.mesh.vertices[i], Vector3.up * 0.2f, Color.red, 3);
                        //    //    Debug.DrawRay(tiles[meshIndex].meshTile.mesh.vertices[i], Vector3.up * 0.2f, Color.red, 3);
                        //    //}

                        //}

                        //tiles[meshIndex].meshTile.mesh.triangles = indices;
                    } // if queue > 0
                }
                yield return 0;
            } // while queue
        } // MeshUpdater

        // TODO make enum
        protected bool isPackedColors = false;
        protected bool isIntPacked = false;
        protected bool isClassificationPacked = false;
        protected bool useLossyBytePacking = false;

        protected string[] filenames;
        protected string[] filenamesRGB;
        readonly string rgbExtension = ".rgb";
        readonly string intExtension = ".int";
        readonly string classificationExtension = ".cla";

        public float gridSizePackMagic = 0;

        public bool overridePosition = false;
        public Vector3 offsetPosition = Vector3.zero;
        protected int largestTilePointCount = 0;

        private void OnEnable()
        {
            //if (useURPCustomPass == false || useHDRPCustomPass == false) return;
            if (!RegisteredViewers.Contains(this)) RegisteredViewers.Add(this);
        }

        private void OnDisable()
        {
            //if (useURPCustomPass == false || useHDRPCustomPass == false) return;
            RegisteredViewers.Remove(this);
        }

        // TODO this could run in a separate thread
        protected virtual bool LoadRootFile(string filePath)
        {
            initIsReady = false;

            if (measureInitialLoadTime == true)
            {
                stopwatch.Reset();
                stopwatch.Start();
                StartCoroutine(ProfileLoaderStatus());
            }

            if (Path.IsPathRooted(filePath) == false)
            {
                filePath = Path.Combine(applicationStreamingAssetsPath, filePath);
            }

            if (PointCloudTools.CheckIfFileExists(filePath) == false)
            {
                Debug.LogError("File not found: " + filePath);
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".pcroot")
            {
                Debug.LogError("File is not V3 root file (.pcroot extension is required): '" + filePath + "'");
                return false;
            }

            StartWorkerThreads();

            Debug.Log("(Tiles Viewer) Loading root file: " + filePath);
            var rootData = File.ReadAllLines(filePath);
            var rootFolder = Path.GetDirectoryName(filePath);

            // get global settings from first row : version | gridsize | totalpointcount | boundsx | boundsy | boundsz | autooffsetx | autooffsety | autooffsetz

            // loop until no more # comments (or end of file)
            int rowCountIndex = 0;
            while (rowCountIndex < rootData.Length && rootData[rowCountIndex].StartsWith("#"))
            {
                rowCountIndex++;
            }

            var globalData = rootData[rowCountIndex].Split(sep);
            rowCountIndex++;

            if (globalData != null && globalData.Length >= 9)
            {
                v3Version = int.Parse(globalData[0], CultureInfo.InvariantCulture);

                if (v3Version < 1 || v3Version > 5)
                {
                    Debug.LogError("v3 header version (" + v3Version + ") is not supported in this viewer!");
                    return false;
                }

                isPackedColors = false;
                isIntPacked = false;
                isClassificationPacked = false;

                // packed
                if (v3Version == 2)
                {
                    isPackedColors = true;
                    Debug.LogWarning("(Tiles Viewer) V3 format #2 detected: Packed colors (Make sure you use material that supports PackedColors)");
                }

                if (v3Version == 3)
                {
                    //isPackedColors = true;
                    useLossyBytePacking = true;
                    Debug.LogWarning("(Tiles Viewer) V3 format #3 detected: Bytepacked data (Make sure you use material that supports BytePacking)");
                }

                if (v3Version == 4)
                {
                    isPackedColors = true;
                    isIntPacked = true;
                    Debug.LogWarning("(Tiles Viewer) V3 format #4 detected: XYZ+RGB+INT data (Make sure you use material that supports INT Packing)");
                }

                if (v3Version == 5)
                {
                    isPackedColors = true;
                    isClassificationPacked = true;
                    Debug.LogWarning("(Tiles Viewer) V3 format #5 detected: XYZ+RGB+INT+CLASSIFICATION data (Make sure you use material that supports INT+CLASS Packing)");
                }

                gridSize = float.Parse(globalData[1], CultureInfo.InvariantCulture);
                totalPointCount = long.Parse(globalData[2], CultureInfo.InvariantCulture);
                Debug.Log("(Tiles Viewer) Total point count = " + totalPointCount + " (" + PointCloudTools.HumanReadableCount(totalPointCount) + ")");
                var minX = float.Parse(globalData[3], CultureInfo.InvariantCulture);
                var minY = float.Parse(globalData[4], CultureInfo.InvariantCulture);
                var minZ = float.Parse(globalData[5], CultureInfo.InvariantCulture);
                var maxX = float.Parse(globalData[6], CultureInfo.InvariantCulture);
                var maxY = float.Parse(globalData[7], CultureInfo.InvariantCulture);
                var maxZ = float.Parse(globalData[8], CultureInfo.InvariantCulture);

                if (showDebug == true) Debug.Log("(Tiles Viewer) minXYZ to maxXYZ = " + minX + " - " + minY + " - " + minZ + " to " + maxX + " - " + maxY + " - " + maxZ);

                var center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
                var size = new Vector3(Mathf.Abs(maxX) + Mathf.Abs(minX), Mathf.Abs(maxY) + Mathf.Abs(minY), Mathf.Abs(maxZ) + Mathf.Abs(minZ));

                if (overridePosition == true)
                {
                    center.x += offsetPosition.x;
                    center.y += offsetPosition.y;
                    center.z += offsetPosition.z;

                    //minX += offsetPosition.x;
                    //minY += offsetPosition.y;
                    //minZ += offsetPosition.z;

                    //maxX += offsetPosition.x;
                    //maxY += offsetPosition.y;
                    //maxZ += offsetPosition.z;
                }

                cloudBounds = new Bounds(center, size);

                var autoOffsetX = float.Parse(globalData[9], CultureInfo.InvariantCulture);
                var autoOffsetY = float.Parse(globalData[10], CultureInfo.InvariantCulture);
                var autoOffsetZ = float.Parse(globalData[11], CultureInfo.InvariantCulture);

                if (v3Version == 2)
                {
                    packMagic = int.Parse(globalData[12], CultureInfo.InvariantCulture);
                }

                cloudOffset = new Vector3(autoOffsetX, autoOffsetY, autoOffsetZ);

                if (showDebug == true) Debug.Log("(Tiles Viewer) gridsize = " + gridSize + " packMagic = " + packMagic);
                if (showDebug == true) Debug.Log("(Tiles Viewer) Bounds = " + cloudBounds);

                if (showDebug == true) PointCloudTools.DrawBounds(cloudBounds, 64);
            }
            else
            {
                Debug.LogError("Failed to parse global values from " + filePath);
                return false;
            }

            // check how many comment lines with for loop
            //int totalCommentsOrEmptyRows = 0;
            for (int ii = rowCountIndex; ii < rootData.Length; ii++)
            {
                if (rootData[ii].StartsWith("#") || string.IsNullOrEmpty(rootData[ii]))
                {
                    rowCountIndex++;
                    //Debug.Log("skip row : "+ rootData[ii]);
                }
            }

            tilesCount = rootData.Length - rowCountIndex;
            tiles = new PointCloudTile[tilesCount];
            //tilesCount = tiles.Length;

            Debug.Log("(Tiles Viewer) Found " + tilesCount + " tiles");
            if (tilesCount <= 0)
            {
                Debug.LogError("Failed parsing V3 tiles root, no tiles found! Check this file in notepad, does it contain data? Usually this happens if your conversion scaling is wrong (not scaling to smaller), one cell only gets few points.." + filePath);
            }

            // arrays for filenames
            filenames = new string[tilesCount];
            filenamesRGB = new string[tilesCount];

            // get data, start from next row
            int items = 0;
            //int actualRows = 0;
            for (int tileRow = rowCountIndex; tileRow < rootData.Length; tileRow++)
            {
                // skip comments with continue and increment actual row index
                if (rootData[tileRow].StartsWith("#")) continue;
                //actualRows++;
                //Debug.Log("row: "+ items+ " : " + rootData[tileRow]);
                var row = rootData[tileRow].Split('|');

                var t = new PointCloudTile();

                //t.filename = Path.Combine(rootFolder, row[0]);
                filenames[items] = Path.Combine(rootFolder, row[0]);

                //if (isPackedColors == false) t.filenameRGB = Path.Combine(rootFolder, row[0] + ".rgb");
                //if (isPackedColors == false && useLossyBytePacking == false) filenamesRGB[actualRows] = Path.Combine(rootFolder, row[0] + ".rgb");
                if (isPackedColors == false && useLossyBytePacking == false)
                {
                    filenamesRGB[items] = Path.Combine(rootFolder, row[0]);// + ".rgb");
                }

                if (limitTilePoints)
                {
                    t.totalPoints = Mathf.Min(int.Parse(row[1], CultureInfo.InvariantCulture), maxPointsPerTile);
                }
                else
                {
                    t.totalPoints = int.Parse(row[1], CultureInfo.InvariantCulture);
                }

                // take largest count
                if (t.totalPoints > largestTilePointCount) largestTilePointCount = t.totalPoints;

                //Debug.Log(filenames[rowIndex]+" totalPoints " + t.totalPoints);
                t.visiblePoints = 0;
                t.loadedPoints = 0;

                // tile bounds
                t.minX = float.Parse(row[2], CultureInfo.InvariantCulture);
                t.minY = float.Parse(row[3], CultureInfo.InvariantCulture);
                t.minZ = float.Parse(row[4], CultureInfo.InvariantCulture);
                t.maxX = float.Parse(row[5], CultureInfo.InvariantCulture);
                t.maxY = float.Parse(row[6], CultureInfo.InvariantCulture);
                t.maxZ = float.Parse(row[7], CultureInfo.InvariantCulture);

                // if offset override
                if (overridePosition == true)
                {
                    t.minX += offsetPosition.x;
                    t.minY += offsetPosition.y;
                    t.minZ += offsetPosition.z;

                    t.maxX += offsetPosition.x;
                    t.maxY += offsetPosition.y;
                    t.maxZ += offsetPosition.z;
                }

                if (isPackedColors == true || useLossyBytePacking == true)
                {
                    t.cellX = int.Parse(row[8], CultureInfo.InvariantCulture);
                    t.cellY = int.Parse(row[9], CultureInfo.InvariantCulture);
                    t.cellZ = int.Parse(row[10], CultureInfo.InvariantCulture);
                }

                // optional tile average gps time
                if (row.Length > 11)
                {
                    t.averageGPSTime = double.Parse(row[11], CultureInfo.InvariantCulture);

                    if (row.Length > 12)
                    {
                        t.overlapRatio = float.Parse(row[12], CultureInfo.InvariantCulture);
                    }
                }

                t.center = new Vector3((t.minX + t.maxX) * 0.5f, (t.minY + t.maxY) * 0.5f, (t.minZ + t.maxZ) * 0.5f);
                //Debug.Log("t.minX=" + t.minX + " row[2]=" + row[2]);

                t.isLoading = false;
                t.isReady = false;
                t.material = new Material(cloudMaterial);

                // TODO dont create all, uses memory? use pooling..
                if (useMeshRendering == true)
                {
                    // create mesh
                    var mesh = new Mesh();
                    mesh.MarkDynamic(); // why?
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    //mesh.SetIndices(indices, MeshTopology.Points, 0, false);
                    mesh.bounds = new Bounds(t.center, new Vector3(t.maxX - t.minX, t.maxY - t.minY, t.maxZ - t.minZ));
                    //mesh.bounds = 

                    //if (i < 10)
                    //{
                    //    PointCloudTools.DrawBounds(mesh.bounds, 99);
                    //}

                    // create gameobject for mesh
                    // TODO offset center to tile position
                    var mf = Instantiate(meshPrefab) as MeshFilter;
                    //var mf = Instantiate(meshPrefab, t.center,Quaternion.identity) as MeshFilter;


                    mf.name = "MeshTile_" + items;// + "_x=" + t.cellX + "_y=" + t.cellY + "_z=" + t.cellZ;
                    mf.gameObject.SetActive(false);

                    // set pos
                    // FIXME without setpos works, but culling breaks
                    //mf.transform.position = new Vector3(t.cellX, t.cellY, t.cellZ); // wrong
                    //mf.transform.position = t.center/16; // wrong
                    //mf.transform.position = new Vector3(t.minX, t.minY, t.minZ); // wrong
                    //mf.transform.position = mesh.bounds.min; // wrong

                    mf.mesh = mesh;

                    // prepare material for packed
                    if (isPackedColors == true)
                    {
                        var mr = mf.GetComponent<MeshRenderer>();
                        //t.material = new Material(mr.material);
                        t.material = new Material(cloudMaterial);
                        mr.material = t.material;

                        //if (items < 10)
                        //{
                        //    PointCloudTools.DrawBounds(mr.bounds, 99);
                        //}
                    }

                    // create tile
                    var meshTile = new PointCloudMeshTile();
                    meshTile.mesh = mesh;
                    meshTile.meshFilter = mf;
                    //meshTile.mesh.bounds = new Bounds(t.center, new Vector3(t.maxX - t.minX, t.maxY - t.minY, t.maxZ - t.minZ));
                    t.meshTile = meshTile;
                }

                // set offset for packed
                if (isPackedColors == true)
                {
                    t.material.SetVector("_Offset", new Vector3(t.cellX * gridSize, t.cellY * gridSize, t.cellZ * gridSize));
                    gridSizePackMagic = gridSize * packMagic;
                    t.material.SetFloat("_GridSizeAndPackMagic", gridSizePackMagic);
                }

                if (useLossyBytePacking == true)
                {
                    t.material.SetVector("_Offset", new Vector3(t.cellX * gridSize, t.cellY * gridSize, t.cellZ * gridSize));
                    //gridSizePackMagic = gridSize * packMagic;
                    //t.material.SetFloat("_GridSizeAndPackMagic", gridSizePackMagic);
                }

                if (overridePosition == true)
                {
                    t.material.SetVector("_Offset", offsetPosition);
                }

                tiles[items] = t;
                items++;
            } // rows

            if (useMeshRendering == true)
            {
                if (showDebug) Debug.Log("largestTilePointCount: " + largestTilePointCount);
                // create max point count index array
                precalculatedIndices = new int[maxPointsPerTile];
                for (int k = 0; k < maxPointsPerTile; k++)
                {
                    precalculatedIndices[k] = k;
                }
            }

            return true;
        } // LoadRootFile


        protected IEnumerator ProfileLoaderStatus()
        {
            initialLoaderTimerEnded = false;
            while (true)
            {
                int totalQueue = 0;
                for (int i = 0; i < loaderThreadCount; i++)
                {
                    totalQueue += threadInfos[i].loadQueue.Count;
                    yield return new WaitForSeconds(0.2f);
                }
                //Debug.Log("Queue: " + totalQueue);

                if (initialLoaderTimerEnded == false && totalQueue == 0)
                {
                    initialLoaderTimerEnded = true;
                    stopwatch.Stop();
                    // Initial load done: 16471ms, 400m cloud, 1024x720, 2 threads
                    Debug.Log("Initial load done: " + stopwatch.ElapsedMilliseconds + "ms");
                }

                if (LoaderQueueStatus != null) LoaderQueueStatus.Invoke(totalQueue);
            }
        }

        //void StartWorkerThreads()
        //{
        //    if (threadRunningA == false)
        //    {
        //        threadRunningA = true;
        //        ParameterizedThreadStart start = new ParameterizedThreadStart(LoaderWorkerThreadA);
        //        importerThreadA = new Thread(start);
        //        importerThreadA.IsBackground = true;
        //        importerThreadA.Start(null);
        //    }

        //    if (threadRunningB == false)
        //    {
        //        threadRunningB = true;
        //        ParameterizedThreadStart startB = new ParameterizedThreadStart(LoaderWorkerThreadB);
        //        importerThreadB = new Thread(startB);
        //        importerThreadB.IsBackground = true;
        //        importerThreadB.Start(null);
        //    }
        //}

        protected void StartWorkerThreads()
        {
            for (int n = 0; n < loaderThreadCount; n++)
            {
                if (threadInfos[n].threadRunning == false)
                {
                    threadInfos[n].threadRunning = true;
                    ParameterizedThreadStart start = new ParameterizedThreadStart(LoaderWorkerThread);
                    threadInfos[n].importerThread = new Thread(start);
                    threadInfos[n].importerThread.IsBackground = true;
                    threadInfos[n].importerThread.Start(n);
                }
            }
        }

        // keeps loading data, when have something in load queue
        void LoaderWorkerThread(System.Object index)
        {
            int n = (int)index;
            while (abortReaderThread == false)
            {
                try
                {
                    if (threadInfos[n].loadQueue.Count > 0)
                    {
                        int loadIndex = threadInfos[n].loadQueue.Dequeue();
                        //Debug.Log("Loading queue=" + loadIndex);
                        ReadPointCloudThreaded(n, loadIndex);
                        //Thread.Sleep(200);
                    }
                    else
                    {
                        // waiting for work
                        Thread.Sleep(2); // was 16
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
            //Debug.Log("(Worker A) Thread ended.");
            threadInfos[n].threadRunning = false;
        }

        //// keeps loading data, when have something in load queue
        //void LoaderWorkerThreadA(System.Object temp)
        //{
        //    while (abortReaderThread == false)
        //    {
        //        try
        //        {
        //            if (loadQueueA.Count > 0)
        //            {
        //                int loadIndex = loadQueueA.Dequeue();
        //                //Debug.Log("Loading queue=" + loadIndex);
        //                ReadPointCloudThreadedNewA(loadIndex);
        //                //Thread.Sleep(200);
        //            }
        //            else
        //            {
        //                // waiting for work
        //                Thread.Sleep(2); // was 16
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogException(e);
        //        }

        //    }
        //    //Debug.Log("(Worker A) Thread ended.");
        //    threadRunningA = false;
        //}

        //void LoaderWorkerThreadB(System.Object temp)
        //{
        //    while (abortReaderThread == false)
        //    {
        //        try
        //        {
        //            if (loadQueueB.Count > 0)
        //            {
        //                int loadIndex = loadQueueB.Dequeue();
        //                //Debug.Log("Loading queue=" + loadIndex);
        //                ReadPointCloudThreadedNewB(loadIndex);
        //                //Thread.Sleep(200);
        //            }
        //            else
        //            {
        //                // waiting for work
        //                Thread.Sleep(2); // was 16
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogException(e);
        //        }
        //    }
        //    //Debug.Log("(Worker B) Thread ended.");
        //    threadRunningB = false;
        //}

        // v3 tiles format
        public void ReadPointCloudThreaded(System.Object nRaw, System.Object loadIndexRaw)
        {
            int n = (int)nRaw;
            int index = (int)loadIndexRaw;

            tiles[index].isLoading = true;
            int newPointCount = tiles[index].totalPoints; // FIXME why whole count? but causes flicker if read only needed amount
            int dataBytesSize = newPointCount * 12;
            if (useLossyBytePacking == true) dataBytesSize = newPointCount * 4;

            // points
#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == true)
            {
                if (tiles[index].pointsNative.IsCreated == true) tiles[index].pointsNative.Dispose();
                tiles[index].pointsNative = new NativeArray<byte>(dataBytesSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                // TODO add small amount reader
                threadInfos[n].dataPoints = File.ReadAllBytes(filenames[index]);
                //Debug.Log(index + " pointCount=" + newPointCount + " points.len = " + tiles[index].points.Length + "  datapointsA.len = " + dataPointsA.Length);
                if (abortReaderThread || tiles[index].pointsNative.IsCreated == false) return;
                PointCloudMath.MoveFromByteArray<byte>(ref threadInfos[n].dataPoints, ref tiles[index].pointsNative);
            }
            else // managed
            {
                tiles[index].points = new Vector3[newPointCount];
                GCHandle vectorPointer = GCHandle.Alloc(tiles[index].points, GCHandleType.Pinned);
                IntPtr pV = vectorPointer.AddrOfPinnedObject();
                // TODO add small amount reader
                threadInfos[n].dataPoints = File.ReadAllBytes(filenames[index]);
                Marshal.Copy(threadInfos[n].dataPoints, 0, pV, dataBytesSize);
                vectorPointer.Free();

                //float packed = tiles[index].points[0].y; 
                //float packed = 758191142; // 20.25098 - bad
                //int packed = 758191142; // 20.14902 - good
                //int packed = (int)tiles[index].points[0].y;
                //Debug.Log(packed);

                //float aUnpacked = (float)Math.Floor(packed / (256.0 * 256.0 * 256.0)) / 255.0f;
                //float bUnpacked = (float)Math.Floor(Math.IEEERemainder(packed / (256.0 * 256.0), 256.0)) / 255.0f;
                //float cIntegralUnpacked = (float)Math.Floor((packed / 256.0) % 256.0);
                //float cFractionalUnpacked = (float)(packed % 256.0) / 255.0f;
                //float cUnpacked = cIntegralUnpacked + cFractionalUnpacked;

                //byte aUnpacked = (byte)(packed >> 24);
                //byte bUnpacked = (byte)(packed >> 16);
                //byte cIntegralUnpacked = (byte)(packed >> 8);
                //byte cFractionalUnpacked = (byte)packed;
                //float cUnpacked = cIntegralUnpacked + (cFractionalUnpacked / 255.0f);

                ////// Returning the unpacked values
                //Debug.Log("r="+aUnpacked+" i="+bUnpacked+" y="+cUnpacked);



                // TEST only
                //if (overridePosition)
                //{
                //    for (int i = 0; i < tiles[index].points.Length; i++)
                //    {
                //        tiles[index].points[i] += offsetPosition;
                //    }
                //}

                if (useMeshRendering == true)
                {
                    //if (index == 0)
                    //{
                    //    Debug.Log("index 0 loaded newPointCount: " + newPointCount);
                    //}
                    // TODO add to update priorityqueue, to set points in mainthread
                    //tiles[index].meshTile.mesh.vertices = tiles[index].points;
                    //int distanceToCam = (int)Vector3.Distance(tiles[index].center, cam.transform.position);
                    //Debug.Log("index, for mesh : " + index + ", loaded points" + tiles[index].points.Length);
                    //tiles[index].isLoading = false;
                    meshUpdateQueue.Enqueue(index, index); // TODO check priority from distance
                }
            }
#else
            GCHandle vectorPointer;
            if (useLossyBytePacking == true)
            {
                tiles[index].pointsBytePacked = new int[newPointCount];
                vectorPointer = GCHandle.Alloc(tiles[index].pointsBytePacked, GCHandleType.Pinned);
            }
            else
            {
                tiles[index].points = new Vector3[newPointCount];
                vectorPointer = GCHandle.Alloc(tiles[index].points, GCHandleType.Pinned);
            }
            IntPtr pV = vectorPointer.AddrOfPinnedObject();

            // if need to load full cloud, TODO load full cloud also if near 80-90 % amount, if its faster..
            //if (1==1)//pointCount == tiles[index].totalPoints)
            //{
            threadInfos[n].dataPoints = File.ReadAllBytes(filenames[index]);
            //}
            //else // read only required amount
            //{
            //    dataPointsA = new byte[dataBytesSize];

            //    using (var stream = new FileStream(filenames[index], FileMode.Open))
            //    {
            //        var reader = new BinaryReader(stream);
            //        stream.Position = 0;
            //        var bufferedReader = new BufferedBinaryReader(stream, 4096);
            //        var numBytesRead = stream.Read(dataPointsA, 0, dataBytesSize);
            //    }
            //}

            Marshal.Copy(threadInfos[n].dataPoints, 0, pV, dataBytesSize);
            vectorPointer.Free();

            if (useMeshRendering == true)
            {
                // TODO add to update priorityqueue, to set points in mainthread
                //tiles[index].meshTile.mesh.vertices = tiles[index].points;
                meshUpdateQueue.Enqueue(index, 100); // TODO check priority from distance
            }
#endif
            tiles[index].loadedPoints = newPointCount;// tiles[index].totalPoints;

            if (forceGC == true) GC.Collect();

            // colors
            if (isPackedColors == false && useLossyBytePacking == false)
            {
#if UNITY_2019_1_OR_NEWER

                if (useNativeArrays == true)
                {
                    if (tiles[index].colorsNative.IsCreated == true) tiles[index].colorsNative.Dispose();
                    tiles[index].colorsNative = new NativeArray<byte>(dataBytesSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    string extension = (useClassificationFile ? classificationExtension : (useIntensityFile ? intExtension : rgbExtension));
                    threadInfos[n].dataPoints = File.ReadAllBytes(filenamesRGB[index] + extension);
                    if (abortReaderThread || tiles[index].colorsNative.IsCreated == false) return;
                    PointCloudMath.MoveFromByteArray<byte>(ref threadInfos[n].dataPoints, ref tiles[index].colorsNative);
                    if (forceGC == true) GC.Collect();
                }
                else // managed
                {
                    tiles[index].colors = new Vector3[newPointCount];
                    GCHandle vectorPointer = GCHandle.Alloc(tiles[index].colors, GCHandleType.Pinned);
                    IntPtr pV = vectorPointer.AddrOfPinnedObject();
                    string extension = (useClassificationFile ? classificationExtension : (useIntensityFile ? intExtension : rgbExtension));
                    threadInfos[n].dataPoints = File.ReadAllBytes(filenamesRGB[index] + extension);
                    //threadInfos[n].dataPoints = File.ReadAllBytes(filenamesRGB[index] + (useIntensityFile ? intExtension : rgbExtension));
                    Marshal.Copy(threadInfos[n].dataPoints, 0, pV, dataBytesSize);
                    vectorPointer.Free();
                }
#else
                tiles[index].colors = new Vector3[newPointCount];
                vectorPointer = GCHandle.Alloc(tiles[index].colors, GCHandleType.Pinned);
                pV = vectorPointer.AddrOfPinnedObject();
                threadInfos[n].dataPoints = File.ReadAllBytes(filenamesRGB[index]+ (useIntensityFile ? rgbExtension : intExtension));
                Marshal.Copy(threadInfos[n].dataPoints, 0, pV, dataBytesSize);
                vectorPointer.Free();
#endif
                if (forceGC == true) GC.Collect();
            }

            // refresh buffers, check if needed?
            threadInfos[n].isInitializingBuffers = true;
            threadInfos[n].tempIndex = index;
            //MainThread.Call(CallInitDX11BufferA);

            //if (useMeshRendering == false) 
            MainThread.Call(CallInitDX11Buffer, n);

            // wait for buffer init
            while (threadInfos[n].isInitializingBuffers == true && abortReaderThread == false)
            {
                Thread.Sleep(1);
            }

            //Debug.Log("tiledata is ready: "+index);
            tiles[index].isInQueue = false;
            tiles[index].isLoading = false;
            tiles[index].isReady = true;

        } // ReadPointCloudThreaded

        //        byte[] dataPointsB = new byte[1];
        //        public void ReadPointCloudThreadedNewB(System.Object rawindex)
        //        {
        //            int index = (int)rawindex;
        //            tiles[index].isLoading = true;
        //            // TODO whole cloud gets read? but then faster to increase, no need to reload again?
        //            int newPointCount = tiles[index].totalPoints;
        //            //int newPointCount = tiles[index].visiblePoints;

        //            int dataBytesSize = newPointCount * 12;
        //            if (useLossyBytePacking == true) dataBytesSize = newPointCount * 4;

        //            if (newPointCount == 0)
        //            {
        //                tiles[index].isLoading = false;
        //                return;
        //            }

        //            // read points
        //#if UNITY_2019_1_OR_NEWER
        //            if (useNativeArrays == true)
        //            {
        //                if (tiles[index].pointsNative.IsCreated == true) tiles[index].pointsNative.Dispose();
        //                tiles[index].pointsNative = new NativeArray<byte>(dataBytesSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        //                dataPointsB = File.ReadAllBytes(filenames[index]);
        //                if (abortReaderThread || tiles[index].pointsNative.IsCreated == false) return;
        //                PointCloudMath.MoveFromByteArray(ref dataPointsB, ref tiles[index].pointsNative);
        //            }
        //            else
        //            {
        //                tiles[index].points = new Vector3[newPointCount];
        //                GCHandle vectorPointer = GCHandle.Alloc(tiles[index].points, GCHandleType.Pinned);
        //                IntPtr pV = vectorPointer.AddrOfPinnedObject();
        //                dataPointsB = File.ReadAllBytes(filenames[index]);
        //                //Debug.Log(index + " pointCount=" + pointCount + " points.len = " + tiles[index].points.Length + "  datapoints.len = " + dataPoints.Length);
        //                Marshal.Copy(dataPointsB, 0, pV, dataBytesSize);
        //                vectorPointer.Free();
        //            }
        //#else
        //            // NOTE this fails, if array is too large
        //            GCHandle vectorPointer;
        //            if (useLossyBytePacking == true)
        //            {
        //                tiles[index].pointsBytePacked = new int[newPointCount];
        //                vectorPointer = GCHandle.Alloc(tiles[index].pointsBytePacked, GCHandleType.Pinned);
        //            }
        //            else
        //            {
        //                tiles[index].points = new Vector3[newPointCount];
        //                vectorPointer = GCHandle.Alloc(tiles[index].points, GCHandleType.Pinned);
        //            }
        //            IntPtr pV = vectorPointer.AddrOfPinnedObject();

        //            // if need to load full cloud, TODO load full cloud also if near 80-90 % amount, if its faster..
        //            // FIXME should read bigger amount anyways, since otherwise need to load more points soon again to increase count
        //            //if (1==1)//newPointCount == tiles[index].totalPoints)
        //            //{
        //            dataPointsB = File.ReadAllBytes(filenames[index]);
        //            //}
        //            //else // read only required amount
        //            //{
        //            //    // TODO no need to completely erase, just resize would be needed..
        //            //    dataPointsB = new byte[dataBytesSize];
        //            //    //Array.Resize(ref dataPointsB, dataBytesSize);

        //            //    using (var stream = new FileStream(filenames[index], FileMode.Open))
        //            //    {
        //            //        var reader = new BinaryReader(stream);
        //            //        stream.Position = 0;
        //            //        var bufferedReader = new BufferedBinaryReader(stream, 4096);
        //            //        // read only missing points
        //            //        var missingPoints = newPointCount - tiles[index].loadedPoints;
        //            //        //if (missingPoints < 0)
        //            //        {
        //            //            //  Debug.LogWarning("index=" + index + "  MissingBytes = " + missingPoints + " loaded=" + tiles[index].loadedPoints + " visible=" + tiles[index].visiblePoints);
        //            //            //return;
        //            //        }
        //            //        //else
        //            //        {
        //            //            // read incrementally
        //            //            //var numBytesRead = stream.Read(dataPointsB, tiles[index].loadedPoints * 12, missingPoints * 12);
        //            //            // read from start
        //            //            var numBytesRead = stream.Read(dataPointsB, 0, dataBytesSize);
        //            //        }
        //            //    }
        //            //}

        //            //Debug.Log(index + " pointCount=" + pointCount + " points.len = " + tiles[index].points.Length + "  datapoints.len = " + dataPoints.Length);
        //            //Debug.Log("filelen= " + dataPointsB.Length + " dataneeded: " + (dataBytesSize));

        //            Marshal.Copy(dataPointsB, 0, pV, dataBytesSize);
        //            vectorPointer.Free();

        //            //if (useMeshRendering == true)
        //            //{
        //            //    // TODO add to update priorityqueue, to set points in mainthread
        //            //    //tiles[index].meshTile.mesh.vertices = tiles[index].points;
        //            //    meshUpdateQueue.Enqueue(index, 100); // TODO check priority from distance
        //            //}

        //#endif
        //            // set current amount (TODO could keep loaded points in memory still!!)
        //            tiles[index].loadedPoints = newPointCount; // tiles[index].totalPoints;
        //            //tiles[index].visiblePoints = newPointCount;

        //            if (forceGC == true) GC.Collect();

        //            // colors
        //            if (isPackedColors == false && useLossyBytePacking == false)
        //            {
        //#if UNITY_2019_1_OR_NEWER
        //                if (useNativeArrays == true)
        //                {
        //                    if (tiles[index].colorsNative.IsCreated == true) tiles[index].colorsNative.Dispose();
        //                    tiles[index].colorsNative = new NativeArray<byte>(dataBytesSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        //                    dataPointsB = File.ReadAllBytes(filenamesRGB[index]);
        //                    if (abortReaderThread || tiles[index].colorsNative.IsCreated == false) return;
        //                    PointCloudMath.MoveFromByteArray(ref dataPointsB, ref tiles[index].colorsNative);
        //                }
        //                else
        //                {
        //                    tiles[index].colors = new Vector3[newPointCount];
        //                    GCHandle vectorPointer = GCHandle.Alloc(tiles[index].colors, GCHandleType.Pinned);
        //                    IntPtr pV = vectorPointer.AddrOfPinnedObject();
        //                    dataPointsB = File.ReadAllBytes(filenamesRGB[index]);
        //                    Marshal.Copy(dataPointsB, 0, pV, dataBytesSize);
        //                    vectorPointer.Free();
        //                }
        //#else
        //                tiles[index].colors = new Vector3[newPointCount];
        //                vectorPointer = GCHandle.Alloc(tiles[index].colors, GCHandleType.Pinned);
        //                pV = vectorPointer.AddrOfPinnedObject();
        //                dataPointsB = File.ReadAllBytes(filenamesRGB[index]);
        //                Marshal.Copy(dataPointsB, 0, pV, dataBytesSize);
        //                vectorPointer.Free();
        //#endif
        //                if (forceGC == true) GC.Collect();
        //            }

        //            // refresh buffers, check if needed?
        //            isInitializingBuffersB = true;
        //            tempIndexB = index;
        //            MainThread.Call(CallInitDX11BufferB);

        //            while (isInitializingBuffersB == true && abortReaderThread == false)
        //            {
        //                Thread.Sleep(1);
        //            }

        //            //Debug.Log("Done loading index=" + index + " points=" + pointCount);
        //            tiles[index].isInQueue = false;
        //            tiles[index].isLoading = false;
        //            tiles[index].isReady = true;

        //        } // ReadPointCloudThreadedB

        void CallInitDX11Buffer(object index)
        {
            int n = (int)index;
            StartCoroutine(InitDX11Buffer(n));
        }

        //public void CallInitDX11BufferA()
        //{
        //    StartCoroutine(InitDX11BufferA());
        //}

        //public void CallInitDX11BufferB()
        //{
        //    StartCoroutine(InitDX11BufferB());
        //}

        IEnumerator InitDX11Buffer(object index)
        {
            int n = (int)index;

            // TESTING
            if (useMeshRendering == true)
            {
                threadInfos[n].isInitializingBuffers = false;
                yield break;
            }


            int nodeIndex = threadInfos[n].tempIndex;

            int pointCount = tiles[nodeIndex].loadedPoints;
            if (pointCount == 0)
            {
                threadInfos[n].isInitializingBuffers = false;
                yield break;
            }

            try
            {
                // init buffers on demand, otherwise grabs full memory
                if (tiles[nodeIndex].bufferPoints != null) tiles[nodeIndex].bufferPoints.Release();
                if (useLossyBytePacking == true)
                {
                    tiles[nodeIndex].bufferPoints = new ComputeBuffer(pointCount, 4);
                }
                else
                {
                    tiles[nodeIndex].bufferPoints = new ComputeBuffer(pointCount, 12);
                }

                if (isPackedColors == false && useLossyBytePacking == false)
                {
                    if (tiles[nodeIndex].bufferColors != null) tiles[nodeIndex].bufferColors.Release();
                    tiles[nodeIndex].bufferColors = new ComputeBuffer(pointCount, 12);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            //int stepSize = pointCount / (gpuUploadSteps == 0 ? 1 : gpuUploadSteps);
            int stepSize = Math.Max(1, pointCount / (gpuUploadSteps == 0 ? 1 : gpuUploadSteps));
            //if (stepSize == 0) Debug.LogError(nodeIndex + "  pointCount=" + pointCount + " stepSize=" + stepSize);
            int stepSizeBytes = stepSize;
            int stepCount = pointCount / stepSize;
            int startIndex = 0;



#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == true)
            {
                stepSizeBytes *= 12;
            }
#endif
            for (int i = 0; i < stepCount; i++)
            {
                try
                {
#if UNITY_2019_1_OR_NEWER

                    if (useNativeArrays == true)
                    {
                        if (tiles[nodeIndex].pointsNative.IsCreated == false) continue;
                        tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].pointsNative, startIndex, startIndex, stepSizeBytes);
                        tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
                    }
                    else
                    {
                        tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].points, startIndex, startIndex, stepSizeBytes);
                        tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
                    }
#else
                    if (useLossyBytePacking == true)
                    {
                        tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].pointsBytePacked, startIndex, startIndex, stepSizeBytes);
                    }
                    else
                    {
                        tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].points, startIndex, startIndex, stepSizeBytes);
                    }
                    tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
#endif

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (gpuUploadSteps > 0) yield return 0;

                if (isPackedColors == false && useLossyBytePacking == false)
                {
                    try
                    {
#if UNITY_2019_1_OR_NEWER
                        if (useNativeArrays == true)
                        {
                            if (tiles[nodeIndex].colorsNative.IsCreated == false) continue;
                            tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colorsNative, startIndex, startIndex, stepSizeBytes);
                            tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);

                        }
                        else
                        {
                            tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colors, startIndex, startIndex, stepSizeBytes);
                            tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);
                        }
#else
                        tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colors, startIndex, startIndex, stepSizeBytes);
                        tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);
#endif

                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (gpuUploadSteps > 0) yield return 0;
                }
                startIndex += stepSizeBytes;
            }
            //Debug.Log(nodeIndex+" total=" + total + " / " + pointCount * 12 + " dif=" + (pointCount * 12 - total));
            threadInfos[n].isInitializingBuffers = false;

        }

        public void ForceUpdateTilePositions(int index)
        {
#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == true)
            {
                tiles[index].bufferPoints.SetData(tiles[index].pointsNative, 0, 0, tiles[index].loadedPoints);
            }
            else
            {
                tiles[index].bufferPoints.SetData(tiles[index].points, 0, 0, tiles[index].loadedPoints);
            }
#else
            tiles[index].bufferPoints.SetData(tiles[index].points, 0, 0, tiles[index].loadedPoints);
#endif

            tiles[index].material.SetBuffer(bufID, tiles[index].bufferPoints);
        }

        public void ForceUpdateTileColors(int index)
        {
#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == true)
            {
                tiles[index].bufferColors.SetData(tiles[index].colorsNative, 0, 0, tiles[index].loadedPoints);
            }
            else
            {
                tiles[index].bufferColors.SetData(tiles[index].colors, 0, 0, tiles[index].loadedPoints);
            }
#else
            tiles[index].bufferColors.SetData(tiles[index].colors, 0, 0, tiles[index].loadedPoints);
#endif

            tiles[index].material.SetBuffer(bufColorID, tiles[index].bufferColors);
        }

        //        IEnumerator InitDX11BufferB()
        //        {
        //            int nodeIndex = tempIndexB;

        //            int pointCount = tiles[nodeIndex].loadedPoints;
        //            if (pointCount == 0)
        //            {
        //                isInitializingBuffersB = false;
        //                yield break;
        //            }

        //            // init buffers on demand, otherwise grabs full memory
        //            if (tiles[nodeIndex].bufferPoints != null) tiles[nodeIndex].bufferPoints.Release();
        //            if (useLossyBytePacking == true)
        //            {
        //                tiles[nodeIndex].bufferPoints = new ComputeBuffer(pointCount, 4);
        //            }
        //            else
        //            {
        //                tiles[nodeIndex].bufferPoints = new ComputeBuffer(pointCount, 12);
        //            }

        //            if (isPackedColors == false && useLossyBytePacking == false)
        //            {
        //                if (tiles[nodeIndex].bufferColors != null) tiles[nodeIndex].bufferColors.Release();
        //                tiles[nodeIndex].bufferColors = new ComputeBuffer(pointCount, 12);
        //            }

        //            int stepSize = pointCount / (gpuUploadSteps == 0 ? 1 : gpuUploadSteps);
        //            //if (stepSize == 0) Debug.LogError(nodeIndex + "  pointCount=" + pointCount + " stepSize=" + stepSize);
        //            int stepSizeBytes = stepSize;
        //            int stepCount = pointCount / stepSize;
        //            int startIndex = 0;
        //#if UNITY_2019_1_OR_NEWER
        //            if (useNativeArrays == true)
        //            {
        //                stepSizeBytes *= 12;
        //            }
        //#endif
        //            for (int i = 0; i < stepCount; i++)
        //            {

        //#if UNITY_2019_1_OR_NEWER
        //                if (useNativeArrays == true)
        //                {
        //                    if (tiles[nodeIndex].pointsNative.IsCreated == false) continue;
        //                    tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].pointsNative, startIndex, startIndex, stepSizeBytes);
        //                    tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
        //                }
        //                else
        //                {
        //                    tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].points, startIndex, startIndex, stepSizeBytes);
        //                    tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
        //                }
        //#else
        //                if (useLossyBytePacking == true)
        //                {
        //                    tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].pointsBytePacked, startIndex, startIndex, stepSizeBytes);
        //                }
        //                else
        //                {
        //                    tiles[nodeIndex].bufferPoints.SetData(tiles[nodeIndex].points, startIndex, startIndex, stepSizeBytes);
        //                }
        //                tiles[nodeIndex].material.SetBuffer(bufID, tiles[nodeIndex].bufferPoints);
        //#endif
        //                if (gpuUploadSteps > 0) yield return 0;

        //                if (isPackedColors == false && useLossyBytePacking == false)
        //                {

        //#if UNITY_2019_1_OR_NEWER
        //                    if (useNativeArrays == true)
        //                    {
        //                        if (tiles[nodeIndex].colorsNative.IsCreated == false) continue;
        //                        tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colorsNative, startIndex, startIndex, stepSizeBytes);
        //                        tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);

        //                    }
        //                    else
        //                    {
        //                        tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colors, startIndex, startIndex, stepSizeBytes);
        //                        tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);
        //                    }
        //#else
        //                    tiles[nodeIndex].bufferColors.SetData(tiles[nodeIndex].colors, startIndex, startIndex, stepSizeBytes);
        //                    tiles[nodeIndex].material.SetBuffer(bufColorID, tiles[nodeIndex].bufferColors);
        //#endif
        //                    if (gpuUploadSteps > 0) yield return 0;
        //                }
        //                startIndex += stepSizeBytes;
        //            }
        //            //Debug.Log(nodeIndex+" total=" + total + " / " + pointCount * 12 + " dif=" + (pointCount * 12 - total));
        //            isInitializingBuffersB = false;
        //        }

        public void ReleaseDX11Buffers()
        {
            if (tiles == null) return;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i].bufferPoints != null) tiles[i].bufferPoints.Release();
                tiles[i].bufferPoints = null;
                if (tiles[i].bufferColors != null) tiles[i].bufferColors.Release();
                tiles[i].bufferColors = null;
#if UNITY_2019_1_OR_NEWER
                if (useNativeArrays == true)
                {
                    if (tiles[i].pointsNative.IsCreated == true) tiles[i].pointsNative.Dispose();
                    if (tiles[i].intensityNative.IsCreated == true) tiles[i].intensityNative.Dispose();
                    if (isPackedColors == false && tiles[i].colorsNative.IsCreated == true) tiles[i].colorsNative.Dispose();
                }
#endif
            }
        }

        void OnDestroy()
        {
            abortReaderThread = true;

            if (pointPickingThread != null && pointPickingThread.IsAlive == true) pointPickingThread.Abort();
            if (pointPickingThread2 != null && pointPickingThread2.IsAlive == true) pointPickingThread2.Abort();

            ReleaseDX11Buffers();

            if (rootLoaded == true)
            {
                cullGroup.onStateChanged -= OnCullingStateChange;
                cullGroup.Dispose();
                cullGroup = null;
            }
        }


        // drawing mainloop, for drawing the points
        //void OnPostRender() // < works also, BUT must have this script attached to Camera
        public void OnRenderObject()
        {
            if (rootLoaded == false || renderPoints == false || useMeshRendering == true || useURPCustomPass == true || useHDRPCustomPass == true || (renderOnlyMainCam == true && Camera.current?.CompareTag("MainCamera") == false)) return;


            for (int i = 0, len = tilesCount; i < len; i++)
            {
                if (tiles[i].isReady == false || tiles[i].isLoading == true || tiles[i].visiblePoints == 0) continue;

                tiles[i].material.SetPass(0);

#if UNITY_2019_1_OR_NEWER
                // TODO test drawmeshnow! but still need to create mesh then?
                Graphics.DrawProceduralNow(MeshTopology.Points, tiles[i].visiblePoints);
#else
                Graphics.DrawProcedural(MeshTopology.Points, tiles[i].visiblePoints);
#endif
            }
        }

        // called after all tiles have been loaded (not working yet, since depends where user looks?)
        public void OnLoadingCompleteCallBack(System.Object a)
        {
            if (OnLoadingComplete != null) OnLoadingComplete((string)a);
            Debug.Log("(Tiles Viewer) Finished loading all tiles");

            // NOTE hackfix for cullingdata not refreshing..
            //StartCoroutine(RefreshCameraCulling());
            // for now, can stop worker threads
            //abortReaderThread = true;
            //PointCloudTools.DrawBounds(cloudBounds, 100);

            initIsReady = true;
        }

        // Temporary fix for culling group not refreshing (unless camera moved)
        IEnumerator RefreshCameraCulling()
        {
            // OR could adjust camera view distance
            cullGroup.SetDistanceReferencePoint(new Vector3(0, 999999, 0));
            yield return 0;
            cullGroup.SetDistanceReferencePoint(cam.transform);
        }

        public void FixMainThreadHelper()
        {
            if (GameObject.Find("#MainThreadHelper") == null || MainThread.instanceCount == 0)
            {
                var go = new GameObject();
                go.name = "#MainThreadHelper";
                go.AddComponent<MainThread>();
            }
        }

        public IEnumerator InitCullingSystem()
        {
            // validate values
            if (startDist < 0)
            {
                Debug.LogError("startDist must be > 0. You have set it to " + startDist);
                startDist = 0;
            }
            if (endDist < startDist)
            {
                Debug.LogError("endDist must be > startDist. You have set it to " + endDist);
                endDist = startDist + 1;
            }

            if (lodSteps < 2 || lodSteps > 180)
            {
                Debug.LogError("lodSteps must be in between 2-180. You have set it to " + lodSteps);
                lodSteps = 100;
            }

            // calculate distances
            cullingBandDistances = new float[lodSteps + 1];
            for (int i = 0; i < lodSteps; i++)
            {
                cullingBandDistances[i] = Mathf.LerpUnclamped(startDist, endDist, ((float)i) / ((float)(lodSteps - 1)));
                //Debug.Log(i + "=" + distances[i]);
            }
            // add last point, to properly hide at maxdist
            cullingBandDistances[lodSteps] = endDist + 1;

            // create culling group
            cullGroup = new CullingGroup();
            cullGroup.targetCamera = cam;

            // measure distance to camera transform
            cullGroup.SetDistanceReferencePoint(cam.transform);

            // search distance "bands" starts from 0, so index=0 is from 0 to searchDistance
            cullGroup.SetBoundingDistances(cullingBandDistances);

            // get cloud pieces
            int objectCount = tiles.Length;
            boundingSpheres = new BoundingSphere[objectCount];
            for (int i = 0; i < objectCount; i++)
            {
                var t = tiles[i];
                boundingSpheres[i].position = t.center;
                boundingSpheres[i].radius = gridSize;
                //                Debug.DrawLine(t.center, t.center + Vector3.up * gridSize / 2, Color.red, 32);
            }

            // set bounds that we track
            InitCullingValues();

            // subscribe to event
            cullGroup.onStateChanged += OnCullingStateChange;
            Debug.Log("(Tiles Viewer) Done init culling.");


            // fix for not calling statechange on creation
            /*
            cam.enabled = false;
            yield return new WaitForSeconds(1f);
            cam.enabled = true;
            */

            /*            
                        cam.transform.Translate(0, 99999, 0);
                        yield return new WaitForSeconds(0.2f);
                        cam.transform.Translate(0, -99999, 0);
              */

            StartCoroutine(RefreshCameraCulling());

            //initDone = true;
            yield return 0;

            initIsReady = true;
        }

        void InitCullingValues()
        {
            cullGroup.SetBoundingSpheres(boundingSpheres);
            cullGroup.SetBoundingSphereCount(tiles.Length);
        }

        // NOTE not so nice, and tiles outside view are not visible anyways
        //bool useMinimumPointCount = true;
        //float minimumPointCountPercentage = 0.5f;
        // TODO this could be done in shader? (sizebydistance already does?) but what about relative to point count or tilesize?
        //public AnimationCurve pointSizeByDistance;
        //bool usePointSizeByDistance = false;

        // object state has changed in culling group
        void OnCullingStateChange(CullingGroupEvent e)
        {
            //Debug.Log(e.index + " " + e.isVisible);
            // FIXME this breaks initialization?
            //if (tiles[e.index].isReady == false) return;

            //if (e.hasBecomeInvisible)
            if (e.isVisible == false) // not visible tile TODO check if this breaks dispose?
            {
                //Debug.Log("hidden "+e.index);
                tiles[e.index].visiblePoints = 0;

                // FIXME is this needed? since 0 verts are not drawn anyways?
                if (useMeshRendering == true)
                {
                    tiles[e.index].meshTile.meshFilter.gameObject.SetActive(false);
                }

                // if too far, hide, and dispose if supported
                if (e.currentDistance == lodSteps + 1)
                {
                    //PointCloudTools.DrawMinMaxBounds(tiles[e.index].minX, tiles[e.index].minY, tiles[e.index].minZ, tiles[e.index].maxX, tiles[e.index].maxY, tiles[e.index].maxZ, 10);

#if UNITY_2019_1_OR_NEWER
                    if (tiles[e.index].isInQueue || tiles[e.index].isLoading) return;
                    tiles[e.index].loadedPoints = 0;
                    tiles[e.index].isReady = false;
                    if (useNativeArrays == true)
                    {
                        if (releaseTileMemory == true && tiles[e.index].pointsNative.IsCreated)
                        {
                            tiles[e.index].pointsNative.Dispose();
                            tiles[e.index].bufferPoints.Dispose();
                            if (isPackedColors == false && tiles[e.index].colorsNative.IsCreated)
                            {
                                tiles[e.index].colorsNative.Dispose();
                                tiles[e.index].bufferColors.Dispose();
                            }
                        }
                    }
#endif
                }
                return;
            } // this tile is not visible (anymore)

            if (useMeshRendering == true)
            {
                tiles[e.index].meshTile.meshFilter.gameObject.SetActive(true);
            }

            int distanceBand = e.currentDistance;

            float distanceMultiplier = (1f - (float)distanceBand / (float)(cullingBandDistances.Length - 1)) * tileResolution;
            //int newpointcount = (int)((float)tiles[e.index].loadedPoints * (useStrongFalloff ? EaseInQuint(0f, 1f, multiplier) : multiplier));
            int newpointcount = 0;
            if (clampLodTilePointCounts == true)
            {
                // if further away
                newpointcount = distanceBand > 0 ?
                    Mathf.Min(tiles[e.index].totalPoints, (int)((float)(maxLodTilePointCount) * (useStrongFalloff ? EaseInQuint(0f, 1f, distanceMultiplier) : distanceMultiplier))) :
                    Mathf.Min(tiles[e.index].totalPoints, maxLodNearTilePointCount); // if near
            }
            else
            {
                newpointcount = (int)((float)tiles[e.index].totalPoints * (useStrongFalloff ? EaseInQuint(0f, 1f, distanceMultiplier) : distanceMultiplier));
            }


            // full tile
            //if (distanceBand == 0)
            //{
            //    Debug.Log("FullTile, newcount=" + newpointcount + " / " + tiles[e.index].totalPoints);
            //}

            // no points will be visible, TODO add minimum pointcount variable
            if (newpointcount < minimumTilePointCount) newpointcount = 0;
            //if (newpointcount == 0 && tiles[e.index].visiblePoints == 0) return;

            // update multiplier size
            if (useSizeMultiplier == true)
            {
                tiles[e.index].material.SetFloat("_SizeMultiplier", pointSizeMultiplier);
            }

            // near smaller
            //tiles[e.index].material.SetFloat("_Size", 1-pointSizeByDistance.Evaluate(multiplier));

            // far smaller
            //tiles[e.index].material.SetFloat("_Size", pointSizeByDistance.Evaluate(multiplier));

            // based on amount visible/loaded
            //if (usePointSizeByDistance == true)
            //{
            //    //float amount = tiles[e.index].totalPoints / (float)newpointcount;
            //    //Debug.Log("amount=" + (multiplier));
            //    //tiles[e.index].material.SetFloat("_Size", 0.05f * (pointSizeByDistance.Evaluate((useStrongFalloff ? EaseInQuint(10f, 10f, 1-multiplier) : pointSizeByDistance.Evaluate(multiplier)))));
            //    tiles[e.index].material.SetFloat("_Size", 0.015f * (useStrongFalloff ? EaseInQuint(1f, 200f, 1 - multiplier) : pointSizeByDistance.Evaluate(multiplier)));
            //}

            //// TODO add minimum point count and pointsize by amount here?
            //if (useMinimumPointCount == true)
            //{
            //    int minCount = (int)(tiles[e.index].totalPoints * minimumPointCountPercentage);
            //    if (newpointcount < minCount)
            //    {
            //        newpointcount = minCount;
            //    }
            //}

            // TODO round or threshold newpointcount to nearest x amount (so that we dont queue tile just because 1 is missing)
            //float missingPointsPercentage = (float)(tiles[e.index].loadedPoints + 1) / (float)(newpointcount + 1);

            //if (missingPointsPercentage < 0.1f || (distanceBand == 0 && newpointcount > tiles[e.index].loadedPoints))
            // here we set the new pointcount, and add to queue if needed
            if (newpointcount > tiles[e.index].loadedPoints)
            {
                if (useOnTileAppearEvent == true)
                {
                    var res = MyInterfaceInstance.ValidateTile(tiles[e.index].averageGPSTime, tiles[e.index].overlapRatio);
                    if (res == false)
                    {
                        //Debug.Log("Tile not valid, skipping index=" + e.index);
                        if (showDebug)
                        {
                            var tileBounds = new Bounds(tiles[e.index].center, new Vector3(tiles[e.index].maxX - tiles[e.index].minX, tiles[e.index].maxY - tiles[e.index].minY, tiles[e.index].maxZ - tiles[e.index].minZ));
                            var color = new Color(1, 0, 1, 1f);
                            PointCloudTools.DrawBounds(tileBounds, color, 64);
                        }
                        return;
                    }
                }


                // NOTE adjusting this here might cause race conditions in loader (since that value is used there at start)
                tiles[e.index].visiblePoints = newpointcount;
                //Debug.Log(missingPointsPercentage + " Not enought points loaded, index=" + e.index + " loaded=" + tiles[e.index].loadedPoints + " needed=" + newpointcount + " isinQUEUE=" + tiles[e.index].isInQueue);

                if (tiles[e.index].isInQueue == false)
                {
                    tiles[e.index].isInQueue = true;
                    threadInfos[e.index % loaderThreadCount].loadQueue.Enqueue(e.index, distanceBand);
                    var i = e.index;
                    //var testBounds = new Bounds(tiles[i].center, new Vector3(tiles[i].maxX - tiles[i].minX, tiles[i].maxY - tiles[i].minY, tiles[i].maxZ - tiles[i].minZ));
                    //PointCloudTools.DrawBounds(testBounds, 1);
                }
            } // if not enough points
            else // we already have enough, or too many points loaded, or close enough amount, dont add to queue
            {
                //if (e.index == 0)
                //{
                //    Debug.Log("enough points cullstate: newpointcount=" + newpointcount + " tiles[e.index].loadedPoints:" + tiles[e.index].loadedPoints + " tiles[e.index].visiblePoints:" + tiles[e.index].visiblePoints);
                //}

                //if (tiles[e.index].isInQueue == false)
                {
                    //if (e.index == 283)
                    //{
                    //    //Debug.Log("SetVisibleCount= " + newpointcount + " index=" + e.index);
                    //    var i = e.index;
                    //    var testBounds = new Bounds(tiles[i].center, new Vector3(tiles[i].maxX - tiles[i].minX, tiles[i].maxY - tiles[i].minY, tiles[i].maxZ - tiles[i].minZ));
                    //    //PointCloudTools.DrawBounds(testBounds, 3);
                    //}
                    tiles[e.index].visiblePoints = newpointcount;

                    // TODO testing if needed, seems so, but should not queue, if already in queue
                    // FIXME no need if pooling?
                    //if (useMeshRendering == true) meshUpdateQueue.Enqueue(e.index, e.index); // TODO check priority from distance
                }

                // this tile is visible
                //if (useMeshRendering == true)
                //{
                //    Debug.Log("e.index, isloading " + e.index + ", " + tiles[e.index].isLoading);
                //    // TEST if loaded
                //    if (tiles[e.index].isLoading == false)
                //    {

                //        // TODO use existing tile in this cell, if already available?
                //        GameObject meshObject = poolManager.pool.Get();
                //        int eIndex = e.index;

                //        // FIXME this is called before tile is ready..

                //        // test override
                //        tiles[eIndex].visiblePoints = tiles[eIndex].loadedPoints;

                //        Debug.Log("e index: " + eIndex + " , tile points " + tiles[eIndex].points?.Length + ", visible: " + tiles[eIndex].visiblePoints);


                //        tiles[eIndex].meshTile = new PointCloudMeshTile();
                //        tiles[eIndex].meshTile.gameObject = meshObject;
                //        //tiles[eIndex].meshTile.meshFilter = meshObject.GetComponent<MeshFilter>();
                //        //tiles[eIndex].meshTile.mesh = tiles[eIndex].meshTile.meshFilter.mesh;

                //        // FIXME this points is null
                //        //tiles[eIndex].meshTile.mesh.vertices = tiles[eIndex].points;
                //        int[] tempIndices = new int[tiles[eIndex].visiblePoints];
                //        Buffer.BlockCopy(precalculatedIndices, 0, tempIndices, 0, tempIndices.Length);
                //        //tiles[eIndex].meshTile.mesh.SetIndices(tempIndices, MeshTopology.Points, 0, true);
                //        //tiles[eIndex].meshTile.mesh.bounds = new Bounds(tiles[eIndex].center, new Vector3(tiles[eIndex].maxX - tiles[eIndex].minX, tiles[eIndex].maxY - tiles[eIndex].minY, tiles[eIndex].maxZ - tiles[eIndex].minZ));

                //        // TODO cache these mats
                //        if (isPackedColors == true)
                //        {
                //            tiles[eIndex].material.SetVector("_Offset", new Vector3(tiles[eIndex].cellX * gridSize, tiles[eIndex].cellY * gridSize, tiles[eIndex].cellZ * gridSize));
                //            gridSizePackMagic = gridSize * packMagic;
                //            tiles[eIndex].material.SetFloat("_GridSizeAndPackMagic", gridSizePackMagic);
                //        }

                //        // TODO should assign to pool item only, not in tile stuff?
                //        meshObject.GetComponent<MeshRenderer>().material = tiles[eIndex].material;
                //        //meshObject.GetComponent<MeshFilter>().mesh = tiles[eIndex].meshTile.mesh;
                //        meshObject.GetComponent<MeshFilter>().mesh.vertices = tiles[eIndex].points;
                //        meshObject.GetComponent<MeshFilter>().mesh.SetIndices(tempIndices, MeshTopology.Points, 0, true);
                //        meshObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(tiles[eIndex].center, new Vector3(tiles[eIndex].maxX - tiles[eIndex].minX, tiles[eIndex].maxY - tiles[eIndex].minY, tiles[eIndex].maxZ - tiles[eIndex].minZ));

                //        //meshObject.GetComponent<MeshFilter>().mesh.vertices = tiles[meshIndex].points;
                //        //meshObject.get

                //        PointCloudTools.DrawBounds(new Bounds(tiles[eIndex].center, new Vector3(tiles[eIndex].maxX - tiles[eIndex].minX, tiles[eIndex].maxY - tiles[eIndex].minY, tiles[eIndex].maxZ - tiles[eIndex].minZ)), Color.green, 5);

                //        //Debug.Log("tiles[meshIndex].points " + tiles[meshIndex].points.Length);
                //        //Debug.Log("tiles[meshIndex].meshTile.mesh.vertices "+ tiles[meshIndex].meshTile.mesh.vertices.Length);

                //        // TODO compare gameobject vs meshrenderer toggle
                //        //tiles[e.index].meshTile.meshFilter.gameObject
                //    }
                //    //tiles[e.index].meshTile.meshFilter.gameObject.SetActive(true);
                //}


            } // if enough points



        } // OnCullingStateChange

        // easing methods by https://gist.github.com/cjddmut/d789b9eb78216998e95c Created by C.J. Kimberlin The MIT License (MIT)
        float EaseInQuint(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        Thread pointPickingThread;
        Thread pointPickingThread2;

        // NOTE doesnt support more than 2 threads now
        const int maxThreads = 2;

        struct PointPickingThreadData
        {
            public int index;
            public Ray ray;
        }

        // point picking initial "brute" version
        public void RunPointPickingThread(Ray ray)
        {
            // start thread 1
            ParameterizedThreadStart start = new ParameterizedThreadStart(FindClosestPoint);
            pointPickingThread = new Thread(start);
            pointPickingThread.IsBackground = true;
            pointPickingThread.Start(new PointPickingThreadData() { index = 0, ray = ray });

            // start thread 2
            ParameterizedThreadStart start2 = new ParameterizedThreadStart(FindClosestPoint);
            pointPickingThread2 = new Thread(start2);
            pointPickingThread2.IsBackground = true;
            pointPickingThread2.Start(new PointPickingThreadData() { index = 1, ray = ray });
        }

        public void FindClosestPoint(object threadData)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foundPoints[0] = false;
            foundPoints[1] = false;
            nearestPoints[0] = Vector3.zero;
            nearestPoints[1] = Vector3.zero;
            nearestDistancesToCamera[0] = Mathf.Infinity;
            nearestDistancesToCamera[1] = Mathf.Infinity;

            Ray ray = ((PointPickingThreadData)threadData).ray;

            Vector3 rayDirection = ray.direction;
            Vector3 rayOrigin = ray.origin;
            Vector3 rayInverted = new Vector3(1f / ray.direction.x, 1f / ray.direction.y, 1f / ray.direction.z);
            Vector3 rayEnd = rayOrigin + rayDirection * maxPickDistance;

            Vector3 point;
            Vector3 color = (default);

            // check all visible tiles

            int threadIndex = ((PointPickingThreadData)threadData).index;

            //precision = Mathf.Clamp(precision,1,)

            //Debug.Log("Thread " + threadIndex + " start=" + startNode + " end=" + endNode);

            for (int i = 0, len = tiles.Length; i < len; i++)
            {
                // if not loaded
                if (tiles[i].isReady == false) continue;

                // TODO could do this first, so both threads no need to do it
                // if ray intersects tile bounds, FIXME doesnt work if origin is inside the bounds?
                //if (tiles[i].b.IntersectRay(ray))
                //if (PointCloudMath.RayBoxIntersect2(rayOrigin, rayInverted, new Vector3(tiles[i].minX, tiles[i].minY, tiles[i].minZ), new Vector3(tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ)) > 0 || NodeContainsPoint(tiles[i], ray.origin) == true)
                if (PointCloudMath.RayBoxIntersect2(rayOrigin, rayInverted, tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ) > 0 || NodeContainsPoint(tiles[i], ray.origin) == true)
                {
                    // depending on thread id, start from different point
                    int pointsPerThread = Mathf.CeilToInt(tiles[i].visiblePoints / (float)maxThreads);
                    int startIndex = threadIndex * pointsPerThread;
                    int endIndex = startIndex + pointsPerThread;

                    if (endIndex > tiles[i].visiblePoints)
                    {
                        endIndex = tiles[i].visiblePoints;
                    }

                    // then check all points from that node
                    //for (int k = 0, visiblePointCount = tiles[i].visiblePoints; k < visiblePointCount; k++)
                    for (int k = startIndex; k < endIndex; k += pointInsideBoxPrecision)
                    {

#if UNITY_2019_1_OR_NEWER
                        if (useNativeArrays == true)
                        {
                            // TODO no need all these if using packing below
                            point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[k * 3 * 4], tiles[i].pointsNative[k * 3 * 4 + 1], tiles[i].pointsNative[k * 3 * 4 + 2], tiles[i].pointsNative[k * 3 * 4 + 3]);
                            point.y = PointCloudMath.BytesToFloat(tiles[i].pointsNative[k * 3 * 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 1 + 4], tiles[i].pointsNative[k * 3 * 4 + 2 + 4], tiles[i].pointsNative[k * 3 * 4 + 3 + 4]);
                            point.z = PointCloudMath.BytesToFloat(tiles[i].pointsNative[k * 3 * 4 + 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 1 + 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 2 + 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 3 + 4 + 4]);
                        }
                        else
#endif
                        {
                            point = tiles[i].points[k];
                        }

                        // regular packing xyz+rgb
                        if (isPackedColors == true)
                        {
                            if (isClassificationPacked == true)
                            {
                                // x
                                int intPointX = PointCloudMath.BytesToInt(tiles[i].pointsNative[k * 3 * 4], tiles[i].pointsNative[k * 3 * 4 + 1], tiles[i].pointsNative[k * 3 * 4 + 2], tiles[i].pointsNative[k * 3 * 4 + 3]);
                                Vector3 resRCX = PointCloudMath.SuperUnpacker(intPointX); // red, classification, x
                                point.x = resRCX.z + tiles[i].cellX * gridSize;
                                color.x = resRCX.x;

                                // y
                                int intPointY = PointCloudMath.BytesToInt(tiles[i].pointsNative[k * 3 * 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 1 + 4], tiles[i].pointsNative[k * 3 * 4 + 2 + 4], tiles[i].pointsNative[k * 3 * 4 + 3 + 4]);
                                Vector3 resRCY = PointCloudMath.SuperUnpacker(intPointY); // green, classification, y
                                point.y = resRCY.z + tiles[i].cellY * gridSize;
                                color.y = resRCY.x;

                                // z
                                Vector2 zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);
                                color.z = zb.x;
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                            }
                            else if (isIntPacked == true)
                            {
                                // x 
                                Vector2 xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                point.x = xr.y + tiles[i].cellX * gridSize;
                                color.x = xr.x;

                                // y
                                int intPointY = PointCloudMath.BytesToInt(tiles[i].pointsNative[k * 3 * 4 + 4], tiles[i].pointsNative[k * 3 * 4 + 1 + 4], tiles[i].pointsNative[k * 3 * 4 + 2 + 4], tiles[i].pointsNative[k * 3 * 4 + 3 + 4]);
                                Vector3 resRCY = PointCloudMath.SuperUnpacker(intPointY); // green, classification, y
                                point.y = resRCY.z + tiles[i].cellY * gridSize;
                                color.y = resRCY.x;

                                // z
                                Vector2 zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);
                                color.z = zb.x;
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                            }
                            else // regular packing xyz+rgb
                            {
                                // need to unpack to get proper xyz
                                Vector2 xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                Vector2 yg = PointCloudMath.SuperUnpacker(point.y, gridSizePackMagic);
                                Vector2 zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);

                                // get rgb for selected point
                                color.x = xr.x;
                                color.y = yg.x;
                                color.z = zb.x;

                                point.x = xr.y + tiles[i].cellX * gridSize;
                                point.y = yg.y + tiles[i].cellY * gridSize;
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                            }
                        }


                        // check ray hit
                        float dotAngle = Vector3.Dot(rayDirection, (point - rayOrigin).normalized);

                        // 1 would be exact hit?
                        if (dotAngle > 0.99f)
                        {
                            ////MainThread.Call(PointCloudMath.DebugHighLightPointGreen, point);
                            //// try to take closest ones first
                            //float camDist = 999999f * dotAngle - PointCloudMath.Distance(rayOrigin, point);

                            //if (camDist > nearestDistancesToCamera[threadIndex])
                            //{
                            //    nearestDistancesToCamera[threadIndex] = camDist;
                            //    foundPoints[threadIndex] = true;
                            //    nearestPoints[threadIndex] = point;
                            //    //MainThread.Call(PointCloudMath.DebugHighLightPointYellow, point);
                            //}
                            float camDist = PointCloudMath.Distance(rayOrigin, point);
                            var pointRayDist = PointCloudMath.SqDistPointSegment(rayOrigin, rayEnd, point);
                            // magic formula to prefer nearby points
                            //var normCamDist = (camDist / maxPickDistance) * pointRayDist; // ok for nearby points, but too often picks nearby
                            var normCamDist = (camDist / maxPickDistance) * pointRayDist * pointRayDist; // best so far, but still too eager to pick nearby points
                                                                                                         //var normCamDist = maxPickDistance-(camDist / (pointRayDist* pointRayDist)); // better for far away picking

                            if (normCamDist < nearestDistancesToCamera[threadIndex])
                            {
                                // skip point if too far
                                if (pointRayDist > maxPickDistance) continue;
                                nearestDistancesToCamera[threadIndex] = normCamDist;
                                foundPoints[threadIndex] = true;
                                nearestPoints[threadIndex] = point;

                                //byte byteValue = (byte)Math.Round(color.x * 255f);

                                // packed color
                                //byte byteValue = (byte)(color.x * 255f);
                                //Debug.LogWarning("classification color: " + byteValue);

                                // get rgb+int packed color



                                //nearestIndex = pointIndex;
                                //viewerIndex = clouds[cloudIndex].viewerIndex;

                                // debug color show checked points
                                //MainThread.Call(PointCloudMath.DebugHighLightPointYellow, point);
                                //Debug.Log("normCamDist=" + normCamDist + " : pointRayDist=" + pointRayDist);
                            }

                        }
                        else // out of threshold
                        {
                            //MainThread.Call(PointCloudMath.DebugHighLightPointGray, point);
                        }
                    } // each point inside box
                } // if ray hits box
            } // all boxes


            //if (foundPoints[threadIndex] == true)
            //{
            // if we are first thread, wait for data from other threads
            if (threadIndex == 0)
            {
                while (pointPickingThread2.IsAlive == true)
                {
                    // wait for other thread to finish
                }

                Vector3 nearestPoint = Vector3.zero;

                // if other thread found closer point, use that one
                if (foundPoints[0] == true && foundPoints[1] == true)
                {
                    if (nearestDistancesToCamera[1] < nearestDistancesToCamera[0])
                    {
                        nearestPoint = nearestPoints[1];
                    }
                    else
                    {
                        nearestPoint = nearestPoints[0];
                    }
                }
                else // no hits in both threads
                {
                    if (foundPoints[0] == true) nearestPoint = nearestPoints[0];
                    if (foundPoints[1] == true) nearestPoint = nearestPoints[1];
                }

                if (foundPoints[0] == true || foundPoints[1] == true)
                {
                    MainThread.Call(PointCallBack, nearestPoint);
                    //Debug.Log("(v3) Selected Point Position:" + nearestPoints[threadIndex]);
                }
                else
                {
                    Debug.Log("No hits");
                }
            }

            //MainThread.Call(PointCloudMath.DebugHighLightPointGreen, nearestPoint);
            //}
            //else
            //{
            //    Debug.Log("(v3) No points found (thread=" + threadIndex + ")");
            //}

            stopwatch.Stop();
            if (showDebug) Debug.Log("(v3) PickTimer: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Reset();

            // TODO use array
            if (threadIndex == 0) if (pointPickingThread != null && pointPickingThread.IsAlive == true) pointPickingThread.Abort();
            if (threadIndex == 1) if (pointPickingThread2 != null && pointPickingThread2.IsAlive == true) pointPickingThread2.Abort();

        } // FindClosesPoint

        Vector3[] nearestPoints = new Vector3[2];
        float[] nearestDistancesToCamera = new float[2];
        bool[] foundPoints = new bool[2];

        // if vector3 is inside tile bounds
        bool NodeContainsPoint(PointCloudTile tile, Vector3 point)
        {
            return tile.minX < point.x && tile.maxX > point.x
                && tile.minY < point.y && tile.maxY > point.y
                && tile.minZ < point.z && tile.maxZ > point.z;
        }

        // this gets called after thread finds closest point
        void PointCallBack(System.Object a)
        {
            if (PointWasSelected != null) PointWasSelected((Vector3)a);
        }

        // PUBLIC API

        // return whole cloud bounds
        public Bounds GetBounds()
        {
            return cloudBounds;
        }

        // returns cloud (auto/manual) offset that was used in the converter
        public Vector3 GetOffset()
        {
            return cloudOffset;
        }

        // returns total pointcount
        public long GetTotalPointCount()
        {
            return totalPointCount;
        }

        // returns visible tiles count
        public int GetVisibleTileCount()
        {
            // FIXME not working?
            return cullGroup.QueryIndices(true, new int[tilesCount], 0);
        }

        // returns total visible point count
        public long GetVisiblePointCount()
        {
            // TODO init once
            int[] visibleTiles = new int[tilesCount];
            int visibleTileCount = cullGroup.QueryIndices(true, visibleTiles, 0);
            long counter = 0;
            for (int i = 0; i < visibleTileCount; i++)
            {
                counter += tiles[visibleTiles[i]].visiblePoints;
            }
            return counter;
        }

        // returns all tile bounds
        public Bounds[] GetAllTileBounds()
        {
            Bounds[] results = new Bounds[tilesCount];

            for (int i = 0; i < tilesCount; i++)
            {
                results[i] = new Bounds(tiles[i].center, new Vector3(tiles[i].maxX - tiles[i].minX, tiles[i].maxY - tiles[i].minY, tiles[i].maxZ - tiles[i].minZ));
            }

            return results;
        }

        // returns culling spheres X,Y,Z,Radius
        public Vector4[] GetCullingSpheres()
        {
            var results = new Vector4[tilesCount];

            for (int i = 0; i < tilesCount; i++)
            {
                var pos = boundingSpheres[i].position;
                results[i] = new Vector4(pos.x, pos.y, pos.z, boundingSpheres[i].radius);
            }

            return results;
        }

        public bool InitIsReady()
        {
            return initIsReady;
        }

        // returns how many tiles still in the load queue
        //public int GetLoadQueueCount()
        //{
        //    return loadQueueA.Count + loadQueueB.Count;
        //}



        // EXPERIMENTAL AREA

        public virtual void RunGetPointsInBoundsThread(object sender, BoxData box)
        {
            Debug.LogError("RunGetPointsInBoundsThread: You need to override this custom method to use it.", gameObject);
        }

        public virtual void RunGetPointsInsideBoxThread(object sender, Transform box)
        {
            Debug.LogError("RunGetPointsInsideBoxThread: You need to override this custom method to use it.", gameObject);
        }

        public virtual void RunGetPointsInsideBoxThread(Transform box)
        {
            ParameterizedThreadStart start = new ParameterizedThreadStart(GetPointsInsideBox);
            pointAreaSearchThread = new Thread(start);
            pointAreaSearchThread.IsBackground = true;

            // NOTE: to take renderer bounds and box collider as boxdata (these are required, TODO add validation to check)
            var boxbounds = box.GetComponent<Renderer>().bounds;
            var boxCollider = box.GetComponent<BoxCollider>();

            var boxData = new BoxData();
            boxData.boxPos = box.position;
            boxData.boxRotation = box.rotation;
            boxData.boxScale = box.localScale;
            boxData.boxCenter = boxCollider.center;
            boxData.boxSize = boxCollider.size;
            boxData.boxBounds = boxbounds;

            pointAreaSearchThread.Start(box);
        }

        public virtual void GetPointsInsideBoxCallBack(System.Object a)
        {
            if (GotPointsInsideBoxEvent != null) GotPointsInsideBoxEvent((List<Tuple<int, int>>)a);
        }

        // returns points inside box bounds, AABB (not rotated box), precision is how many points to skip
        public virtual void GetPointsInsideBox(object bounds)
        //public void GetPointsInsideBox(object bounds, object hitTestPrecision)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var boxData = (BoxData)bounds;

            List<Tuple<int, int>> resTiles = new List<Tuple<int, int>>();
            //List<Tuple<int, Vector3, Vector3>> resTiles = new List<Tuple<int, Vector3, Vector3>>();

            //PointCloudTools.DrawBounds(boxbounds, 10);
            // loop all tiles
            for (int i = 0, len = tiles.Length; i < len; i++)
            {
                if (tiles[i].isReady == false) continue;

                // NOTE checks for AABB, not rotated box
                if (PointCloudMath.AABBIntersectsAABB(boxData.boxBounds, tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ))
                {
                    //PointCloudTools.DrawMinMaxBounds(tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ, 3);

                    //Debug.Log("tiles[i].points:"+ tiles[i].);
                    // check if transform box contains point
                    //var len2 = useNativeArrays ? tiles[i].pointsNative.Length : tiles[i].points.Length;
                    int len2 = tiles[i].loadedPoints;
                    //for (int j = 0; j < len2; j++)

                    for (int j = 0; j < len2; j += pointInsideBoxPrecision)
                    {
                        Vector3 point;
#if UNITY_2019_1_OR_NEWER
                        if (useNativeArrays == true)
                        {
                            if (isPackedColors == true)
                            {
                                Vector3 yg;

                                if (isIntPacked == true)
                                {
                                    // TODO not working yet?
                                    int intPointY = PointCloudMath.BytesToInt(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                    Vector3 res = PointCloudMath.SuperUnpacker(intPointY);

                                    yg.x = res.x; // green
                                    yg.y = res.y; // int
                                    point.y = res.z + tiles[i].cellY * gridSize; // y
                                }
                                else
                                {
                                    point.y = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                    Vector2 res = PointCloudMath.SuperUnpacker(point.y, gridSizePackMagic);
                                    yg.x = res.x; // green
                                    yg.y = res.y; // y
                                    point.y = yg.y + tiles[i].cellY * gridSize;
                                }

                                // TODO calculate index once
                                point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                point.z = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4 + 4]);

                                // need to unpack to get proper xyz
                                var xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                var zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);

                                point.x = xr.y + tiles[i].cellX * gridSize;
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                            }
                            else // not using packed colors
                            {
                                // TODO calculate index once
                                point.x = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4], tiles[i].pointsNative[j * 3 * 4 + 1], tiles[i].pointsNative[j * 3 * 4 + 2], tiles[i].pointsNative[j * 3 * 4 + 3]);
                                point.y = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4]);
                                point.z = PointCloudMath.BytesToFloat(tiles[i].pointsNative[j * 3 * 4 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 1 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 2 + 4 + 4], tiles[i].pointsNative[j * 3 * 4 + 3 + 4 + 4]);
                            }
                        }
                        else // not using native arrays
#endif
                        {
                            point = tiles[i].points[j];

                            if (isPackedColors == true)
                            {
                                Vector3 yg;

                                if (isIntPacked == true)
                                {
                                    // NOTE this is not supported! (it is INT, but we have read it as float in regular array)
                                    Vector3 res = PointCloudMath.SuperUnpacker((int)point.y);
                                    yg.x = res.x; // green
                                    yg.y = res.y; // int
                                    point.y = res.z + tiles[i].cellY * gridSize; // y
                                }
                                else
                                {
                                    Vector2 res = PointCloudMath.SuperUnpacker(point.y, gridSizePackMagic);
                                    yg.x = res.x;
                                    yg.y = res.y;
                                    point.y = yg.y + tiles[i].cellY * gridSize;
                                }

                                var xr = PointCloudMath.SuperUnpacker(point.x, gridSizePackMagic);
                                var zb = PointCloudMath.SuperUnpacker(point.z, gridSizePackMagic);

                                point.x = xr.y + tiles[i].cellX * gridSize;
                                point.z = zb.y + tiles[i].cellZ * gridSize;
                            }
                        }

                        if (PointCloudMath.IsPointInsideBoxCollider(point, boxData.boxPos, boxData.boxRotation, boxData.boxScale, boxData.boxCenter, boxData.boxSize))
                        {
                            // TODO could update the points here already (instead of collecting)

                            resTiles.Add(new Tuple<int, int>(i, j));
                            //resTiles.Add(new Tuple<int, Vector3, Vector3>(i, point, pointColor));
                        }
                    }
                }
                else
                {
                    //PointCloudTools.DrawMinMaxBounds(tiles[i].minX, tiles[i].minY, tiles[i].minZ, tiles[i].maxX, tiles[i].maxY, tiles[i].maxZ, 2);
                }
            }

            MainThread.Call(GetPointsInsideBoxCallBack, resTiles);

            stopwatch.Stop();
            Debug.Log("(v3) GetPointsInsideBox: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Reset();

            if (pointAreaSearchThread != null && pointAreaSearchThread.IsAlive == true) pointAreaSearchThread.Abort();
        } // GetPointsInsideBox

        public void ToggleColorMode(bool isRGB)
        {
            useIntensityFile = !isRGB;
            StartCoroutine(RefreshCameraCulling());
        }

        internal bool LoadIntensityTile(int index)
        {
            // NOTE doesnt need to check everytime.. could do that start
            if (File.Exists(filenamesRGB[index] + intExtension) == false)
            {
                Debug.LogError("Intensity file not found: " + filenamesRGB[index] + intExtension);
                return false;
            }

            int newPointCount = tiles[index].totalPoints;
            int dataBytesSize = newPointCount * 12;

            if (useNativeArrays == true)
            {
                if (tiles[index].intensityNative.IsCreated == true) tiles[index].intensityNative.Dispose();
                tiles[index].intensityNative = new NativeArray<byte>(dataBytesSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                var temp = File.ReadAllBytes(filenamesRGB[index] + intExtension);
                if (abortReaderThread || tiles[index].intensityNative.IsCreated == false) return false;
                PointCloudMath.MoveFromByteArray<byte>(ref temp, ref tiles[index].intensityNative);
            }
            else
            {
                tiles[index].intensityColors = new Vector3[newPointCount];
                GCHandle vectorPointer = GCHandle.Alloc(tiles[index].intensityColors, GCHandleType.Pinned);
                IntPtr pV = vectorPointer.AddrOfPinnedObject();
                var temp = File.ReadAllBytes(filenamesRGB[index] + intExtension);
                Marshal.Copy(temp, 0, pV, dataBytesSize);
                vectorPointer.Free();
                //Debug.Log("Loaded intensity tile index: " + index);
            }
            return true;
        }

        // 1 = iterate every point, 2 = every second point..
        public void SetPointInsideBoxScanPrecision(int precision)
        {
            if (precision < 1) precision = 1;
            pointInsideBoxPrecision = precision;
        }

        // returns average gps times for all tiles (in order of tiles)
        public List<double> GetTimestamps()
        {
            List<double> results = new List<double>();
            for (int i = 0, len = tiles.Length; i < len; i++)
            {
                results.Add(tiles[i].averageGPSTime);
            }
            return results;
        }

        // returns overlap ratios for all tiles (in order of tiles)
        public List<float> GetOverlapRations()
        {
            List<float> results = new List<float>();
            for (int i = 0, len = tiles.Length; i < len; i++)
            {
                results.Add(tiles[i].overlapRatio);
            }
            return results;
        }

        // returns tile index and point index, if point is inside box collider
        //public List<Tuple<int, int>> GetPointsInsideBox(Transform boxTransform)
        //{

        //}

        // TODO get node index by position, get node count, get visible nodes, get hidden nodes, ..

    } // class
} // namespace
