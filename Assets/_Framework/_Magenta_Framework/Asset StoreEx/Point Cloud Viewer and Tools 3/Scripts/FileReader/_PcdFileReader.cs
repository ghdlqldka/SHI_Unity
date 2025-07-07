// Point Cloud Binary Viewer DX11
// reads custom binary file and displays it with dx11 shader
// http://unitycoder.com

using UnityEngine;
using System.IO;
using System.Threading;
using PointCloudHelpers;
using Debug = UnityEngine.Debug;
using pointcloudviewer.tools;
using static pointcloudviewer.tools.MainThread;
using pointcloudviewer.helpers;
using PointCloudViewer.Structs;
using System.Globalization;

namespace pointcloudviewer.binaryviewer
{
    //[ExecuteInEditMode] // ** You can enable this, if you want to see DX11 cloud inside editor, without playmode NOTE: only works with V1 .bin and with threading disabled **
    [System.Serializable]
    public class _PcdFileReader : _PointCloudFileReader
    {
        private static string LOG_FORMAT = "<color=#27B7AD><b>[_PcdFileReader]</b></color> {0}";

        public static float[] LUT255 = new float[] { 0f, 0.00392156862745098f, 0.00784313725490196f, 0.011764705882352941f, 0.01568627450980392f, 0.0196078431372549f, 0.023529411764705882f, 0.027450980392156862f, 0.03137254901960784f, 0.03529411764705882f, 0.0392156862745098f, 0.043137254901960784f, 0.047058823529411764f, 0.050980392156862744f, 0.054901960784313725f, 0.058823529411764705f, 0.06274509803921569f, 0.06666666666666667f, 0.07058823529411765f, 0.07450980392156863f, 0.0784313725490196f, 0.08235294117647059f, 0.08627450980392157f, 0.09019607843137255f, 0.09411764705882353f, 0.09803921568627451f, 0.10196078431372549f, 0.10588235294117647f, 0.10980392156862745f, 0.11372549019607843f, 0.11764705882352941f, 0.12156862745098039f, 0.12549019607843137f, 0.12941176470588237f, 0.13333333333333333f, 0.13725490196078433f, 0.1411764705882353f, 0.1450980392156863f, 0.14901960784313725f, 0.15294117647058825f, 0.1568627450980392f, 0.1607843137254902f, 0.16470588235294117f, 0.16862745098039217f, 0.17254901960784313f, 0.17647058823529413f, 0.1803921568627451f, 0.1843137254901961f, 0.18823529411764706f, 0.19215686274509805f, 0.19607843137254902f, 0.2f, 0.20392156862745098f, 0.20784313725490197f, 0.21176470588235294f, 0.21568627450980393f, 0.2196078431372549f, 0.2235294117647059f, 0.22745098039215686f, 0.23137254901960785f, 0.23529411764705882f, 0.23921568627450981f, 0.24313725490196078f, 0.24705882352941178f, 0.25098039215686274f, 0.2549019607843137f, 0.25882352941176473f, 0.2627450980392157f, 0.26666666666666666f, 0.27058823529411763f, 0.27450980392156865f, 0.2784313725490196f, 0.2823529411764706f, 0.28627450980392155f, 0.2901960784313726f, 0.29411764705882354f, 0.2980392156862745f, 0.30196078431372547f, 0.3058823529411765f, 0.30980392156862746f, 0.3137254901960784f, 0.3176470588235294f, 0.3215686274509804f, 0.3254901960784314f, 0.32941176470588235f, 0.3333333333333333f, 0.33725490196078434f, 0.3411764705882353f, 0.34509803921568627f, 0.34901960784313724f, 0.35294117647058826f, 0.3568627450980392f, 0.3607843137254902f, 0.36470588235294116f, 0.3686274509803922f, 0.37254901960784315f, 0.3764705882352941f, 0.3803921568627451f, 0.3843137254901961f, 0.38823529411764707f, 0.39215686274509803f, 0.396078431372549f, 0.4f, 0.403921568627451f, 0.40784313725490196f, 0.4117647058823529f, 0.41568627450980394f, 0.4196078431372549f, 0.4235294117647059f, 0.42745098039215684f, 0.43137254901960786f, 0.43529411764705883f, 0.4392156862745098f, 0.44313725490196076f, 0.4470588235294118f, 0.45098039215686275f, 0.4549019607843137f, 0.4588235294117647f, 0.4627450980392157f, 0.4666666666666667f, 0.47058823529411764f, 0.4745098039215686f, 0.47843137254901963f, 0.4823529411764706f, 0.48627450980392156f, 0.49019607843137253f, 0.49411764705882355f, 0.4980392156862745f, 0.5019607843137255f, 0.5058823529411764f, 0.5098039215686274f, 0.5137254901960784f, 0.5176470588235295f, 0.5215686274509804f, 0.5254901960784314f, 0.5294117647058824f, 0.5333333333333333f, 0.5372549019607843f, 0.5411764705882353f, 0.5450980392156862f, 0.5490196078431373f, 0.5529411764705883f, 0.5568627450980392f, 0.5607843137254902f, 0.5647058823529412f, 0.5686274509803921f, 0.5725490196078431f, 0.5764705882352941f, 0.5803921568627451f, 0.5843137254901961f, 0.5882352941176471f, 0.592156862745098f, 0.596078431372549f, 0.6f, 0.6039215686274509f, 0.6078431372549019f, 0.611764705882353f, 0.615686274509804f, 0.6196078431372549f, 0.6235294117647059f, 0.6274509803921569f, 0.6313725490196078f, 0.6352941176470588f, 0.6392156862745098f, 0.6431372549019608f, 0.6470588235294118f, 0.6509803921568628f, 0.6549019607843137f, 0.6588235294117647f, 0.6627450980392157f, 0.6666666666666666f, 0.6705882352941176f, 0.6745098039215687f, 0.6784313725490196f, 0.6823529411764706f, 0.6862745098039216f, 0.6901960784313725f, 0.6941176470588235f, 0.6980392156862745f, 0.7019607843137254f, 0.7058823529411765f, 0.7098039215686275f, 0.7137254901960784f, 0.7176470588235294f, 0.7215686274509804f, 0.7254901960784313f, 0.7294117647058823f, 0.7333333333333333f, 0.7372549019607844f, 0.7411764705882353f, 0.7450980392156863f, 0.7490196078431373f, 0.7529411764705882f, 0.7568627450980392f, 0.7607843137254902f, 0.7647058823529411f, 0.7686274509803922f, 0.7725490196078432f, 0.7764705882352941f, 0.7803921568627451f, 0.7843137254901961f, 0.788235294117647f, 0.792156862745098f, 0.796078431372549f, 0.8f, 0.803921568627451f, 0.807843137254902f, 0.8117647058823529f, 0.8156862745098039f, 0.8196078431372549f, 0.8235294117647058f, 0.8274509803921568f, 0.8313725490196079f, 0.8352941176470589f, 0.8392156862745098f, 0.8431372549019608f, 0.8470588235294118f, 0.8509803921568627f, 0.8549019607843137f, 0.8588235294117647f, 0.8627450980392157f, 0.8666666666666667f, 0.8705882352941177f, 0.8745098039215686f, 0.8784313725490196f, 0.8823529411764706f, 0.8862745098039215f, 0.8901960784313725f, 0.8941176470588236f, 0.8980392156862745f, 0.9019607843137255f, 0.9058823529411765f, 0.9098039215686274f, 0.9137254901960784f, 0.9176470588235294f, 0.9215686274509803f, 0.9254901960784314f, 0.9294117647058824f, 0.9333333333333333f, 0.9372549019607843f, 0.9411764705882353f, 0.9450980392156862f, 0.9490196078431372f, 0.9529411764705882f, 0.9568627450980393f, 0.9607843137254902f, 0.9647058823529412f, 0.9686274509803922f, 0.9725490196078431f, 0.9764705882352941f, 0.9803921568627451f, 0.984313725490196f, 0.9882352941176471f, 0.9921568627450981f, 0.996078431372549f, 1f };

        public static bool useDX11 = true;

        // protected bool readRGB = true;
        protected bool readIntensity = true;
        public bool ReadIntensity
        {
            get
            {
                return readIntensity;
            }
        }
        protected Vector3 manualOffset = Vector3.zero;
        // protected bool hasLoadedPointCloud = false;
        // protected bool isSearchingPoint = false;
        protected int maxIterationsPerFrame = 256000;

        protected class PcdOptions : Options
        {
            //
        }

        public _PcdFileReader(string filePath) : base()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");

            _isNewFormat = false;

            assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            this.filePath = filePath;
            this.extension = ".pcd";
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "this.filePath : <b>" + this.filePath + "</b>");
            string _extension = Path.GetExtension(this.filePath).ToLower();
            Debug.Assert(extension == _extension);

            this.DEBUG_showDebug = true;
#endif
        }

        public virtual void Read(Func OnInitDX11BuffersBack, Function OnReadCompleteCallBack)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>Read()</color>");

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

            Debug.LogFormat(LOG_FORMAT, "this.filePath : <b>" + this.filePath + "</b>, _fileFullPath : <b>" + _fileFullPath + "</b>");

            PcdOptions options = new PcdOptions();
            options.fileFullPath = _fileFullPath;
            options.OnInitDX11BuffersBack = OnInitDX11BuffersBack;
            options.OnReadCompleteCallBack = OnReadCompleteCallBack;
            ParameterizedThreadStart start = new ParameterizedThreadStart(ReadPointCloudThreaded);

            importerThread = new Thread(start);
            importerThread.IsBackground = true;
            importerThread.Start(options);
            // TODO need to close previous thread before loading new!
        }

        protected virtual void ReadPointCloudThreaded(System.Object param)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ReadPointCloudThreaded()");

            bool autoOffsetNearZero = false;
            bool useUnitScale = false;
            float unitScale = 0.001f;
            bool useManualOffset = false;
            bool flipYZ = true;

            PcdOptions options = param as PcdOptions;
#if false ///
            // cleanup old buffers
            if (useDX11 == true)
            {
                ReleaseDX11Buffers();
            }
#endif

            Debug.LogFormat(LOG_FORMAT, "fullPath : <b>" + options.fileFullPath + "</b>");

            IsLoading = true;

            // hasLoadedPointCloud = false;
            Debug.Assert(IsReady == false);
            IsReady = false;

            long lines = 0;

            // get initial data (so can check if data is ok)
            using (StreamReader streamReader = new StreamReader(File.OpenRead(options.fileFullPath)))
            {
                double _x = 0, _y = 0, _z = 0;
                float _r = 0, _g = 0, _b = 0; //,nx=0,ny=0,nz=0;; // init vals
                string line = null;
                string[] row = null;

                // PeekHeaderData headerCheck;
                _PcdHeaderData headerCheck = new _PcdHeaderData();
                headerCheck.x = 0;
                headerCheck.y = 0;
                headerCheck.z = 0;
                headerCheck.linesRead = 0;

                long masterPointCount = 0;
                // headerCheck = _PeekHeader.PeekHeaderPCD(streamReader, ref readRGB, ref masterPointCount);
                headerCheck = _PeekHeader.PeekHeaderPCD(streamReader, ref containsRGB, ref readIntensity, ref masterPointCount);
                Debug.LogWarningFormat(LOG_FORMAT, "containsRGB : <b><color=red>" + containsRGB +
                    "</color></b>, readIntensity : <b><color=red>" + readIntensity + 
                    "</color></b>" + ", masterPointCount : <b>" + masterPointCount + "</b>");

                if (headerCheck.readSuccess == false)
                {
                    Debug.Assert(false);
                    streamReader.Close();
                    return;
                }

                if (autoOffsetNearZero == true)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<b>autoOffsetNearZero : <color=red>" + autoOffsetNearZero + "</color></b>");
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

                if (containsRGB == true || readIntensity == true)
                {
                    pointColors = new Vector3[masterPointCount];
                }

                _totalPoints = (int)masterPointCount;

                progressCounter = 0;

                int skippedRows = 0;
                long rowCount = 0;
                bool haveMoreToRead = true;

                float minX = Mathf.Infinity;
                float minY = Mathf.Infinity;
                float minZ = Mathf.Infinity;
                float maxX = Mathf.NegativeInfinity;
                float maxY = Mathf.NegativeInfinity;
                float maxZ = Mathf.NegativeInfinity;

                // process all points
                while (haveMoreToRead == true && AbortThread == false)
                {
                    if (progressCounter > 256000)
                    {
                        // TODO: add runtime progressbar
                        //EditorUtility.DisplayProgressBar(appName, "Converting point cloud to binary file", rowCount / (float)lines);
                        progressCounter = 0;
                    }

                    progressCounter++;

                    line = streamReader.ReadLine();
                    // Debug.LogFormat(LOG_FORMAT, "line : " + line);

                    if (line != null)// && line.Length > 9)
                    {
                        // trim duplicate spaces
                        line = line.Replace("   ", " ").Replace("  ", " ").Trim();
                        row = line.Split(' ');

                        if (row.Length > 2)
                        {
                            _x = double.Parse(row[0], CultureInfo.InvariantCulture);
                            _y = double.Parse(row[1], CultureInfo.InvariantCulture);
                            _z = double.Parse(row[2], CultureInfo.InvariantCulture);

                            if (containsRGB == true)
                            {
                                // TODO: need to check both rgb formats
                                if (row.Length == 4)
                                {
                                    int rgb = (int)decimal.Parse(row[3], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
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
                                    Color _color = IntensityToJet(intensity);
                                    _r = _color.r;
                                    _g = _color.g;
                                    _b = _color.b;
                                }
                            }
#endif

                            // scaling enabled
                            if (useUnitScale == true)
                            {
                                _x *= unitScale;
                                _y *= unitScale;
                                _z *= unitScale;
                            }

                            // manual offset enabled
                            if (autoOffsetNearZero == true || useManualOffset == true) // NOTE: can use only one at a time
                            {
                                _x -= manualOffset.x;
                                _y -= manualOffset.y;
                                _z -= manualOffset.z;
                            }

                            // if flip
                            if (flipYZ == true)
                            {
                                _Points[rowCount].Set((float)_x, (float)_z, (float)_y);
                            }
                            else
                            {
                                Debug.LogWarningFormat(LOG_FORMAT, "flipYZ : false");
                                _Points[rowCount].Set((float)_x, (float)_y, (float)_z);
                            }

                            // if have color data
                            if (containsRGB == true || readIntensity == true)
                            {
                                pointColors[rowCount].Set(_r, _g, _b);
                            }

                            rowCount++;
                        }
                        else
                        {
                            Debug.LogWarningFormat(LOG_FORMAT, "row.Length <= 2");
                            // if row length
                            skippedRows++;
                        }
                    }
                    else
                    { // if linelen
                        Debug.LogFormat(LOG_FORMAT, "skip line");
                        skippedRows++;
                    }

                    // get bounds
                    if (_x < minX)
                        minX = (float)_x;
                    else if (_x > maxX)
                        maxX = (float)_x;

                    //((x < minX) ? ref minX : ref maxX) = x; // c#7
                    if (_y < minY)
                        minY = (float)_y;
                    else if (_y > maxY)
                        maxY = (float)_y;

                    if (_z < minZ)
                        minZ = (float)_z;
                    else if (_z > maxZ)
                        maxZ = (float)_z;

                    // reached end or enough points
                    if (streamReader.EndOfStream == true || rowCount >= masterPointCount)
                    {
                        if (skippedRows > 0)
                        {
                            Debug.LogWarningFormat(LOG_FORMAT, "Parser skipped " + skippedRows + " rows (wrong length or bad data)");
                        }

                        if (rowCount < masterPointCount) // error, file ended too early, not enough points
                        {
                            Debug.LogWarningFormat(LOG_FORMAT, "File does not contain enough points, fixing point count to " + rowCount + " (expected : " + masterPointCount + ")");
                        }
                        haveMoreToRead = false;
                    }
                } // while loop reading file

                cloudBounds = new Bounds(new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f), new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ)));

                // done reading, display it now
                IsLoading = false;

#if true //
                // cannot init 0 size, so create dummy data if its 0
                if (_totalPoints == 0)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "########################");
                    _totalPoints = 1;
                    points = new Vector3[1];
#if true // customization!!!!!
                    if (containsRGB == true || readIntensity == true)
#else
                    if (containsRGB == true)
#endif
                    {
                        pointColors = new Vector3[1];
                    }
                }
#endif

                // if (useDX11 == true)
                {
                    IsInitializingBuffers = true;
                    _MainThread.Call(options.OnInitDX11BuffersBack);
                    // Thread.Sleep(10); // wait for buffers to be ready_
                    // Debug.LogFormat(LOG_FORMAT, "IsInitializingBuffers : " + IsInitializingBuffers + ", AbortThread : " + AbortThread);
                    while (IsInitializingBuffers == true && AbortThread == false)
                    {
                        Thread.Sleep(1);
                    }
                }
                _MainThread.Call(options.OnReadCompleteCallBack, this.filePath);

                // hasLoadedPointCloud = true;
                IsReady = true;

            } // using reader

            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@ Finished loading. @@@@@@@@@@@@@@@@@");

        } // ReadPointCloudThreaded

#if true // by ChatGPT
        /// <summary>
        /// Converts an intensity value (0–255) into a Jet colormap RGB color.
        /// </summary>
        /// <param name="intensity">The intensity value, usually in the range 0 to 255.</param>
        /// <returns>Color mapped in the Jet colormap (blue → green → red).</returns>
        public static Color IntensityToJet(float intensity)
        {
            // Normalize the intensity to the range [0, 1]
            intensity = Mathf.Clamp01(intensity / 255f);

            // Jet colormap calculation:
            // Blue at 0.0, Green at 0.5, Red at 1.0
            float r = Mathf.Clamp01(1.5f - Mathf.Abs(4f * intensity - 3f));
            float g = Mathf.Clamp01(1.5f - Mathf.Abs(4f * intensity - 2f));
            float b = Mathf.Clamp01(1.5f - Mathf.Abs(4f * intensity - 1f));

            return new Color(r, g, b);
        }
#endif

    } // class
} // namespace