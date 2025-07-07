// Point Cloud Binary Viewer DX11
// reads custom binary file and displays it with dx11 shader
// http://unitycoder.com

#if !UNITY_WEBPLAYER && !UNITY_SAMSUNGTV

using UnityEngine;
using System.IO;
using PointCloudHelpers;
using UnityEngine.Rendering;
using System;
using Debug = UnityEngine.Debug;
using pointcloudviewer.tools;
using PointCloudViewer;
using System.Collections;

namespace pointcloudviewer.binaryviewer
{
    //[ExecuteInEditMode] // ** You can enable this, if you want to see DX11 cloud inside editor, without playmode NOTE: only works with V1 .bin and with threading disabled **
    public class _PointCloudViewerDX11 : PointCloudViewerDX11
    {
        private static string LOG_FORMAT = "<color=#03940F><b>[_PointCloudViewerDX11]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected _PointCloudFileReader _reader = new _PointCloudFileReader();

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

        // protected Bounds cloudBounds;
        public Bounds CloudBounds
        {
            get
            {
                return _reader.CloudBounds;
            }
        }

        // public bool containsRGB = false;
        public bool ContainsRGB
        {
            get
            {
                return _reader.ContainsRGB;
            }
        }

        // public int totalPoints = 0;
        public int TotalPoints
        {
            get
            {
                return _reader.TotalPoints;
            }
        }

        // public int totalMaxPoints = 0; // new variable to keep total maximum point count
        public int TotalMaxPoints
        {
            get
            {
                return ((_UcpcFileReader)_reader).TotalMaxPoints;
            }
        }

        // public Vector3[] points; // actual point cloud points array
        public Vector3[] _Points
        {
            get
            {
                return _reader._Points;
            }
        }

        // public Vector3[] pointColors;
        public Vector3[] _PointColors
        {
            get
            {
                return _reader._PointColors;
            }
        }

        public bool _IsReady
        {
            get
            {
                return (_reader.IsReady);
            }
        }

        protected override void Awake()
        {
            // applicationStreamingAssetsPath = Application.streamingAssetsPath;
            _assetsPath = Application.dataPath + 
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
            Debug.LogFormat(LOG_FORMAT, "_assetsPath : <b>" + _assetsPath + "</b>");

            useThreading = true; // forcelly set!!!!
            showDebug = true; // forcelly set!!!!!
            instantiateMaterial = true; // forcelly set!!!!!

            displayPoints = true; // forcelly set!!!!
            renderOnlyMainCam = false; // forcelly set!!!!!

            byteArrayToFloats = null; // Do not used!!!!!

            isReady = false; // Do not use!!!!!!

            // [Tooltip("Shuffle points to use dynamic resolution adjuster *Cannot use if ReadCachedFile is enabled. **V2+ formats are usually already randomized")]
            randomizeArray = false; // forcelly set!!!!!!
            packColors = false; // forcelly set!!!!!!

#if UNITY_EDITOR
            commandBufferToSceneCamera = false;
#endif
        }

        protected override void OnDestroy()
        {
            ReleaseDX11Buffers();
            points = new Vector3[0];
            pointColors = new Vector3[0];

            if (isLoading == true)
            {
                abortReaderThread = true;
            }

            if (_reader != null)
            {
                if (_reader.IsLoading == true)
                {
                    _reader.AbortThread = true;
                }
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), loadAtStart : <b><color=yellow>" + loadAtStart + "</color></b>");

            transformPos = this.transform.position;

            // cam = Camera.main;
            cam = _PointCloudManager.Instance._Camera;
            Debug.Assert(cam != null);

            // validate URP and HDRP
            if (useURPCustomPass == true && useHDRPCustomPass == true)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "URP and URP custom passes cannot be used at the same time, please disable one of them");
            }

            if (useURPCustomPass == true)
            {
                // NOTE doesnt actually check if URP is used, but if has any srp
                RenderPipelineAsset currentPipelineAsset = GraphicsSettings.defaultRenderPipeline;
                if (currentPipelineAsset == null)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "URP Custom Pass is enabled, but URP is not installed. Please disable URP Custom Pass or install URP");
                }
            }

            if (useHDRPCustomPass == true)
            {
                // NOTE doesnt actually check if URP is used, but if has any srp
                RenderPipelineAsset currentPipelineAsset = GraphicsSettings.defaultRenderPipeline;
                if (currentPipelineAsset == null)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "HDRP Custom Pass is enabled, but HDRP is not installed. Please disable HDRP Custom Pass or install HDRP");
                }
            }

            if (useCommandBuffer == true)
            {
                commandBuffer = new CommandBuffer();
                cam.AddCommandBuffer(camDrawPass, commandBuffer);

#if false //
#if UNITY_EDITOR
                if (commandBufferToSceneCamera == true)
                {
                    UnityEditor.SceneView.GetAllSceneCameras()[0].AddCommandBuffer(camDrawPass, commandBuffer);
                }
#endif
#endif
            }

            if (forceDepthBufferPass == true)
            {
                depthMaterial = cloudMaterial;
                commandBufferDepth = new CommandBuffer();
                cam.AddCommandBuffer(camDepthPass, commandBufferDepth);
            }

            /*
            if (cam == null)
            { 
                Debug.LogErrorFormat(LOG_FORMAT, "Camera main is missing..", gameObject);
            }
            */

            // create material clone, so can view multiple clouds
            // if (instantiateMaterial == true)
            {
                cloudMaterial = new Material(cloudMaterial);
            }

            // if (useThreading == true)
            {
                // check if MainThread script exists in scene, its required only for threading
                FixMainThreadHelper();
            }

            // cache pos
            transformPos = this.transform.position;

            if (loadAtStart == true)
            {
                // if (useThreading == true)
                {
                    abortReaderThread = false;
                    CallReadPointCloudThreaded(fileName);
                }
                /*
                else
                {
                    ReadPointCloud();
                }
                */
            }
        }

        public override void ReadPointCloud()
        {
            throw new System.NotSupportedException("");
        }

        protected override void Update()
        {
            //if (isLoading == true || haveError == true) return;

            if (applyTranslationMatrix == true)
            {
                translationMatrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
                cloudMaterial.SetMatrix("_TranslationMatrix", translationMatrix);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (_reader.Extension == ".pcd" && isSearchingPoint == false)
                {
                    StartCoroutine(FindClosestPointBrute(Input.mousePosition));
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "isSearchingPoint : " + isSearchingPoint);
                }
            }
        }

        // drawing mainloop, for drawing the points
        //void OnPostRender() // < works also, BUT must have this script attached to Camera
        public override void OnRenderObject()
        {
            /*
            // optional: if you only want to render to specific camera, use next line
            if (renderOnlyMainCam == true && Camera.current.CompareTag("MainCamera") == false)
            {
                return;
            }
            */

            // dont display while loading, it slows down with huge clouds
            if (isLoading == true || /*displayPoints == false ||*/ useCommandBuffer == true || useURPCustomPass == true || useHDRPCustomPass == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                return;
            }

            cloudMaterial.SetPass(0);

#if UNITY_2019_1_OR_NEWER
            Graphics.DrawProceduralNow(MeshTopology.Points, _reader.TotalPoints);
#else
            Graphics.DrawProcedural(MeshTopology.Points, _reader.TotalPoints);
#endif
        }

        public delegate void PointSelected(Vector3 pointPos);
        public event PointSelected PointWasSelected;
        protected virtual IEnumerator FindClosestPointBrute(Vector2 mousePos) // in screen pixel coordinates
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

            Vector2 screenPos = Vector2.zero;
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

            for (int i = 0, len = _reader._Points.Length; i < len; i++)
            {
                if (i % maxIterationsPerFrame == 0)
                {
                    // Pause our work here, and continue finding on the next frame
                    yield return null;
                }

                if (forwardPlane.GetSide(_reader._Points[i]) == false)
                    continue;
                if (topRight.GetSide(_reader._Points[i]))
                    continue;
                if (bottomRight.GetSide(_reader._Points[i]))
                    continue;
                if (bottomLeft.GetSide(_reader._Points[i]))
                    continue;
                if (topLeft.GetSide(_reader._Points[i]))
                    continue;

                screenPos = cam.WorldToScreenPoint(_reader._Points[i]);

                distance = Vector2.Distance(mousePos, screenPos);
                //distance = DistanceApprox(mousePos, screenPos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    if (distance <= pixelThreshold)
                    {
                        break; // early exit on close enough hit
                    }
                }
            }

            if (closestIndex != null)
            {
                if (PointWasSelected != null)
                {
                    PointWasSelected(_reader._Points[(int)closestIndex]); // fire event if have listeners
                }
                Debug.LogWarningFormat(LOG_FORMAT, "<color=red>PointIndex : <b>" + ((int)closestIndex) + 
                    "</b></color>, pos : <color=yellow>" + _reader._Points[(int)closestIndex] + "</color>");
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "No point selected..");
            }
            isSearchingPoint = false;
        }

        public override void FixMainThreadHelper()
        {
            if (GameObject.Find("#_MainThreadHelper") == null || _MainThread.instanceCount == 0)
            {
                var go = new GameObject();
                go.name = "#_MainThreadHelper";
                go.AddComponent<_MainThread>();
            }
        }

        public override void CallReadPointCloudThreaded(string filePath)
        {
            Debug.LogFormat(LOG_FORMAT, "CallReadPointCloudThreaded(), filePath : <b><color=yellow>" + filePath + "</color></b>");

            // isReady = false;

            // Only allowed (.bin || .ucpc)
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension != ".bin" && extension != ".ucpc" && extension != ".pcd")
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File is not v1 or v2 file (.bin or .ucpc or .pcd extension is required) : " + extension);
                return;
            }

            /*
            if (isLoadingNewData == false)
            {
                Debug.LogFormat(LOG_FORMAT, "(Viewer) Reading threaded pointcloud file: " + filePath);
            }
            */

            Debug.Assert(randomizeArray == false);
            if (extension == ".ucpc")
            {
                _reader = new _UcpcFileReader(filePath);
                ((_UcpcFileReader)_reader).Read(readWholeCloud, initialPointsToRead, randomizeArray, _InitDX11Buffers, _OnReadUcpcCompleteCallBack);
            }
            else if (extension == ".bin")
            {
                _reader = new _BinFileReader(filePath, transformPos);
                ((_BinFileReader)_reader).Read(randomizeArray, reSaveBinFile, _InitDX11Buffers, _OnReadBinCompleteCallBack);
            }
            else if (extension == ".pcd")
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=red>$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$</color>");
                Destroy(_PointCloudManager.Instance); // <===========================================

                // cleanup old buffers
                if (_PcdFileReader.useDX11 == true)
                {
                    ReleaseDX11Buffers();
                }

                _reader = new _PcdFileReader(filePath);
                ((_PcdFileReader)_reader).Read(_InitDX11Buffers_pcd, _OnLoadingCompleteCallBack_pcd);
            }
            else
            {
                Debug.Assert(false);
            }

            // TODO need to close previous thread before loading new!
        }

        protected int maxIterationsPerFrame = 256000;
        protected bool isSearchingPoint = false;

        public override void AdjustVisiblePointsAmount(int offsetAmount)
        {
            Debug.LogFormat(LOG_FORMAT, "AdjustVisiblePointsAmount(), offsetAmount : " + offsetAmount);
            // base.AdjustVisiblePointsAmount(offsetAmount);

            if (isNewFormat == true) // .ucpc
            {
                // TODO wait for load to finish (probably increased last click already)
                if (isLoadingNewData == true)
                {
                    return;
                }

                totalPoints += offsetAmount;
                if (TotalPoints < 0)
                {
                    totalPoints = 0;
                }
                else if (TotalPoints > TotalMaxPoints)
                {
                    totalPoints = TotalMaxPoints;
                }

                if (TotalPoints > initialPointsToRead)
                {
                    //Debug.Log("Need to load..");

                    // load new data, with given point cloud count, then need to set point size here instead of dynamicres script?
                    // TODO later, incrementally load data instead of all data again
                    //CallReadPointCloudThreaded(string fullPath);
                    initialPointsToRead = TotalPoints;// + offsetAmount;
                                                      //Debug.Log("initialPointsToRead=" + initialPointsToRead);
                    isLoadingNewData = true;
                    CallReadPointCloudThreaded(fileName);
                }
            }
            else // old format
            {
                totalPoints += offsetAmount;
                if (TotalPoints < 0)
                {
                    totalPoints = 0;
                }
                else
                {
                    if (TotalPoints > TotalMaxPoints)
                    {
                        totalPoints = TotalMaxPoints;
                    }
                }
            }
        }

        // set amount of points to draw
        public override void SetVisiblePointCount(int amount)
        {
            if (isNewFormat == true) // .ucpc
            {
                // TODO wait for load to finish (probably increased last click already)
                if (isLoadingNewData == true)
                {
                    return;
                }

                totalPoints = amount;
                if (TotalPoints < 0)
                {
                    totalPoints = 0;
                }
                else if (TotalPoints > TotalMaxPoints)
                {
                    totalPoints = TotalMaxPoints;
                }

                if (TotalPoints > initialPointsToRead)
                {
                    //Debug.Log("Need to load..");

                    // load new data, with given point cloud count, then need to set point size here instead of dynamicres script?
                    // TODO later, incrementally load data instead of all data again
                    //CallReadPointCloudThreaded(string fullPath);
                    initialPointsToRead = TotalPoints;// + offsetAmount;
                    isLoadingNewData = true;
                    CallReadPointCloudThreaded(fileName);
                }
            }
            else // old format
            {
                totalPoints = amount;
                if (TotalPoints < 0)
                {
                    totalPoints = 0;
                }
                else
                {
                    if (TotalPoints > TotalMaxPoints)
                    {
                        totalPoints = TotalMaxPoints;
                    }
                }
            }
        }

        protected virtual void _InitDX11Buffers()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>_InitDX11Buffers()</color></b>");

#if false //
            isInitializingBuffers = true;
            // cannot init 0 size, so create dummy data if its 0
            if (TotalPoints == 0)
            {
                totalPoints = 1;
                points = new Vector3[1];
                if (ContainsRGB == true)
                {
                    pointColors = new Vector3[1];
                }
            }
#endif

            // clear old buffers
            ReleaseDX11Buffers();

            if (bufferPoints != null)
            {
                bufferPoints.Dispose();
            }

#if false //
            bool packColors2 = false;
            if (packColors2 == true) //  not working
            {
                // broken
            }
            else if (packColors == true) // packer2
            {
                PackedPoint[] points2 = new PackedPoint[_Points.Length];
                int len = _Points.Length;
                for (int i = 0; i < len; i++)
                {
                    var p = new PackedPoint();
                    // pack red and x
                    var xx = PointCloudMath.SuperPacker(_PointColors[i].x * 0.98f, _Points[i].x);
                    // pack green and y
                    var yy = PointCloudMath.SuperPacker(_PointColors[i].y * 0.98f, _Points[i].y);
                    // pack blue and z
                    var zz = PointCloudMath.SuperPacker(_PointColors[i].z * 0.98f, _Points[i].z);
                    p.x = xx;
                    p.y = yy;
                    p.z = zz;
                    points2[i] = p;
                }
                bufferPoints = new ComputeBuffer(TotalPoints, 12);
                bufferPoints.SetData(points2);
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
            else // original
#endif
            {
                bufferPoints = new ComputeBuffer(_reader.TotalPoints, 12);
                bufferPoints.SetData(_reader._Points);
                // TODO use mat2int
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
                if (_reader.ContainsRGB == true)
                {
                    if (bufferColors != null)
                    {
                        bufferColors.Dispose();
                    }
                    bufferColors = new ComputeBuffer(_reader.TotalPoints, 12);
                    bufferColors.SetData(_reader._PointColors);
                    cloudMaterial.SetBuffer("buf_Colors", bufferColors);
                }
            }

            if (forceDepthBufferPass == true)
            {
                depthMaterial.SetBuffer("buf_Points", bufferPoints);
            }

            _reader.IsInitializingBuffers = false;
        }

        protected virtual void _InitDX11Buffers_pcd()
        {
            Debug.LogFormat(LOG_FORMAT, "_InitDX11Buffers_pcd(), totalPoints : " + _reader.TotalPoints +
                ", containsRGB : <b>" + _reader.ContainsRGB + "</b>, readIntensity : <b>" + ((_PcdFileReader)_reader).ReadIntensity + "</b>");

            // base.InitDX11Buffers();

#if false //
            // cannot init 0 size, so create dummy data if its 0
            if (TotalPoints == 0)
            {
                TotalPoints = 1;
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

            // clear old buffers
            if (_PcdFileReader.useDX11 == true)
            {
                ReleaseDX11Buffers();
            }

            if (bufferPoints != null)
            {
                bufferPoints.Dispose();
            }

            Debug.LogFormat(LOG_FORMAT, "_reader.TotalPoints : " + _reader.TotalPoints);
            bufferPoints = new ComputeBuffer(_reader.TotalPoints, 12);
            bufferPoints.SetData(_reader._Points);
            cloudMaterial.SetBuffer("buf_Points", bufferPoints);

#if true // customization!!!!!
            if (_reader.ContainsRGB == true || ((_PcdFileReader)_reader).ReadIntensity == true)
#else
            if (containsRGB == true)
#endif
            {
                if (bufferColors != null)
                {
                    bufferColors.Dispose();
                }
                bufferColors = new ComputeBuffer(_reader.TotalPoints, 12);
                bufferColors.SetData(_reader._PointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }

            if (forceDepthBufferPass == true)
            {
                // not needed now, since using same material
                //cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }

            _reader.IsInitializingBuffers = false;
        }


        protected virtual void _OnReadBinCompleteCallBack(System.Object param)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "=====> <b><color=yellow>_OnReadBinCompleteCallBack()</color></b> <=====");

            // base.OnLoadingCompleteCallBack(a);

            // TEST
            // PointCloudTools.DrawBounds(GetBounds(), 99);
            PointCloudTools.DrawBounds(_reader.CloudBounds, 99);

            //Debug.Log("OnLoadingCompleteCallBack");
            /*
            if (OnLoadingComplete != null)
                OnLoadingComplete((string)a);
            */
            Invoke_OnLoadingComplete((string)param);

            if (useCommandBuffer == true)
            {
                commandBuffer.DrawProcedural(Matrix4x4identity, cloudMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 0);
                // transform.localToWorldMatrix
            }

            if (forceDepthBufferPass == true)
            {
                commandBufferDepth.DrawProcedural(Matrix4x4identity, depthMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 0);
            }

            isLoading = false;
            // isReady = true;
            Debug.LogFormat(LOG_FORMAT, "===============> Finished loading <===============");
        }

        protected virtual void _OnReadUcpcCompleteCallBack(System.Object param)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "=====> <b><color=yellow>_OnReadUcpcCompleteCallBack()</color></b> <=====");

            // base.OnLoadingCompleteCallBack(a);

            // TEST
            // PointCloudTools.DrawBounds(GetBounds(), 99);
            PointCloudTools.DrawBounds(_reader.CloudBounds, 99);

            //Debug.Log("OnLoadingCompleteCallBack");
            /*
            if (OnLoadingComplete != null)
                OnLoadingComplete((string)a);
            */
            Invoke_OnLoadingComplete((string)param);

            if (useCommandBuffer == true)
            {
                commandBuffer.DrawProcedural(Matrix4x4identity, cloudMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 0);
                // transform.localToWorldMatrix
            }

            if (forceDepthBufferPass == true)
            {
                commandBufferDepth.DrawProcedural(Matrix4x4identity, depthMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 0);
            }

            isLoading = false;
            // isReady = true;
            Debug.LogFormat(LOG_FORMAT, "===============> Finished loading <===============");
        }

        protected virtual void _OnLoadingCompleteCallBack_pcd(System.Object a)
        {
            Debug.LogFormat(LOG_FORMAT, "_OnLoadingCompleteCallBack_pcd()");
            // base.OnLoadingCompleteCallBack(a);

            // if (OnLoadingComplete != null) OnLoadingComplete((string)a);
            Invoke_OnLoadingComplete((string)a);

            if (useCommandBuffer == true)
            {
                commandBuffer.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 1);
            }

            if (forceDepthBufferPass == true)
            {
                commandBufferDepth.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, _reader.TotalPoints, 1);
            }
        }


        public override bool IsReady()
        {
            throw new NotSupportedException("");
            // return isReady;
        }

        public override void ToggleCloud(bool state)
        {
            throw new System.NotSupportedException("");
        }

        public override void UpdatePointData()
        {
#if true //
            throw new System.NotSupportedException("");
#else
            Debug.LogFormat(LOG_FORMAT, "UpdatePointData()");

            if (_Points.Length == bufferPoints.count)
            {
                // same length as earlier
                bufferPoints.SetData(_Points);
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
            else
            {
                // new data is different sized array, need to redo it
                TotalPoints = _Points.Length;
                TotalMaxPoints = TotalPoints;
                bufferPoints.Dispose();
                // NOTE: not for packed data..
                //Debug.Log("new ComputeBuffer");
                bufferPoints = new ComputeBuffer(TotalPoints, 12);
                bufferPoints.SetData(_Points);
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
#endif
        }

        public virtual void UpdatePointData(Vector3[] points)
        {
            Debug.LogFormat(LOG_FORMAT, "UpdatePointData()");

            this.points = points;

            if (_Points.Length == bufferPoints.count)
            {
                // same length as earlier
                bufferPoints.SetData(_Points);
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
            else
            {
                // new data is different sized array, need to redo it
                totalPoints = _Points.Length;
                totalMaxPoints = TotalPoints;
                bufferPoints.Dispose();
                // NOTE: not for packed data..
                //Debug.Log("new ComputeBuffer");
                bufferPoints = new ComputeBuffer(TotalPoints, 12);
                bufferPoints.SetData(_Points);
                cloudMaterial.SetBuffer("buf_Points", bufferPoints);
            }
        }

        // can use this to set new point colors data
        public override void UpdateColorData()
        {
            Debug.LogFormat(LOG_FORMAT, "UpdateColorData()");

            if (_PointColors.Length == bufferColors.count)
            {
                // same length as earlier
                bufferColors.SetData(_PointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }
            else
            {
                // new data is different sized array, need to redo it
                totalPoints = _PointColors.Length;
                totalMaxPoints = TotalPoints;

                bufferColors.Dispose();
                bufferColors = new ComputeBuffer(TotalPoints, 12);
                bufferColors.SetData(_PointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }
        }

        public virtual void UpdateColorData(Vector3[] pointColors)
        {
            Debug.LogFormat(LOG_FORMAT, "UpdateColorData()");

            this.pointColors = pointColors;

            if (_PointColors.Length == bufferColors.count)
            {
                // same length as earlier
                bufferColors.SetData(_PointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }
            else
            {
                // new data is different sized array, need to redo it
                totalPoints = _PointColors.Length;
                totalMaxPoints = TotalPoints;

                bufferColors.Dispose();
                bufferColors = new ComputeBuffer(TotalPoints, 12);
                bufferColors.SetData(_PointColors);
                cloudMaterial.SetBuffer("buf_Colors", bufferColors);
            }
        }

        public override Bounds GetBounds()
        {
            throw new System.NotSupportedException("");
            // return cloudBounds;
        }
    } // class
} // namespace

#endif