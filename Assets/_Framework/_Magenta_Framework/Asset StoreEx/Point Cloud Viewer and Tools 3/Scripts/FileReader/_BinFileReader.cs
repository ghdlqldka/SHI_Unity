// Point Cloud Binary Viewer DX11
// reads custom binary file and displays it with dx11 shader
// http://unitycoder.com

using UnityEngine;
using System.IO;
using System.Threading;
using PointCloudHelpers;
using System;
using Debug = UnityEngine.Debug;
using pointcloudviewer.tools;
using static pointcloudviewer.tools.MainThread;

namespace pointcloudviewer.binaryviewer
{
    //[ExecuteInEditMode] // ** You can enable this, if you want to see DX11 cloud inside editor, without playmode NOTE: only works with V1 .bin and with threading disabled **
    [System.Serializable]
    public class _BinFileReader : _PointCloudFileReader
    {
        private static string LOG_FORMAT = "<color=#27B7AD><b>[_BinFileReader]</b></color> {0}";

        protected Vector3 transformPos;

        [ReadOnly]
        [SerializeField]
        public int totalMaxPoints = 0; // new variable to keep total maximum point count

        protected class BinOptions : Options
        {
            public bool reSaveBinFile;
        }

        public _BinFileReader(string filePath, Vector3 transformPos) : base()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!, filePath : <b>" + filePath + "</b>");

            _isNewFormat = false;

            assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            this.filePath = filePath;
            this.extension = ".bin";
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "this.filePath : " + this.filePath);
            string _extension = Path.GetExtension(this.filePath).ToLower();
            Debug.Assert(extension == _extension);
#endif

            this.transformPos = transformPos;
#if DEBUG
            this.DEBUG_showDebug = true;
#endif
        }

        public virtual void Read(bool randomizeArray, bool reSaveBinFile,
            Func OnInitDX11BuffersBack, Function OnReadCompleteCallBack)
        {
            Debug.LogFormat(LOG_FORMAT, "Read()");

#if false // a
            if (isLoadingNewData == false)
            {
                Debug.LogFormat(LOG_FORMAT, "(Viewer) Reading threaded pointcloud file: " + fullPath);
            }
#endif
            string _filePath = this.filePath;
            if (Path.IsPathRooted(_filePath) == false)
            {
                // Debug.LogWarningFormat(LOG_FORMAT, "Not Root!!!!");

                _filePath = Path.Combine(assetsPath, _filePath);
            }
            if (PointCloudTools.CheckIfFileExists(_filePath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + _filePath);
                Debug.Assert(false);
            }

            BinOptions options = new BinOptions();
            options.fileFullPath = _filePath;
            options.randomizeArray = randomizeArray;
            options.reSaveBinFile = reSaveBinFile;
            options.OnInitDX11BuffersBack = OnInitDX11BuffersBack;
            options.OnReadCompleteCallBack = OnReadCompleteCallBack;
            ParameterizedThreadStart start = new ParameterizedThreadStart(ReadPointCloudThreaded);
            
            importerThread = new Thread(start);
            importerThread.IsBackground = true;
            importerThread.Start(options);
            // TODO need to close previous thread before loading new!
        }

        protected virtual void ReadPointCloudThreaded(System.Object a)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ReadPointCloudThreaded()");

            // for testing loading times
#if DEBUG
            if (DEBUG_showDebug == true)
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
            }
#endif

            IsLoading = true;

            BinOptions options = a as BinOptions;
            string fileFullPath = options.fileFullPath;
            byte[] data;
            try
            {
                data = File.ReadAllBytes(fileFullPath);
            }
            catch
            {
                Debug.LogErrorFormat(LOG_FORMAT, fileFullPath + " cannot be opened with ReadAllBytes(), it might be too large >2gb. Try splitting your data into smaller parts (using external point cloud editing tools)");
                return;
            }

            System.Int32 byteIndex = 0;

            int binaryVersion = data[byteIndex];
            byteIndex += sizeof(System.Byte);

            // check format
            if (binaryVersion > 1)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File binaryVersion should have value (0-1). Was " + binaryVersion + " - Loading cancelled. " + ((binaryVersion == 2) ? "(2 is Animated Point Cloud, use BrekelViewer for that)" : ""));
                return;
            }

            int totalPoints = (int)System.BitConverter.ToInt32(data, byteIndex);
            _totalPoints = totalPoints;
            byteIndex += sizeofInt32;
            //Debug.Log(totalPoints);

            containsRGB = System.BitConverter.ToBoolean(data, byteIndex);
            byteIndex += sizeof(System.Boolean);

            // TEST
            //totalPoints = totalPoints * 2;

            points = new Vector3[totalPoints];

            // Debug.LogFormat(LOG_FORMAT, "Loading old v1/v2 format: " + totalPoints + " points..");

            float _x, _y, _z;
            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float minZ = Mathf.Infinity;
            float maxX = Mathf.NegativeInfinity;
            float maxY = Mathf.NegativeInfinity;
            float maxZ = Mathf.NegativeInfinity;

            if (containsRGB == true)
            {
                pointColors = new Vector3[totalPoints];
            }

            float[] byteArrayToFloats;
            byteArrayToFloats = new float[(data.Length - byteIndex) / 4];
            // BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);
            System.Buffer.BlockCopy(data, byteIndex, byteArrayToFloats, 0, data.Length - byteIndex);

            int dataIndex = 0;
            for (int i = 0; i < totalPoints; i++)
            {
                _x = byteArrayToFloats[dataIndex] + transformPos.x;
                dataIndex++;
                _y = byteArrayToFloats[dataIndex] + transformPos.y;
                dataIndex++;
                _z = byteArrayToFloats[dataIndex] + transformPos.z;
                dataIndex++;

                _Points[i].x = _x;
                _Points[i].y = _y;
                _Points[i].z = _z;

                // get bounds
                if (_x < minX)
                    minX = _x;
                else if (_x > maxX)
                    maxX = _x;

                //((x < minX) ? ref minX : ref maxX) = x; // c#7
                if (_y < minY)
                    minY = _y;
                else if (_y > maxY)
                    maxY = _y;

                if (_z < minZ)
                    minZ = _z;
                else if (_z > maxZ)
                    maxZ = _z;

                if (containsRGB == true)
                {
                    float _r, _g, _b;

                    _r = byteArrayToFloats[dataIndex];
                    dataIndex++;
                    _g = byteArrayToFloats[dataIndex];
                    dataIndex++;
                    _b = byteArrayToFloats[dataIndex];
                    dataIndex++;

                    pointColors[i].x = _r;
                    pointColors[i].y = _g;
                    pointColors[i].z = _b;
                }

                if (AbortThread == true)
                {
                    return;
                }
            } // for all points

            cloudBounds = new Bounds(new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f), new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ)));

            totalMaxPoints = totalPoints;

            // if randomize enabled, and didnt read from cache, then randomize
            Debug.Assert(options.randomizeArray == false);
            /*
            if (options.randomizeArray == true)
            {
                // if (showDebug) 
                {
                    Debug.LogFormat(LOG_FORMAT, "Randomizing cloud..");
                }
                PointCloudTools.Shuffle(new System.Random(), ref points, ref pointColors);
            }
            */

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
            // refresh buffers
            IsInitializingBuffers = true;
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

            if (options.reSaveBinFile == true)
            {
                // Debug.Assert(isNewFormat == false); // Not .ucpc
                /*
                if (isNewFormat == true)
                {
                    Debug.LogError("Cannot use reSaveBinFile with new V2 format");
                }
                else
                */
                {
                    string outputFile = options.fileFullPath; // Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + "-cached.bin";
                    Debug.LogFormat(LOG_FORMAT, "saving cached file: " + options.fileFullPath);

                    BinaryWriter writer = null;

                    try
                    {
                        writer = new BinaryWriter(File.Open(outputFile, FileMode.Create));

                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (writer == null)
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "Cannot output file: " + outputFile);
                        return;
                    }

                    writer.Write((byte)binaryVersion);
                    writer.Write((Int32)totalMaxPoints);
                    writer.Write(containsRGB);

                    for (int i = 0, length = _Points.Length; i < length; i++)
                    {
                        writer.Write(_Points[i].x);
                        writer.Write(_Points[i].y);
                        writer.Write(_Points[i].z);
                        if (containsRGB == true)
                        {
                            writer.Write(pointColors[i].x);
                            writer.Write(pointColors[i].y);
                            writer.Write(pointColors[i].z);
                        }
                    }
                    writer.Close();
                    Debug.LogFormat(LOG_FORMAT, "Finished saving cached file: " + outputFile);
                }
            } // cache


            IsLoading = false;
            _MainThread.Call(options.OnReadCompleteCallBack, this.filePath);

            // data = null;

            IsReady = true;

#if DEBUG
            if (DEBUG_showDebug == true)
            {
                stopwatch.Stop();
                Debug.LogFormat(LOG_FORMAT, "Timer: " + stopwatch.ElapsedMilliseconds + "ms");
                stopwatch.Reset();
            }
#endif
        }
    } // class
}