// Point Cloud Binary Viewer DX11 for runtime parsing
// http://unitycoder.com

using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
using PointCloudHelpers;
using UnityEngine.Rendering;
// using UnityEngine.EventSystems;
using System;
using System.Globalization;
using PointCloudViewer.Structs;
using pointcloudviewer.tools;
using pointcloudviewer.helpers;
// using UnityEditor.Localization.Plugins.XLIFF.V12;
using pointcloudviewer.binaryviewer;




#if !UNITY_SAMSUNGTV && !UNITY_WEBGL
using System.Threading;
using System.IO;
#endif

namespace PointCloudRuntimeViewer
{
    public class _RuntimeViewerDX11 : RuntimeViewerDX11
    {
#if !UNITY_SAMSUNGTV && !UNITY_WEBGL

        private static string LOG_FORMAT = "<color=#03940F><b>[_RuntimeViewerDX11]</b></color> {0}";

        // protected string applicationStreamingAssetsPath;
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

        // public PointCloudFormat pointCloudFormat = PointCloudFormat.USE_FILE_EXTENSION;
        public PointCloudFormat FileFormat
        {
            get
            {
                return pointCloudFormat;
            }
            protected set
            {
                pointCloudFormat = value;
                Debug.LogWarningFormat(LOG_FORMAT, "SET!!!!!!!!!!!, FileFormat : <b><color=yellow>" + value + "</color></b>");
            }
        }

        [SerializeField]
        protected Camera _camera;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            // base.Awake ();

            // pplicationStreamingAssetsPath = Application.streamingAssetsPath;
            _assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";
        }

        protected override IEnumerator Start()
        {
            // cam = Camera.main;
            cam = _camera;

            if (instantiateMaterial == true)
            {
                cloudMaterial = new Material(cloudMaterial);
            }

            // check if MainThread script exists in scene, its required only for threading though
            if (GameObject.Find("#_MainThreadHelper") == null)
            {
                var go = new GameObject("#_MainThreadHelper");
                go.AddComponent<_MainThread>();
            }


            if (useCommandBuffer == true)
            {
                commandBuffer = new CommandBuffer();
                cam.AddCommandBuffer(camDrawPass, commandBuffer);
            }

            if (forceDepthBufferPass == true)
            {
                //depthMaterial = cloudMaterial;
                commandBufferDepth = new CommandBuffer();
                cam.AddCommandBuffer(camDepthPass, commandBufferDepth);
            }

            if (loadAtStart == true)
            {
                // allow app to start first
                yield return new WaitForSecondsRealtime(1);

                try
                {
                    CallImporterThreaded(fullPath);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }

            yield return null;
        }

        public override void CallImporterThreaded(string fullPath)
        {
            Debug.LogFormat(LOG_FORMAT, "CallImporterThreaded(), fullPath : <b>" + fullPath + "</b>");

            // if not full path, try streaming assets
            if (Path.IsPathRooted(fullPath) == false)
            {
                // fullPath = Path.Combine(Application.streamingAssetsPath, fullPath);
                fullPath = Path.Combine(_assetsPath, fullPath);
            }
            if (File.Exists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found: " + fullPath);
                return;
            }

            if (PointCloudTools.CheckIfFileExists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + fullPath);
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "Reading threaded pointcloud file: " + fullPath, gameObject);

            if (FileFormat == PointCloudFormat.USE_FILE_EXTENSION)
            {
                var extension = Path.GetExtension(fullPath).ToUpper();
                switch (extension)
                {
                    case ".ASC":
                        FileFormat = PointCloudFormat.ASC;
                        break;
                    case ".CATIA_ASC":
                        FileFormat = PointCloudFormat.CATIA_ASC;
                        break;
                    case ".CGO":
                        FileFormat = PointCloudFormat.CGO;
                        break;
                    case ".LAS":
                        FileFormat = PointCloudFormat.LAS;
                        break;
                    case ".XYZ":
                        FileFormat = PointCloudFormat.XYZ;
                        break;
                    case ".PCD":
                        FileFormat = PointCloudFormat.PCD_ASCII;
                        break;
                    case ".PLY":
                        FileFormat = PointCloudFormat.PLY_ASCII;
                        break;
                    case ".PTS":
                        FileFormat = PointCloudFormat.PTS;
                        break;
                    case ".XYZRGB":
                        FileFormat = PointCloudFormat.XYZRGB;
                        break;
                    default:
                        Debug.LogErrorFormat(LOG_FORMAT, "Unknown file extension: " + extension + ", trying to import as XYZRGB..");
                        FileFormat = PointCloudFormat.XYZRGB;
                        break;
                }
            }

            ParameterizedThreadStart start = new ParameterizedThreadStart(LoadRawPointCloud);
            switch (FileFormat)
            {
                case PointCloudFormat.ASC:
                    break;
                case PointCloudFormat.CATIA_ASC:
                    break;
                case PointCloudFormat.CGO:
                    break;
                case PointCloudFormat.LAS:
                    break;
                case PointCloudFormat.XYZ:
                    break;
                case PointCloudFormat.PCD_ASCII:
                    start = new ParameterizedThreadStart(LoadRawPointCloud_pcd);
                    break;
                case PointCloudFormat.PLY_ASCII:
                    break;
                case PointCloudFormat.PTS:
                    start = new ParameterizedThreadStart(LoadRawPointCloud_pts);
                    break;
                case PointCloudFormat.XYZRGB:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            importerThread = new Thread(start);
            importerThread.IsBackground = true;
            importerThread.Start(fullPath); // TODO use normal thread, not params
        }

        public override void LoadRawPointCloud(System.Object a)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "LoadRawPointCloud(), useDX11 : " + useDX11);

            // cleanup old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            fullPath = (string)a;

#if false //
            // if not full path, try streaming assets
            if (Path.IsPathRooted(fullPath) == false)
            {
                fullPath = Path.Combine(applicationStreamingAssetsPath, fullPath);
            }

            if (PointCloudTools.CheckIfFileExists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + fullPath);
                return;
            }
#endif

#if false //
            // check if automatic fileformat, get extension
            if (FileFormat == PointCloudFormat.USE_FILE_EXTENSION)
            {
                var extension = Path.GetExtension(fullPath).ToUpper();
                switch (extension)
                {
                    case ".ASC":
                        FileFormat = PointCloudFormat.ASC;
                        break;
                    case ".CATIA_ASC":
                        FileFormat = PointCloudFormat.CATIA_ASC;
                        break;
                    case ".CGO":
                        FileFormat = PointCloudFormat.CGO;
                        break;
                    case ".LAS":
                        FileFormat = PointCloudFormat.LAS;
                        break;
                    case ".XYZ":
                        FileFormat = PointCloudFormat.XYZ;
                        break;
                    case ".PCD":
                        FileFormat = PointCloudFormat.PCD_ASCII;
                        break;
                    case ".PLY":
                        FileFormat = PointCloudFormat.PLY_ASCII;
                        break;
                    case ".PTS":
                        FileFormat = PointCloudFormat.PTS;
                        break;
                    case ".XYZRGB":
                        FileFormat = PointCloudFormat.XYZRGB;
                        break;
                    default:
                        Debug.LogErrorFormat(LOG_FORMAT, "Unknown file extension: " + extension + ", trying to import as XYZRGB..");
                        FileFormat = PointCloudFormat.XYZRGB;
                        break;
                }
            }
#endif

            // Custom reader for LAS binary
            if (FileFormat == PointCloudFormat.LAS)
            {
                //LASDataConvert();
                Debug.LogFormat(LOG_FORMAT, "LAS format is not yet supported in runtime importer:" + fullPath);
                return;
            }

            isLoading = true;
            hasLoadedPointCloud = false;

            Debug.LogFormat(LOG_FORMAT, "Loading " + FileFormat + " file: " + fullPath);

            long lines = 0;

            // get initial data (so can check if data is ok)
            using (StreamReader streamReader = new StreamReader(File.OpenRead(fullPath)))
            {
                double x = 0, y = 0, z = 0;
                float r = 0, g = 0, b = 0; //,nx=0,ny=0,nz=0;; // init vals
                string line = null;
                string[] row = null;

                PeekHeaderData headerCheck;
                headerCheck.x = 0;
                headerCheck.y = 0;
                headerCheck.z = 0;
                headerCheck.linesRead = 0;

                switch (FileFormat)
                {
                    case PointCloudFormat.ASC: // ASC (space at front)
                        {
                            headerCheck = PeekHeader.PeekHeaderASC(streamReader, readRGB);
                            if (headerCheck.readSuccess == false)
                            { 
                                streamReader.Close();
                                return;
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;

                    case PointCloudFormat.CGO: // CGO	(counter at first line and uses comma)
                        {
                            headerCheck = PeekHeader.PeekHeaderCGO(streamReader, readRGB);
                            if (headerCheck.readSuccess == false)
                            { 
                                streamReader.Close(); 
                                return;
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;

                    case PointCloudFormat.CATIA_ASC: // CATIA ASC (with header and Point Format           = 'X %f Y %f Z %f')
                        {
                            headerCheck = PeekHeader.PeekHeaderCATIA_ASC(streamReader, ref readRGB);
                            if (headerCheck.readSuccess == false) 
                            { 
                                streamReader.Close(); 
                                return; 
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;

                    case PointCloudFormat.XYZRGB:
                    case PointCloudFormat.XYZ: // XYZ RGB(INT)
                        {
                            headerCheck = PeekHeader.PeekHeaderXYZ(streamReader, ref readRGB);
                            if (headerCheck.readSuccess == false) 
                            {
                                streamReader.Close();
                                return;
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;

                    case PointCloudFormat.PTS: // PTS (INT) (RGB)
                        {
                            headerCheck = PeekHeader.PeekHeaderPTS(streamReader, readRGB, readIntensity, ref masterPointCount);
                            if (headerCheck.readSuccess == false)
                            {
                                streamReader.Close();
                                return;
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;

                    case PointCloudFormat.PLY_ASCII: // PLY (ASCII)
                        {
                            headerCheck = PeekHeader.PeekHeaderPLY(streamReader, readRGB, ref masterPointCount, ref plyHasNormals, ref plyHasDensity);
                            if (headerCheck.readSuccess == false) 
                            { 
                                streamReader.Close(); 
                                return;
                            }
                        }
                        break;

                    case PointCloudFormat.PCD_ASCII: // PCD (ASCII)
                        {
                            headerCheck = PeekHeader.PeekHeaderPCD(streamReader, ref readRGB, ref masterPointCount);
                            if (headerCheck.readSuccess == false)
                            { 
                                streamReader.Close();
                                return;
                            }
                        }
                        break;

                    default:
                        Debug.Assert(false);
                        break;

                } // switch format

                if (autoOffsetNearZero == true)
                {
                    manualOffset = new Vector3((float)headerCheck.x, (float)headerCheck.y, (float)headerCheck.z);
                }

                // scaling enabled, scale offset too
                if (useUnitScale == true)
                    manualOffset *= unitScale;

                // progressbar
                long progressCounter = 0;

                // get total amount of points
                if (FileFormat == PointCloudFormat.PLY_ASCII || FileFormat == PointCloudFormat.PTS || 
                    FileFormat == PointCloudFormat.CGO || FileFormat == PointCloudFormat.PCD_ASCII)
                {
                    lines = masterPointCount;

                    // reset back to start of file
                    streamReader.DiscardBufferedData();
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    streamReader.BaseStream.Position = 0;

                    // get back to before first actual data line
                    for (int i = 0; i < headerCheck.linesRead - 1; i++)
                    {
                        streamReader.ReadLine();
                    }
                }
                else
                { // other formats need to be read completely

                    // reset back to start of file
                    streamReader.DiscardBufferedData();
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    streamReader.BaseStream.Position = 0;

                    // get back to first actual data line
                    for (int i = 0; i < headerCheck.linesRead; i++)
                    {
                        streamReader.ReadLine();
                    }
                    lines = 0;

                    // calculate actual point data lines
                    int splitCount = 0;
                    while (streamReader.EndOfStream == false && abortReaderThread == false)
                    {
                        line = streamReader.ReadLine();

                        if (progressCounter > 256000)
                        {
                            progressCounter = 0;
                        }

                        progressCounter++;

                        if (line.Length > 9)
                        {
                            splitCount = CharCount(line, ' ');
                            if (splitCount > 2 && splitCount < 16)
                            {
                                lines++;
                            }
                        }
                    }

                    // reset back to start of data
                    streamReader.DiscardBufferedData();
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    streamReader.BaseStream.Position = 0;

                    // now skip header lines
                    for (int i = 0; i < headerCheck.linesRead; i++)
                    {
                        streamReader.ReadLine();
                    }

                    masterPointCount = lines;
                }

                // create buffers
                points = new Vector3[masterPointCount];

                if (readRGB == true || readIntensity == true)
                {
                    pointColors = new Vector3[masterPointCount];
                }

                totalPoints = (int)masterPointCount;

                progressCounter = 0;

                int skippedRows = 0;
                long rowCount = 0;
                bool haveMoreToRead = true;

                // process all points
                while (haveMoreToRead == true && abortReaderThread == false)
                {
                    if (progressCounter > 256000)
                    {
                        // TODO: add runtime progressbar
                        //EditorUtility.DisplayProgressBar(appName, "Converting point cloud to binary file", rowCount / (float)lines);
                        progressCounter = 0;
                    }

                    progressCounter++;

                    line = streamReader.ReadLine();

                    if (line != null)// && line.Length > 9)
                    {
                        // trim duplicate spaces
                        line = line.Replace("   ", " ").Replace("  ", " ").Trim();
                        row = line.Split(' ');

                        if (row.Length > 2)
                        {
                            switch (FileFormat)
                            {
                                case PointCloudFormat.ASC: // ASC
                                    if (line.IndexOf('!') == 0 || line.IndexOf('*') == 0)
                                    {
                                        skippedRows++;
                                        continue;
                                    }
                                    x = double.Parse(row[0], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2], CultureInfo.InvariantCulture);
                                    break;

                                case PointCloudFormat.CGO: // CGO	(counter at first line and uses comma)
                                    if (line.IndexOf('!') == 0 || line.IndexOf('*') == 0)
                                    {
                                        skippedRows++;
                                        continue;
                                    }
                                    x = double.Parse(row[0].Replace(",", "."), CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1].Replace(",", "."), CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2].Replace(",", "."), CultureInfo.InvariantCulture);
                                    break;

                                case PointCloudFormat.CATIA_ASC: // CATIA ASC (with header and Point Format           = 'X %f Y %f Z %f')
                                    if (line.IndexOf('!') == 0 || line.IndexOf('*') == 0)
                                    {
                                        skippedRows++;
                                        continue;
                                    }
                                    x = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[3], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[5], CultureInfo.InvariantCulture);
                                    break;

                                case PointCloudFormat.XYZRGB:
                                case PointCloudFormat.XYZ: // XYZ RGB(INT)
                                    if (double.TryParse(row[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x) == false)
                                    { 
                                        skippedRows++; 
                                        continue;
                                    }

                                    if (double.TryParse(row[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y) == false)
                                    {
                                        skippedRows++;
                                        continue;
                                    }
                                    
                                    if (double.TryParse(row[2], NumberStyles.Any, CultureInfo.InvariantCulture, out z) == false)
                                    { 
                                        skippedRows++;
                                        continue;
                                    }

                                    if (readRGB == true)
                                    {
                                        if (int.TryParse(row[3], NumberStyles.Any, CultureInfo.InvariantCulture, out int rInt) == false)
                                        { 
                                            skippedRows++;
                                            continue;
                                        }

                                        if (int.TryParse(row[4], NumberStyles.Any, CultureInfo.InvariantCulture, out int gInt) == false)
                                        { 
                                            skippedRows++; 
                                            continue;
                                        }

                                        if (int.TryParse(row[5], NumberStyles.Any, CultureInfo.InvariantCulture, out int bInt) == false)
                                        { 
                                            skippedRows++;
                                            continue;
                                        }

                                        r = LUT255[rInt];
                                        g = LUT255[gInt];
                                        b = LUT255[bInt];
                                    }
                                    break;

                                case PointCloudFormat.PTS: // PTS (INT) (RGB)
                                    x = double.Parse(row[0], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2], CultureInfo.InvariantCulture);

                                    if (readRGB == true)
                                    {
                                        if (row.Length == 7) // XYZIRGB
                                        {
                                            r = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 6) // XYZRGB
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 9) // XYZRGBNxNyNz
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 10) // XYZIRGBNxNyNz
                                        {
                                            r = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                        }
                                    }
                                    else if (readIntensity == true)
                                    {
                                        if (row.Length == 4 || row.Length == 7) // XYZI or XYZIRGB
                                        {
                                            r = Remap(float.Parse(row[3], CultureInfo.InvariantCulture), -2048, 2047, 0, 1);
                                            g = r;
                                            b = r;
                                        }
                                    }
                                    break;

                                case PointCloudFormat.PLY_ASCII: // PLY (ASCII)
                                    x = double.Parse(row[0], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2], CultureInfo.InvariantCulture);

                                    if (readRGB == true)
                                    {
                                        // TODO: need to fix PLY CloudCompare normals, they are before RGB
                                        if (plyHasNormals == true)
                                        {
                                            r = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[7], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[8], CultureInfo.InvariantCulture)];
                                        }
                                        else if (plyHasDensity == false) // no normals or density
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                        else // no normals, but have density
                                        {
                                            r = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                        }
                                        //a = float.Parse(row[6], CultureInfo.InvariantCulture)/255; // TODO: alpha not supported yet
                                    }

                                    break;

                                case PointCloudFormat.PCD_ASCII: // pcd ascii
                                    x = double.Parse(row[0], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2], CultureInfo.InvariantCulture);

                                    if (readRGB == true)
                                    {
                                        // TODO: need to check both rgb formats
                                        if (row.Length == 4)
                                        {
                                            var rgb = (int)decimal.Parse(row[3], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                                            r = (rgb >> 16) & 0x0000ff;
                                            g = (rgb >> 8) & 0x0000ff;
                                            b = (rgb) & 0x0000ff;
                                            r = LUT255[(int)r];
                                            g = LUT255[(int)g];
                                            b = LUT255[(int)b];
                                        }
                                        else if (row.Length == 6)
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                    }
                                    break;

                                default:
                                    Debug.Assert(false);
                                    break;

                            } // switch

                            // scaling enabled
                            if (useUnitScale == true)
                            {
                                x *= unitScale;
                                y *= unitScale;
                                z *= unitScale;
                            }

                            // manual offset enabled
                            if (autoOffsetNearZero == true || useManualOffset == true) // NOTE: can use only one at a time
                            {
                                x -= manualOffset.x;
                                y -= manualOffset.y;
                                z -= manualOffset.z;
                            }

                            // if flip
                            if (flipYZ == true)
                            {
                                points[rowCount].Set((float)x, (float)z, (float)y);
                            }
                            else
                            {
                                points[rowCount].Set((float)x, (float)y, (float)z);
                            }

                            // if have color data
                            if (readRGB == true || readIntensity == true)
                            {
                                pointColors[rowCount].Set(r, g, b);
                            }

                            rowCount++;
                        }
                        else
                        { // if row length
                            skippedRows++;
                        }
                    }
                    else
                    { // if linelen
                        skippedRows++;
                    }

                    // reached end or enough points
                    if (streamReader.EndOfStream == true || rowCount >= masterPointCount)
                    {
                        if (skippedRows > 0)
                        {
                            Debug.LogWarning("Parser skipped " + skippedRows + " rows (wrong length or bad data)");
                        }

                        if (rowCount < masterPointCount) // error, file ended too early, not enough points
                        {
                            Debug.LogWarning("File does not contain enough points, fixing point count to " + rowCount + " (expected : " + masterPointCount + ")");
                        }
                        haveMoreToRead = false;
                    }
                } // while loop reading file


                // done reading, display it now
                isLoading = false;
                if (useDX11 == true)
                {
                    _MainThread.Call(InitDX11Buffers);
                    Thread.Sleep(10); // wait for buffers to be ready_
                }
                _MainThread.Call(OnLoadingCompleteCallBack, fullPath);

                hasLoadedPointCloud = true;
            } // using reader

            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@ Finished loading. @@@@@@@@@@@@@@@@@");

            // if mesh version, build meshes
            if (useDX11 == false)
            {
                // build mesh assets
                int indexCount = 0;

#if UNITY_2017_3_OR_NEWER
                int MaxVertexCountPerMesh = 1000000;
#else
                int MaxVertexCountPerMesh = 65000;
#endif

                Vector3[] verts = new Vector3[MaxVertexCountPerMesh];
                Vector2[] uvs2 = new Vector2[MaxVertexCountPerMesh];
                int[] tris = new int[MaxVertexCountPerMesh];
                Color[] cols = new Color[MaxVertexCountPerMesh];
                Vector3[] norms = new Vector3[MaxVertexCountPerMesh];

                // process all point data into meshes
                for (int i = 0, len = points.Length; i < len; i++)
                {
                    verts[indexCount] = points[i];
                    uvs2[indexCount].Set(points[i].x, points[i].y);
                    tris[indexCount] = i % MaxVertexCountPerMesh;

                    if (readRGB || readIntensity)
                    {
                        cols[indexCount] = new Color(pointColors[i].x, pointColors[i].y, pointColors[i].z, 1);
                    }
                    //if (readNormals) normals2[indexCount] = normalArray[i];

                    indexCount++;

                    if (indexCount >= MaxVertexCountPerMesh || i == MaxVertexCountPerMesh - 1)
                    {
                        var m = new TempMesh();
                        m.verts = verts;
                        m.tris = tris;
                        m.cols = cols;
                        m.norms = norms;
                        //Debug.Log(m.verts.Length);
                        isBuildingMesh = true;
                        _MainThread.Call(BuildMesh, m);
                        while (isBuildingMesh == true)
                        {
                            Thread.Sleep(50);
                        }

                        indexCount = 0;

                        // need to clear arrays, should use lists otherwise last mesh has too many verts (or slice last array)
                        System.Array.Clear(verts, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(uvs2, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(tris, 0, MaxVertexCountPerMesh);
                        if (readRGB || readIntensity)
                            System.Array.Clear(cols, 0, MaxVertexCountPerMesh);
                    }
                } // all points
            } // use dx11

            // if caching, save as bin
            if (cacheBinFile == true)
            {
                var outputFile = fullPath + ".bin";

                if (File.Exists(outputFile) == true && overrideExistingCacheFile == false)
                {
                    Debug.Log("Cache file already exists, not saving new cached file.." + outputFile);
                    return;
                }

                var writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));
                if (writer == null)
                {
                    Debug.LogError("Cannot output file: " + outputFile);
                    return;
                }

                byte binaryVersion = 1;
                writer.Write(binaryVersion);
                writer.Write((System.Int32)masterPointCount);
                writer.Write(readRGB | readIntensity);

                for (int i = 0, length = points.Length; i < length; i++)
                {
                    writer.Write(points[i].x);
                    writer.Write(points[i].y);
                    writer.Write(points[i].z);
                    if (readRGB == true || readIntensity == true)
                    {
                        writer.Write(pointColors[i].x);
                        writer.Write(pointColors[i].y);
                        writer.Write(pointColors[i].z);
                    }
                }
                writer.Close();
                Debug.Log("Finished saving cached file: " + outputFile);
            } // cache
        } // LoadRawPointCloud()

        protected virtual void LoadRawPointCloud_pcd(System.Object a)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "LoadRawPointCloud_<color=red>pcd</color>(), useDX11 : " + useDX11);

            Debug.Assert(FileFormat == PointCloudFormat.PCD_ASCII);

            // cleanup old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            fullPath = (string)a;

            isLoading = true;
            hasLoadedPointCloud = false;

            Debug.LogFormat(LOG_FORMAT, "Loading " + FileFormat + ", file: " + fullPath);

            long lines = 0;

            // get initial data (so can check if data is ok)
            using (StreamReader streamReader = new StreamReader(File.OpenRead(fullPath)))
            {
                double x = 0, y = 0, z = 0;
                float _r = 0, _g = 0, _b = 0; //,nx=0,ny=0,nz=0;; // init vals
                string line = null;
                string[] row = null;

                // PeekHeaderData headerCheck;
                _PcdHeaderData headerCheck = new _PcdHeaderData();
                headerCheck.x = 0;
                headerCheck.y = 0;
                headerCheck.z = 0;
                headerCheck.linesRead = 0;

                // headerCheck = _PeekHeader.PeekHeaderPCD(streamReader, ref readRGB, ref masterPointCount);
                headerCheck = _PeekHeader.PeekHeaderPCD(streamReader, ref readRGB, ref readIntensity, ref masterPointCount);

                if (headerCheck.readSuccess == false)
                {
                    streamReader.Close();
                    return;
                }

                if (autoOffsetNearZero == true)
                {
                    manualOffset = new Vector3((float)headerCheck.x, (float)headerCheck.y, (float)headerCheck.z);
                }

                // scaling enabled, scale offset too
                if (useUnitScale == true)
                    manualOffset *= unitScale;

                // progressbar
                long progressCounter = 0;

                // get total amount of points
                // if (FileFormat == PointCloudFormat.PLY_ASCII || FileFormat == PointCloudFormat.PTS ||
                //     FileFormat == PointCloudFormat.CGO || FileFormat == PointCloudFormat.PCD_ASCII)
                {
                    lines = masterPointCount;

                    // reset back to start of file
                    streamReader.DiscardBufferedData();
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    streamReader.BaseStream.Position = 0;

                    // get back to before first actual data line
                    for (int i = 0; i < headerCheck.linesRead - 1; i++)
                    {
                        streamReader.ReadLine();
                    }
                }
                

                // create buffers
                points = new Vector3[masterPointCount];

                if (readRGB == true || readIntensity == true)
                {
                    pointColors = new Vector3[masterPointCount];
                }

                totalPoints = (int)masterPointCount;

                progressCounter = 0;

                int skippedRows = 0;
                long rowCount = 0;
                bool haveMoreToRead = true;

                // process all points
                while (haveMoreToRead == true && abortReaderThread == false)
                {
                    if (progressCounter > 256000)
                    {
                        // TODO: add runtime progressbar
                        //EditorUtility.DisplayProgressBar(appName, "Converting point cloud to binary file", rowCount / (float)lines);
                        progressCounter = 0;
                    }

                    progressCounter++;

                    line = streamReader.ReadLine();

                    if (line != null)// && line.Length > 9)
                    {
                        // trim duplicate spaces
                        line = line.Replace("   ", " ").Replace("  ", " ").Trim();
                        row = line.Split(' ');

                        if (row.Length > 2)
                        {
                            x = double.Parse(row[0], CultureInfo.InvariantCulture);
                            y = double.Parse(row[1], CultureInfo.InvariantCulture);
                            z = double.Parse(row[2], CultureInfo.InvariantCulture);

                            if (readRGB == true)
                            {
                                // TODO: need to check both rgb formats
                                if (row.Length == 4)
                                {
                                    var rgb = (int)decimal.Parse(row[3], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                                    _r = (rgb >> 16) & 0x0000ff;
                                    _g = (rgb >> 8) & 0x0000ff;
                                    _b = (rgb) & 0x0000ff;
                                    _r = LUT255[(int)_r];
                                    _g = LUT255[(int)_g];
                                    _b = LUT255[(int)_b];
                                }
                                else if (row.Length == 6)
                                {
                                    _r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                    _g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                    _b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                }
                            }
#if true // customization!!!!!!
                            else if (readIntensity == true)
                            {
                                // Debug.LogFormat(LOG_FORMAT, "###################################################, row.Length : " + row.Length);
                                // -1.2488459 -1.7935097 2.7711847 29 0 0 0 0
                                if (row.Length == 8)
                                {
                                    float intensity = float.Parse(row[3]);
                                    Color _color = _PcdFileReader.IntensityToJet(intensity);
                                    _r = _color.r;
                                    _g = _color.g;
                                    _b = _color.b;
                                }
                            }
#endif

                            // scaling enabled
                            if (useUnitScale == true)
                            {
                                x *= unitScale;
                                y *= unitScale;
                                z *= unitScale;
                            }

                            // manual offset enabled
                            if (autoOffsetNearZero == true || useManualOffset == true) // NOTE: can use only one at a time
                            {
                                x -= manualOffset.x;
                                y -= manualOffset.y;
                                z -= manualOffset.z;
                            }

                            // if flip
                            if (flipYZ == true)
                            {
                                points[rowCount].Set((float)x, (float)z, (float)y);
                            }
                            else
                            {
                                points[rowCount].Set((float)x, (float)y, (float)z);
                            }

                            // if have color data
                            if (readRGB == true || readIntensity == true)
                            {
                                pointColors[rowCount].Set(_r, _g, _b);
                            }

                            rowCount++;
                        }
                        else
                        { // if row length
                            skippedRows++;
                        }
                    }
                    else
                    { // if linelen
                        skippedRows++;
                    }

                    // reached end or enough points
                    if (streamReader.EndOfStream == true || rowCount >= masterPointCount)
                    {
                        if (skippedRows > 0)
                        {
                            Debug.LogWarning("Parser skipped " + skippedRows + " rows (wrong length or bad data)");
                        }

                        if (rowCount < masterPointCount) // error, file ended too early, not enough points
                        {
                            Debug.LogWarning("File does not contain enough points, fixing point count to " + rowCount + " (expected : " + masterPointCount + ")");
                        }
                        haveMoreToRead = false;
                    }
                } // while loop reading file


                // done reading, display it now
                isLoading = false;
                if (useDX11 == true)
                {
                    _MainThread.Call(InitDX11Buffers_pcd);
                    Thread.Sleep(10); // wait for buffers to be ready_
                }
                _MainThread.Call(OnLoadingCompleteCallBack, fullPath);

                hasLoadedPointCloud = true;
            } // using reader

            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@ Finished loading. @@@@@@@@@@@@@@@@@");

            // if mesh version, build meshes
            if (useDX11 == false)
            {
                // build mesh assets
                int indexCount = 0;

#if UNITY_2017_3_OR_NEWER
                int MaxVertexCountPerMesh = 1000000;
#else
                int MaxVertexCountPerMesh = 65000;
#endif

                Vector3[] verts = new Vector3[MaxVertexCountPerMesh];
                Vector2[] uvs2 = new Vector2[MaxVertexCountPerMesh];
                int[] tris = new int[MaxVertexCountPerMesh];
                Color[] cols = new Color[MaxVertexCountPerMesh];
                Vector3[] norms = new Vector3[MaxVertexCountPerMesh];

                // process all point data into meshes
                for (int i = 0, len = points.Length; i < len; i++)
                {
                    verts[indexCount] = points[i];
                    uvs2[indexCount].Set(points[i].x, points[i].y);
                    tris[indexCount] = i % MaxVertexCountPerMesh;

                    if (readRGB || readIntensity)
                    {
                        cols[indexCount] = new Color(pointColors[i].x, pointColors[i].y, pointColors[i].z, 1);
                    }
                    //if (readNormals) normals2[indexCount] = normalArray[i];

                    indexCount++;

                    if (indexCount >= MaxVertexCountPerMesh || i == MaxVertexCountPerMesh - 1)
                    {
                        var m = new TempMesh();
                        m.verts = verts;
                        m.tris = tris;
                        m.cols = cols;
                        m.norms = norms;
                        //Debug.Log(m.verts.Length);
                        isBuildingMesh = true;
                        _MainThread.Call(BuildMesh, m);
                        while (isBuildingMesh == true)
                        {
                            Thread.Sleep(50);
                        }

                        indexCount = 0;

                        // need to clear arrays, should use lists otherwise last mesh has too many verts (or slice last array)
                        System.Array.Clear(verts, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(uvs2, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(tris, 0, MaxVertexCountPerMesh);
                        if (readRGB || readIntensity)
                            System.Array.Clear(cols, 0, MaxVertexCountPerMesh);
                    }
                } // all points
            } // use dx11

            // if caching, save as bin
            if (cacheBinFile == true)
            {
                var outputFile = fullPath + ".bin";

                if (File.Exists(outputFile) == true && overrideExistingCacheFile == false)
                {
                    Debug.Log("Cache file already exists, not saving new cached file.." + outputFile);
                    return;
                }

                var writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));
                if (writer == null)
                {
                    Debug.LogError("Cannot output file: " + outputFile);
                    return;
                }

                byte binaryVersion = 1;
                writer.Write(binaryVersion);
                writer.Write((System.Int32)masterPointCount);
                writer.Write(readRGB | readIntensity);

                for (int i = 0, length = points.Length; i < length; i++)
                {
                    writer.Write(points[i].x);
                    writer.Write(points[i].y);
                    writer.Write(points[i].z);
                    if (readRGB == true || readIntensity == true)
                    {
                        writer.Write(pointColors[i].x);
                        writer.Write(pointColors[i].y);
                        writer.Write(pointColors[i].z);
                    }
                }
                writer.Close();
                Debug.Log("Finished saving cached file: " + outputFile);
            } // cache
        } // LoadRawPointCloud()

        protected virtual void LoadRawPointCloud_pts(System.Object a)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "LoadRawPointCloud()_pts, useDX11 : " + useDX11);

            // cleanup old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            fullPath = (string)a;

            isLoading = true;
            hasLoadedPointCloud = false;

            Debug.LogFormat(LOG_FORMAT, "Loading " + FileFormat + " file: " + fullPath);

            long lines = 0;

            // get initial data (so can check if data is ok)
            using (StreamReader streamReader = new StreamReader(File.OpenRead(fullPath)))
            {
                double x = 0, y = 0, z = 0;
                float r = 0, g = 0, b = 0; //,nx=0,ny=0,nz=0;; // init vals
                string line = null;
                string[] row = null;

                PeekHeaderData headerCheck;
                headerCheck.x = 0; headerCheck.y = 0; headerCheck.z = 0;
                headerCheck.linesRead = 0;

                switch (FileFormat)
                {
                    
                    case PointCloudFormat.PTS: // PTS (INT) (RGB)
                        {
                            headerCheck = PeekHeader.PeekHeaderPTS(streamReader, readRGB, readIntensity, ref masterPointCount);
                            if (headerCheck.readSuccess == false)
                            {
                                streamReader.Close();
                                return;
                            }
                            lines = headerCheck.linesRead;
                        }
                        break;
                } // switch format

                if (autoOffsetNearZero == true)
                {
                    manualOffset = new Vector3((float)headerCheck.x, (float)headerCheck.y, (float)headerCheck.z);
                }

                // scaling enabled, scale offset too
                if (useUnitScale == true)
                {
                    manualOffset *= unitScale;
                }

                // progressbar
                long progressCounter = 0;

                // get total amount of points

                lines = masterPointCount;

                // reset back to start of file
                streamReader.DiscardBufferedData();
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                streamReader.BaseStream.Position = 0;

                // get back to before first actual data line
                for (int i = 0; i < headerCheck.linesRead - 1; i++)
                {
                    streamReader.ReadLine();
                }

                // create buffers
                points = new Vector3[masterPointCount];

                if (readRGB == true || readIntensity == true)
                {
                    pointColors = new Vector3[masterPointCount];
                }

                totalPoints = (int)masterPointCount;

                progressCounter = 0;

                int skippedRows = 0;
                long rowCount = 0;
                bool haveMoreToRead = true;

                // process all points
                while (haveMoreToRead == true && abortReaderThread == false)
                {
                    if (progressCounter > 256000)
                    {
                        // TODO: add runtime progressbar
                        //EditorUtility.DisplayProgressBar(appName, "Converting point cloud to binary file", rowCount / (float)lines);
                        progressCounter = 0;
                    }

                    progressCounter++;

                    line = streamReader.ReadLine();

                    if (line != null)// && line.Length > 9)
                    {
                        // trim duplicate spaces
                        line = line.Replace("   ", " ").Replace("  ", " ").Trim();
                        row = line.Split(' ');

                        if (row.Length > 2)
                        {
                            switch (FileFormat)
                            {
                                case PointCloudFormat.PTS: // PTS (INT) (RGB)
                                    x = double.Parse(row[0], CultureInfo.InvariantCulture);
                                    y = double.Parse(row[1], CultureInfo.InvariantCulture);
                                    z = double.Parse(row[2], CultureInfo.InvariantCulture);

                                    if (readRGB == true)
                                    {
                                        if (row.Length == 7) // XYZIRGB
                                        {
                                            r = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 6) // XYZRGB
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 9) // XYZRGBNxNyNz
                                        {
                                            r = LUT255[int.Parse(row[3], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                        }
                                        else if (row.Length == 10) // XYZIRGBNxNyNz
                                        {
                                            r = LUT255[int.Parse(row[4], CultureInfo.InvariantCulture)];
                                            g = LUT255[int.Parse(row[5], CultureInfo.InvariantCulture)];
                                            b = LUT255[int.Parse(row[6], CultureInfo.InvariantCulture)];
                                        }
                                    }
                                    else if (readIntensity == true)
                                    {
                                        if (row.Length == 4 || row.Length == 7) // XYZI or XYZIRGB
                                        {
                                            r = Remap(float.Parse(row[3], CultureInfo.InvariantCulture), -2048, 2047, 0, 1);
                                            g = r;
                                            b = r;
                                        }
                                    }
                                    break;

                                default:
                                    Debug.LogError("> Error Unknown format:" + FileFormat);
                                    break;

                            } // switch

                            // scaling enabled
                            if (useUnitScale == true)
                            {
                                x *= unitScale;
                                y *= unitScale;
                                z *= unitScale;
                            }

                            // manual offset enabled
                            if (autoOffsetNearZero == true || useManualOffset == true) // NOTE: can use only one at a time
                            {
                                x -= manualOffset.x;
                                y -= manualOffset.y;
                                z -= manualOffset.z;
                            }

                            // if flip
                            if (flipYZ == true)
                            {
                                points[rowCount].Set((float)x, (float)z, (float)y);
                            }
                            else
                            {
                                points[rowCount].Set((float)x, (float)y, (float)z);
                            }

                            // if have color data
                            if (readRGB == true || readIntensity == true)
                            {
                                pointColors[rowCount].Set(r, g, b);
                            }
                            /*
							// if have normals data, TODO: not possible yet
							if (readNormals)
							{
								writer.Write(nx);
								writer.Write(ny);
								writer.Write(nz);
							}
							*/

                            rowCount++;
                        }
                        else
                        { // if row length
                            skippedRows++;
                        }

                    }
                    else
                    { // if linelen
                        skippedRows++;
                    }


                    // reached end or enough points
                    if (streamReader.EndOfStream == true || rowCount >= masterPointCount)
                    {

                        if (skippedRows > 0) 
                            Debug.LogWarning("Parser skipped " + skippedRows + " rows (wrong length or bad data)");
                        //Debug.Log(masterVertexCount);

                        if (rowCount < masterPointCount) // error, file ended too early, not enough points
                        {
                            Debug.LogWarning("File does not contain enough points, fixing point count to " + rowCount + " (expected : " + masterPointCount + ")");
                            // fix header point count
                            //                            writer.BaseStream.Seek(0, SeekOrigin.Begin);
                            //                            writer.Write(binaryVersion);
                            //                            writer.Write((System.Int32)rowCount);
                        }
                        haveMoreToRead = false;
                    }
                } // while loop reading file


                // done reading, display it now
                isLoading = false;
                if (useDX11 == true)
                {
                    _MainThread.Call(InitDX11Buffers);
                    Thread.Sleep(10); // wait for buffers to be ready_
                }
                _MainThread.Call(OnLoadingCompleteCallBack, fullPath);

                hasLoadedPointCloud = true;
            } // using reader

            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@ Finished loading. @@@@@@@@@@@@@@@@@");

            // if mesh version, build meshes
            if (useDX11 == false)
            {
                // build mesh assets
                int indexCount = 0;

#if UNITY_2017_3_OR_NEWER
                int MaxVertexCountPerMesh = 1000000;
#else
                int MaxVertexCountPerMesh = 65000;
#endif

                Vector3[] verts = new Vector3[MaxVertexCountPerMesh];
                Vector2[] uvs2 = new Vector2[MaxVertexCountPerMesh];
                int[] tris = new int[MaxVertexCountPerMesh];
                Color[] cols = new Color[MaxVertexCountPerMesh];
                Vector3[] norms = new Vector3[MaxVertexCountPerMesh];

                // process all point data into meshes
                for (int i = 0, len = points.Length; i < len; i++)
                {
                    verts[indexCount] = points[i];
                    uvs2[indexCount].Set(points[i].x, points[i].y);
                    tris[indexCount] = i % MaxVertexCountPerMesh;

                    if (readRGB || readIntensity)
                    {
                        cols[indexCount] = new Color(pointColors[i].x, pointColors[i].y, pointColors[i].z, 1);
                    }
                    //if (readNormals) normals2[indexCount] = normalArray[i];

                    indexCount++;

                    if (indexCount >= MaxVertexCountPerMesh || i == MaxVertexCountPerMesh - 1)
                    {
                        var m = new TempMesh();
                        m.verts = verts;
                        m.tris = tris;
                        m.cols = cols;
                        m.norms = norms;
                        //Debug.Log(m.verts.Length);
                        isBuildingMesh = true;
                        _MainThread.Call(BuildMesh, m);
                        while (isBuildingMesh == true)
                        {
                            Thread.Sleep(50);
                        }
                        //if (addMeshesToScene && go != null) if (createLODS) BuildLODS(go, vertices2, triangles2, colors2, normals2);

                        indexCount = 0;

                        // need to clear arrays, should use lists otherwise last mesh has too many verts (or slice last array)
                        System.Array.Clear(verts, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(uvs2, 0, MaxVertexCountPerMesh);
                        System.Array.Clear(tris, 0, MaxVertexCountPerMesh);
                        if (readRGB || readIntensity) 
                            System.Array.Clear(cols, 0, MaxVertexCountPerMesh);
                        //if (readNormals) System.Array.Clear(norms, 0, MaxVertexCountPerMesh);
                    }
                } // all points
            } // use dx11

            // if caching, save as bin
            if (cacheBinFile == true)
            {
                var outputFile = fullPath + ".bin";

                if (File.Exists(outputFile) == true && overrideExistingCacheFile == false)
                {
                    Debug.Log("Cache file already exists, not saving new cached file.." + outputFile);
                    return;
                }

                var writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));
                if (writer == null)
                {
                    Debug.LogError("Cannot output file: " + outputFile);
                    return;
                }

                byte binaryVersion = 1;
                writer.Write(binaryVersion);
                writer.Write((System.Int32)masterPointCount);
                writer.Write(readRGB | readIntensity);

                for (int i = 0, length = points.Length; i < length; i++)
                {
                    writer.Write(points[i].x);
                    writer.Write(points[i].y);
                    writer.Write(points[i].z);
                    if (readRGB == true || readIntensity == true)
                    {
                        writer.Write(pointColors[i].x);
                        writer.Write(pointColors[i].y);
                        writer.Write(pointColors[i].z);
                    }
                }
                writer.Close();
                Debug.Log("Finished saving cached file: " + outputFile);
            } // cache
        }

        public override void InitDX11Buffers()
        {
            // base.InitDX11Buffers();

            // cannot init 0 size, so create dummy data if its 0
            if (totalPoints == 0)
            {
                totalPoints = 1;
                points = new Vector3[1];
                if (readRGB == true)
                {
                    pointColors = new Vector3[1];
                }
            }

            // clear old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            if (bufferPoints != null)
            {
                bufferPoints.Dispose();
            }

            bufferPoints = new ComputeBuffer(totalPoints, 12);
            bufferPoints.SetData(points);
            cloudMaterial.SetBuffer("buf_Points", bufferPoints);

            if (readRGB == true)
            {
                if (bufferColors != null)
                {
                    bufferColors.Dispose();
                }
                bufferColors = new ComputeBuffer(totalPoints, 12);
                bufferColors.SetData(pointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }

            if (forceDepthBufferPass == true)
            {
                // not needed now, since using same material
                //cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
        }

        public virtual void InitDX11Buffers_pcd()
        {
            // base.InitDX11Buffers();

            // cannot init 0 size, so create dummy data if its 0
            if (totalPoints == 0)
            {
                totalPoints = 1;
                points = new Vector3[1];
#if true // customization!!!!!
                if (readRGB == true || readIntensity == true)
#else
                if (readRGB == true)
#endif
                {
                    pointColors = new Vector3[1];
                }
            }

            // clear old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            if (bufferPoints != null)
            {
                bufferPoints.Dispose();
            }

            bufferPoints = new ComputeBuffer(totalPoints, 12);
            bufferPoints.SetData(points);
            cloudMaterial.SetBuffer("buf_Points", bufferPoints);

#if true // customization!!!!!
            if (readRGB == true || readIntensity == true)
#else
            if (readRGB == true)
#endif
            {
                if (bufferColors != null)
                {
                    bufferColors.Dispose();
                }
                bufferColors = new ComputeBuffer(totalPoints, 12);
                bufferColors.SetData(pointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }

            if (forceDepthBufferPass == true)
            {
                // not needed now, since using same material
                //cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
        }

        /*
        protected void Invoke_OnLoadingComplete(string fullPath)
        {
            if (OnLoadingComplete != null)
            {
                OnLoadingComplete(fullPath);
            }
        }
        */

        protected override void OnLoadingCompleteCallBack(System.Object a)
        {
            // base.OnLoadingCompleteCallBack(a);

            // if (OnLoadingComplete != null) OnLoadingComplete((string)a);
            Invoke_OnLoadingComplete((string)a);

            if (useCommandBuffer == true)
            {
                commandBuffer.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, totalPoints, 1);
            }

            if (forceDepthBufferPass == true)
            {
                commandBufferDepth.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, totalPoints, 1);
            }
        }

        protected override IEnumerator FindClosestPointBrute(Vector2 mousePos) // in screen pixel coordinates
        {
            isSearchingPoint = true;

            int? closestIndex = null;
            float closestDistance = Mathf.Infinity;
            // Camera cam = Camera.main;

            var offsetPixels = new Vector2(0, 32); // search area in pixels
            var farPointUp = cam.ScreenPointToRay(mousePos + offsetPixels).GetPoint(999);
            var farPointDown = cam.ScreenPointToRay(mousePos - offsetPixels).GetPoint(999);

            offsetPixels = new Vector2(32, 0);  // search area in pixels
            var farPointLeft = cam.ScreenPointToRay(mousePos - offsetPixels).GetPoint(999);
            var farPointRight = cam.ScreenPointToRay(mousePos + offsetPixels).GetPoint(999);

            var screenPos = Vector2.zero;
            float distance = Mathf.Infinity;

            // build filtering planes
            Plane forwardPlane = new Plane(cam.transform.forward, cam.transform.position);
            Plane bottomLeft = new Plane(cam.transform.position, farPointDown, farPointLeft);
            Plane topLeft = new Plane(cam.transform.position, farPointLeft, farPointUp);
            Plane topRight = new Plane(cam.transform.position, farPointUp, farPointRight);
            Plane bottomRight = new Plane(cam.transform.position, farPointRight, farPointDown);

            /*
            // display search area
            Debug.DrawLine(farPointDown,farPointLeft, Color.magenta,20);
            Debug.DrawLine(farPointLeft,farPointUp, Color.magenta,20);
            Debug.DrawLine(farPointUp,farPointRight,Color.magenta,20);
            Debug.DrawLine(farPointRight,farPointDown,Color.magenta,20);
            */

            // check all points, until find close enough hit
            var pixelThreshold = 3; // if distance is this or less, just select it

            for (int i = 0, len = points.Length; i < len; i++)
            {
                if (i % maxIterationsPerFrame == 0)
                {
                    // Pause our work here, and continue finding on the next frame
                    yield return null;
                }

                if (!forwardPlane.GetSide(points[i])) continue;
                if (topRight.GetSide(points[i])) continue;
                if (bottomRight.GetSide(points[i])) continue;
                if (bottomLeft.GetSide(points[i])) continue;
                if (topLeft.GetSide(points[i])) continue;

                screenPos = cam.WorldToScreenPoint(points[i]);

                distance = Vector2.Distance(mousePos, screenPos);
                //distance = DistanceApprox(mousePos, screenPos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    if (distance <= pixelThreshold) break; // early exit on close enough hit
                }
            }

            if (closestIndex != null)
            {
                /*
                if (PointWasSelected != null) 
                    PointWasSelected(points[(int)closestIndex]); // fire event if have listeners
                */
                Invoke_PointWasSelected(points[(int)closestIndex]);

                Debug.Log("PointIndex:" + ((int)closestIndex) + " pos:" + points[(int)closestIndex]);
            }
            else
            {
                Debug.Log("No point selected..");
            }
            isSearchingPoint = false;
        }

        /*
        protected void Invoke_PointWasSelected(Vector3 pointPos)
        {
            if (PointWasSelected != null)
                PointWasSelected(pointPos); // fire event if have listeners
        }
        */
#endif // #if !UNITY_SAMSUNGTV && !UNITY_WEBGL
            } // class
    } // namespace

