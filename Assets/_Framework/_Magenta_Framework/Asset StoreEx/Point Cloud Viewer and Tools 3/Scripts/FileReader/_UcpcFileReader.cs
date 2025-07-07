// Point Cloud Binary Viewer DX11
// reads custom binary file and displays it with dx11 shader
// http://unitycoder.com

using UnityEngine;
using System.IO;
using System.Threading;
using PointCloudHelpers;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using pointcloudviewer.tools;
using static pointcloudviewer.tools.MainThread;

namespace pointcloudviewer.binaryviewer
{
    //[ExecuteInEditMode] // ** You can enable this, if you want to see DX11 cloud inside editor, without playmode NOTE: only works with V1 .bin and with threading disabled **
    [System.Serializable]
    public class _UcpcFileReader : _PointCloudFileReader
    {
        private static string LOG_FORMAT = "<color=#27B7AD><b>[_UcpcFileReader]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected int totalMaxPoints = 0; // new variable to keep total maximum point count
        public int TotalMaxPoints
        {
            get
            {
                return totalMaxPoints;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected bool isLoadingNewData = false;

        protected class UcpcOptions : Options
        {
            public bool readWholeCloud = true;
            public int initialPointsToRead = 10000;
        }

        public _UcpcFileReader(string filePath) : base()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");

            _isNewFormat = true;

            assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            this.filePath = filePath;
            this.extension = ".ucpc";
#if DEBUG
            string _extension = Path.GetExtension(this.filePath).ToLower();
            Debug.Assert(extension == _extension);
            Debug.LogFormat(LOG_FORMAT, "this.filePath : <b>" + this.filePath + "</b>");

            this.DEBUG_showDebug = true;
#endif
        }

        public virtual void Read(bool readWholeCloud, int initialPointsToRead, bool randomizeArray,
            Func OnInitDX11BuffersBack, Function OnReadCompleteCallBack)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>Read()</color>, readWholeCloud : <b><color=red>" + 
                readWholeCloud + "</color></b>, initialPointsToRead : <b>" + initialPointsToRead + "</b>");

            string _fileFullPath = this.filePath;
            if (Path.IsPathRooted(_fileFullPath) == false)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Not Root!!!!");

                _fileFullPath = Path.Combine(assetsPath, _fileFullPath);
            }
            if (PointCloudTools.CheckIfFileExists(_fileFullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + _fileFullPath);
                Debug.Assert(false);
            }

            Debug.LogFormat(LOG_FORMAT, "this.filePath : <b>" + this.filePath + "</b>");
            Debug.LogFormat(LOG_FORMAT, "_fileFullPath : <b>" + _fileFullPath + "</b>");

            UcpcOptions options = new UcpcOptions();
            options.fileFullPath = _fileFullPath;
            options.randomizeArray = randomizeArray;
            options.readWholeCloud = readWholeCloud;
            options.initialPointsToRead = initialPointsToRead;
            options.OnInitDX11BuffersBack = OnInitDX11BuffersBack;
            options.OnReadCompleteCallBack = OnReadCompleteCallBack;
            ParameterizedThreadStart start = new ParameterizedThreadStart(ReadPointCloudThreadedNew);

            importerThread = new Thread(start);
            importerThread.IsBackground = true;
            importerThread.Start(options);
            // TODO need to close previous thread before loading new!

#if false //
            IsReady = false;

            if (Path.IsPathRooted(fullPath) == false)
            {
                Debug.LogFormat(LOG_FORMAT, "Not Root!!!!");

                fullPath = Path.Combine(assetsPath, fullPath);
            }

            // TEMP, needed later in loader, should pass instead
            fileName = fullPath;
            if (PointCloudTools.CheckIfFileExists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + fullPath);
                return;
            }

            // Only allowed (.bin || .ucpc)
            string extension = Path.GetExtension(fullPath).ToLower();
            if (extension != ".bin" && extension != ".ucpc")
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File is not v1 or v2 file (.bin or .ucpc extension is required) : " + Path.GetExtension(fullPath).ToLower());
                return;
            }
#endif
        }

        public virtual void ReadPointCloudThreadedNew(System.Object param)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ReadPointCloudThreaded<color=red>New</color>()");

#if false ///
            Debug.Assert(isNewFormat == true); // .ucpc
#endif

#if DEBUG
            if (DEBUG_showDebug == true)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
#endif

            IsLoading = true;

            byte[] headerdata = null;
            byte[] dataPoints = null;
            byte[] dataColors = null;
            int totalPoints;

            UcpcOptions options = param as UcpcOptions;
            try
            {
                // load header
                // load x amount of points and colors
                using (FileStream fs = File.Open(options.fileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BufferedStream bs = new BufferedStream(fs))
                using (BinaryReader binaryReader = new BinaryReader(bs))
                {
                    int headerSizeTemp = 34;
                    headerdata = new byte[headerSizeTemp];
                    headerdata = binaryReader.ReadBytes(headerSizeTemp);
                    int byteIndexTemp = 4 + 1 + 1;

                    totalPoints = (int)System.BitConverter.ToInt32(headerdata, byteIndexTemp);
                    long totalMaxPointsTemp = totalPoints;

                    Debug.LogFormat(LOG_FORMAT, "(Header) totalPoints: <b><color=red>" + totalPoints + "</color></b>");
                    byteIndexTemp += sizeof(System.Int32);

                    if (options.readWholeCloud == true)
                    {
                        options.initialPointsToRead = totalPoints;
                    }
                    else
                    {
                        totalPoints = Mathf.Clamp(options.initialPointsToRead, 0, totalPoints);
                    }
                    //Debug.Log("initialPointsToRead="+ initialPointsToRead);

                    int pointsChunkSize = totalPoints * (4 + 4 + 4);

                    dataPoints = binaryReader.ReadBytes(pointsChunkSize);

                    // jump to colors
                    binaryReader.BaseStream.Flush();
                    binaryReader.BaseStream.Position = (long)(totalMaxPointsTemp * (4 + 4 + 4) + headerdata.Length);

                    dataColors = binaryReader.ReadBytes(pointsChunkSize);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogErrorFormat(LOG_FORMAT, options.fileFullPath + " cannot be opened, its probably too large.. Test with [ ] Load Whole Cloud disabled, set initial load count (max 178m), or try splitting your data into smaller parts (using external point cloud editing tools)");
                return;
            }

            System.Int32 byteIndex = 0;

#if DEBUG
            // magic
            byte[] magic = new byte[] { headerdata[byteIndex++], headerdata[byteIndex++], headerdata[byteIndex++], headerdata[byteIndex++] };
            // if (showDebug)
            {
                Debug.LogFormat(LOG_FORMAT, "<b>magic=<color=red>" + System.Text.Encoding.ASCII.GetString(magic) + "</color></b>");
            }
#endif

            int binaryVersion = headerdata[byteIndex];
            byteIndex += sizeof(System.Byte);
            // if (showDebug)
            {
                Debug.LogFormat(LOG_FORMAT, "<b>binaryVersion=<color=red>" + binaryVersion + "</color></b>");
            }

            // check format
            if (binaryVersion != 2 && binaryVersion != 3)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File binaryVersion should have value (2 *regular or 3 *packed). Was " + binaryVersion + " - Loading cancelled.");
                return;
            }

            containsRGB = System.BitConverter.ToBoolean(headerdata, byteIndex);
            byteIndex += sizeof(System.Boolean);

            if (containsRGB == false)
            {
#if false ///
                Debug.Assert(isNewFormat == true); // .ucpc
#endif
                if (/*isNewFormat == true &&*/ binaryVersion != 3)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "v2 format requires RGB data - loading cancelled. Check that [x] Read RGB values is enabled from converter window.");
                    return;
                }
                else
                {
                    if (binaryVersion != 3)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "No RGB data in the file, cloud will be black..");
                    }
                }
            }

            // if (showDebug)
            {
                Debug.LogFormat(LOG_FORMAT, "<b>containsRGB=<color=red>" + containsRGB + "</color></b>");
            }

            totalPoints = (int)System.BitConverter.ToInt32(headerdata, byteIndex);
            totalMaxPoints = totalPoints;
            byteIndex += sizeof(System.Int32);

            // TEST load initially less points
            totalPoints = (int)Mathf.Clamp(options.initialPointsToRead, 0, totalPoints);
            _totalPoints = totalPoints;

            // bounds
            float minX = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);
            float minY = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);
            float minZ = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);
            float maxX = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);
            float maxY = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);
            float maxZ = System.BitConverter.ToSingle(headerdata, byteIndex);
            byteIndex += sizeof(System.Single);

            cloudBounds = new Bounds(new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f), new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ)));

            points = new Vector3[totalPoints];
            // if (showDebug)
            {
                Debug.LogFormat(LOG_FORMAT, "<b>cloudBounds=<color=yellow>" + cloudBounds + "</color></b>");
            }

            if (cloudBounds.extents.magnitude == 0)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Cloud bounds is 0! something wrong with header data, or its only 0's");
            }

            if (/*showDebug &&*/ isLoadingNewData == false)
            {
                Debug.LogFormat(LOG_FORMAT, "Loading new (V2) format: <b>" + totalPoints + "</b> points..");
            }

            GCHandle vectorPointer = GCHandle.Alloc(_Points, GCHandleType.Pinned);
            IntPtr pV = vectorPointer.AddrOfPinnedObject();
            Marshal.Copy(dataPoints, 0, pV, totalPoints * 4 * 3);
            vectorPointer.Free();

            if (containsRGB == true)
            {
                pointColors = new Vector3[totalPoints];
                GCHandle vectorPointer2 = GCHandle.Alloc(pointColors, GCHandleType.Pinned);
                IntPtr pV2 = vectorPointer2.AddrOfPinnedObject();
                Marshal.Copy(dataColors, 0, pV2, totalPoints * 4 * 3);
                vectorPointer2.Free();
            }

            // if randomize enabled, and didnt read from cache, then randomize
            if (options.randomizeArray == true)
            {
                // if (showDebug)
                {
                    Debug.LogFormat(LOG_FORMAT, "Randomizing cloud..");
                }
                PointCloudTools.Shuffle(new System.Random(), ref points, ref pointColors);
            }

            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++
            if (totalPoints == 0)
            {
                totalPoints = 1;
                points = new Vector3[1];
                if (containsRGB == true)
                {
                    pointColors = new Vector3[1];
                }
            }
            IsInitializingBuffers = true;
            // refresh buffers
            _MainThread.Call(options.OnInitDX11BuffersBack);
            while (IsInitializingBuffers == true && AbortThread == false)
            {
                Thread.Sleep(1);
            }
            // ------------------------------------------------------

            // NOTE: disabled this, was it needed?
            //UnityLibrary.MainThread.Call(UpdatePointData);
            //if (containsRGB == true) UnityLibrary.MainThread.Call(UpdateColorData);

            // if caching, save as bin (except if already read from cache)
            // TODO dont save if no changes to data
            // TODO move to separate method, so can call save anytime, if modify or remove points manually  

#if true //
            // [Header("V1 Format Only")]
            // [Tooltip("Create Cached file. Save .bin file again (to include randomizing or other changes to data during load *Not supported for new the V2 format)")]
            // public bool reSaveBinFile = false;
            // Debug.Assert(reSaveBinFile == false);
#else
            if (options.reSaveBinFile == true)
            {
                Debug.Assert(isNewFormat == true); // .ucpc
                if (isNewFormat == true)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Cannot use reSaveBinFile with new V2 format");
                }
                else
                {
                    string outputFile = options.fileFullPath;
                    Debug.LogFormat(LOG_FORMAT, "saving " + options.fileFullPath);

                    BinaryWriter writer = null;

                    try
                    {
                        writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (writer == null)
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "Cannot output file: " + outputFile);
                        return;
                    }

                    writer.Write((byte)binaryVersion);
                    writer.Write((System.Int32)totalMaxPoints);
                    writer.Write(containsRGB);

                    for (int i = 0, length = points.Length; i < length; i++)
                    {
                        writer.Write(points[i].x);
                        writer.Write(points[i].y);
                        writer.Write(points[i].z);
                        if (containsRGB == true)
                        {
                            writer.Write(pointColors[i].x);
                            writer.Write(pointColors[i].y);
                            writer.Write(pointColors[i].z);
                        }
                    }
                    writer.Close();
                    Debug.LogFormat(LOG_FORMAT, "Finished saving cached file: " + outputFile);
                } // cache
            }
#endif

            IsLoading = false;

            _MainThread.Call(options.OnReadCompleteCallBack, this.filePath);

            //data = null;
            IsReady = true;

            if (isLoadingNewData == false)
            {
                Debug.LogFormat(LOG_FORMAT, "Finished Loading: <b>" + options.initialPointsToRead + "</b> / <b>" + totalMaxPoints + "</b>");
            }

#if DEBUG
            if (DEBUG_showDebug == true)
            {
                stopwatch.Stop();
                //Debug.Log("Timer: " + stopwatch.ElapsedMilliseconds + "ms");
                // 2243ms for 25m
                // 3528ms for 50m > 2900ms (no .set)
                // 380ms for 50m (new format)
                stopwatch.Reset();
            }
#endif

            isLoadingNewData = false;

        } // ReadPointCloudThreaded

    } // class
} // namespace