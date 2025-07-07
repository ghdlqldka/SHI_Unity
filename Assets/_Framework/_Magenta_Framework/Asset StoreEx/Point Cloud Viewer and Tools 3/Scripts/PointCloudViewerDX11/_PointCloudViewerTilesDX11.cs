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
using System.Diagnostics;
using UnityEngine.Rendering;

using Debug = UnityEngine.Debug;


#if UNITY_2019_1_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace pointcloudviewer.binaryviewer
{
    public class _PointCloudViewerTilesDX11 : PointCloudViewerTilesDX11
    {
        private static string LOG_FORMAT = "<color=#03940F><b>[_PointCloudViewerTilesDX11]</b></color> {0}";

        [SerializeField]
        protected Camera _camera;

        protected string _assetsPath
        {
            get
            {
                return applicationStreamingAssetsPath;
            }
            set
            {
                applicationStreamingAssetsPath = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // applicationStreamingAssetsPath = Application.streamingAssetsPath;
            _assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            FixMainThreadHelper();
        }

        protected override void Start()
        {
            // cam = Camera.main;
            cam = _camera;
            Debug.Assert(cam != null);
            /*
            if (cam == null) 
            { 
                Debug.LogError("Camera Main is missing..", gameObject);
            }
            */

            // validate URP and HDRP
            if (useURPCustomPass == true && useHDRPCustomPass == true)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "URP and HDRP custom passes cannot be used at the same time, please disable one of them");
            }

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
                    Debug.LogErrorFormat(LOG_FORMAT, "Native arrays are not supported with mesh rendering!");
                }

                if (overridePosition == true)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Override Position is not yet supported with mesh rendering");
                }

                // TODO no need? if they are updated on demand in oncull..?
                StartCoroutine(MeshUpdater());
            }

            if (loadAtStart == true)
            {
                abortReaderThread = false;

                // all these are required to do
                rootLoaded = LoadRootFile(rootFile);
                if (rootLoaded == true)
                    StartCoroutine(InitCullingSystem());
            }

            // add warnings if bad setttings
#if UNITY_2019_1_OR_NEWER
            if (useNativeArrays == false && releaseTileMemory == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "useNativeArrays is not enabled, but releaseTileMemory is enabled - Cannot release memory for managed memory tiles");
            }
#endif
        } // Start

        protected override bool LoadRootFile(string filePath)
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
                Debug.LogErrorFormat(LOG_FORMAT, "File not found: " + filePath);
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".pcroot")
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File is not V3 root file (.pcroot extension is required): '" + filePath + "'");
                return false;
            }

            StartWorkerThreads();

            Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) Loading root file: " + filePath);
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
                    Debug.LogErrorFormat(LOG_FORMAT, "v3 header version (" + v3Version + ") is not supported in this viewer!");
                    return false;
                }

                isPackedColors = false;
                isIntPacked = false;
                isClassificationPacked = false;

                // packed
                if (v3Version == 2)
                {
                    isPackedColors = true;
                    Debug.LogWarningFormat(LOG_FORMAT, "(Tiles Viewer) V3 format #2 detected: Packed colors (Make sure you use material that supports PackedColors)");
                }

                if (v3Version == 3)
                {
                    //isPackedColors = true;
                    useLossyBytePacking = true;
                    Debug.LogWarningFormat(LOG_FORMAT, "(Tiles Viewer) V3 format #3 detected: Bytepacked data (Make sure you use material that supports BytePacking)");
                }

                if (v3Version == 4)
                {
                    isPackedColors = true;
                    isIntPacked = true;
                    Debug.LogWarningFormat(LOG_FORMAT, "(Tiles Viewer) V3 format #4 detected: XYZ+RGB+INT data (Make sure you use material that supports INT Packing)");
                }

                if (v3Version == 5)
                {
                    isPackedColors = true;
                    isClassificationPacked = true;
                    Debug.LogWarningFormat(LOG_FORMAT, "(Tiles Viewer) V3 format #5 detected: XYZ+RGB+INT+CLASSIFICATION data (Make sure you use material that supports INT+CLASS Packing)");
                }

                gridSize = float.Parse(globalData[1], CultureInfo.InvariantCulture);
                totalPointCount = long.Parse(globalData[2], CultureInfo.InvariantCulture);
                Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) Total point count = " + totalPointCount + " (" + PointCloudTools.HumanReadableCount(totalPointCount) + ")");
                var minX = float.Parse(globalData[3], CultureInfo.InvariantCulture);
                var minY = float.Parse(globalData[4], CultureInfo.InvariantCulture);
                var minZ = float.Parse(globalData[5], CultureInfo.InvariantCulture);
                var maxX = float.Parse(globalData[6], CultureInfo.InvariantCulture);
                var maxY = float.Parse(globalData[7], CultureInfo.InvariantCulture);
                var maxZ = float.Parse(globalData[8], CultureInfo.InvariantCulture);

                if (showDebug == true) 
                    Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) minXYZ to maxXYZ = " + minX + " - " + minY + " - " + minZ + " to " + maxX + " - " + maxY + " - " + maxZ);

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

                if (showDebug == true)
                    Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) gridsize = " + gridSize + " packMagic = " + packMagic);
                if (showDebug == true)
                    Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) Bounds = " + cloudBounds);

                if (showDebug == true)
                    PointCloudTools.DrawBounds(cloudBounds, 64);
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Failed to parse global values from " + filePath);
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

            Debug.LogFormat(LOG_FORMAT, "(Tiles Viewer) Found " + tilesCount + " tiles");
            if (tilesCount <= 0)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Failed parsing V3 tiles root, no tiles found! Check this file in notepad, does it contain data? Usually this happens if your conversion scaling is wrong (not scaling to smaller), one cell only gets few points.." + filePath);
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
                if (showDebug) 
                    Debug.LogFormat(LOG_FORMAT, "largestTilePointCount: " + largestTilePointCount);
                // create max point count index array
                precalculatedIndices = new int[maxPointsPerTile];
                for (int k = 0; k < maxPointsPerTile; k++)
                {
                    precalculatedIndices[k] = k;
                }
            }

            return true;
        } // LoadRootFile
    } // class
} // namespace
