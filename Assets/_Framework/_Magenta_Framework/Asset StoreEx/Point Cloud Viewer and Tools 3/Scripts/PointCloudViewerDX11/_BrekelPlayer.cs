// Brekel Animated Point Cloud Viewer (.bin format, exported from Brekel) https://brekel.com/brekel-pointcloud-v3/
// http://unitycoder.com

#if !UNITY_WEBPLAYER && !UNITY_SAMSUNGTV

using UnityEngine;
using System.IO;
using PointCloudHelpers;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using pointcloudviewer.tools;
using UnityEngine.Playables;

namespace pointcloudviewer.brekel
{
    public class _BrekelPlayer : BrekelPlayer
    {
        private static string LOG_FORMAT = "<color=#DAEA1E><b>[_BrekelPlayer]</b></color> {0}";

        [SerializeField]
        protected Camera _camera;

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

        protected override void Awake()
        {
            // applicationStreamingAssetsPath = Application.streamingAssetsPath;
            _assetsPath = Application.dataPath +
                "/_Framework/_Magenta_Framework/Asset StoreEx/Point Cloud Viewer and Tools 3/_StreamingAssets";

            Debug.LogFormat(LOG_FORMAT, "Awake()");
            Debug.LogFormat(LOG_FORMAT, "_assetsPath : " + _assetsPath);
        }

        // init
        protected override void Start()
        {
            FixMainThreadHelper();

            transformPos = transform.position;
            // cam = Camera.main;
            cam = _camera;
            Debug.Assert(cam != null);

            if (useCommandBuffer == true)
            {
                commandBuffer = new CommandBuffer();
                cam.AddCommandBuffer(camDrawPass, commandBuffer);

#if UNITY_EDITOR
                if (commandBufferToSceneCamera == true)
                {
                    // UnityEditor.SceneView.GetAllSceneCameras()[0].AddCommandBuffer(camDrawPass, commandBuffer);
                    Camera[] cameras = UnityEditor.SceneView.GetAllSceneCameras();
                    cameras[0].AddCommandBuffer(camDrawPass, commandBuffer);
                }
#endif
            }

            if (forceDepthBufferPass == true)
            {
                Debug.LogFormat(LOG_FORMAT, "%%%%%%%%%%%%%%%");
                depthMaterial = cloudMaterial;
                commandBufferDepth = new CommandBuffer();
                cam.AddCommandBuffer(camDepthPass, commandBufferDepth);
            }

            /*
            if (cam == null)
            {
                Debug.LogError("Camera main is missing..", gameObject);
            }
            */

            // create material clone, so can view multiple clouds
            if (instantiateMaterial == true)
            {
                cloudMaterial = new Material(cloudMaterial);
            }

            if (loadAtStart == true)
            {
                //ReadAnimatedPointCloud(); // add option for non-threading?

                ParameterizedThreadStart start = new ParameterizedThreadStart(ReadAnimatedPointCloud);
                fileStreamerThread = new Thread(start);
                fileStreamerThread.IsBackground = true;
                fileStreamerThread.Start(fileName);
            }
        }

        protected override void Update()
        {
            if (isLoading == true || haveError == true && useLargeFileStreaming == false)
            {
                Debug.LogFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@@@");
                return;
            }

            if (Input.GetKeyDown(togglePlay))
            {
                isPlaying = !isPlaying;
            }

            if (isPlaying == true && Time.time > nextFrameTimer)
            {
                nextFrameTimer = Time.time + playbackDelay;
                UpdateFrame();
                currentFrame = (++currentFrame) % numberOfFrames;
            }
            else // paused or waiting for framedelay
            {
                if (useLargeFileStreaming == false)
                {
                    if (Input.GetKeyDown(playPrevFrame))
                    {
                        currentFrame--;
                        if (currentFrame < 0)
                        {
                            currentFrame = numberOfFrames - 1;
                        }
                        UpdateFrame();
                    }

                    if (Input.GetKeyDown(playNextFrame))
                    {
                        currentFrame = (++currentFrame) % numberOfFrames;
                        UpdateFrame();
                    }
                }
            }

            if (applyTranslationMatrix == true)
            {
                Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                cloudMaterial.SetMatrix("_TranslationMatrix", m);
            }

        }

        protected override void ReadAnimatedPointCloud(System.Object objfileName)
        {
            isLoading = true;

            string fileName = (string)objfileName;
            // if not full path, use streaming assets
            if (Path.IsPathRooted(fileName) == false)
            {
                fileName = Path.Combine(applicationStreamingAssetsPath, fileName);
            }

            if (PointCloudTools.CheckIfFileExists(fileName) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "File not found:" + fileName);
                haveError = true;
                isLoading = false;
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "Reading pointcloud from: " + fileName);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            isReady = false;
            haveError = false;
            int totalCounter = 0;

            long fileSize = new FileInfo(fileName).Length;
            var tempPoint = Vector3.zero;
            var tempColor = Vector3.zero;

            totalNumberOfPoints = 0;
            maxPointsPerFrame = 0;

            if (fileSize >= 2147483647 || useLargeFileStreaming == true)
            {
                Debug.LogFormat(LOG_FORMAT, "Starting large file streaming: " + PointCloudTools.HumanReadableFileSize(fileSize));
                isLoading = false;

                useLargeFileStreaming = true;

                using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                using (BinaryReader binReader = new BinaryReader(fs))
                {
                    // parse header data
                    Int32 byteIndex = 0;
                    binaryVersion = binReader.ReadByte();
                    byteIndex += sizeOfByte;
                    if (binaryVersion != 3) 
                    { 
                        Debug.LogErrorFormat("For large Animated point cloud, header binaryVersion should have value (3), received=" + binaryVersion);
                        return;
                    }

                    numberOfFrames = binReader.ReadInt32();
                    byteIndex += sizeofInt32;
                    frameRate = binReader.ReadInt32();
                    byteIndex += sizeofInt32;
                    containsRGB = binReader.ReadBoolean();
                    byteIndex += sizeOfBool;

                    numberOfPointsPerFrame = new int[numberOfFrames];

                    // if (showDebug)
                    {
                        Debug.LogFormat(LOG_FORMAT, "(ReadAll) Animated file header: binaryVersion= " + binaryVersion + " numberOfFrames = " + numberOfFrames + " hasRGB=" + containsRGB);
                    }

                    // get each frame point count info
                    for (int i = 0; i < numberOfFrames; i++)
                    {
                        numberOfPointsPerFrame[i] = binReader.ReadInt32();
                        byteIndex += sizeofInt32;
                        if (numberOfPointsPerFrame[i] > maxPointsPerFrame)
                        {
                            maxPointsPerFrame = numberOfPointsPerFrame[i]; // largest value will be used as a fixed size for point array
                        }
                        totalNumberOfPoints += numberOfPointsPerFrame[i];
                    }

                    MAXPOINTCOUNT = maxPointsPerFrame;

                    if (useCommandBuffer || forceDepthBufferPass)
                    {
                        clearArray = new Vector3[MAXPOINTCOUNT];
                    }

                    isInitializingBuffers = true;
                    _MainThread.Call(InitBuffersForStreaming);

                    while (isInitializingBuffers == true && abortStreamerThread == false)
                    {
                        Thread.Sleep(100);
                    }

                    int headerSize = byteIndex;
                    int currentReadFrame = 0;
                    long currentPosition = byteIndex;

                    // read data loop
                    while (abortStreamerThread == false)
                    {
                        // if queue too full, wait
                        if (abortStreamerThread == false && pointQueueCount >= maxFrameBufferCount)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        int numberOfPointsThisFrame = numberOfPointsPerFrame[currentReadFrame];
                        int dataBytesSize = numberOfPointsThisFrame * 4 * 3;

                        // convert single frame, TODO init outside loop, for max amount, then slice? (can use nativearray later)
                        Vector3[] convertedPoints = new Vector3[numberOfPointsThisFrame];
                        GCHandle vectorPointer = GCHandle.Alloc(convertedPoints, GCHandleType.Pinned);
                        IntPtr pV = vectorPointer.AddrOfPinnedObject();
                        Marshal.Copy(binReader.ReadBytes(dataBytesSize), 0, pV, dataBytesSize);
                        byteIndex += dataBytesSize;
                        vectorPointer.Free();

                        // post to queue
                        pointQueue.Enqueue(convertedPoints);
                        if (containsRGB == true)
                        {
                            Vector3[] convertedColors = new Vector3[numberOfPointsThisFrame];
                            GCHandle colorPointer = GCHandle.Alloc(convertedColors, GCHandleType.Pinned);
                            IntPtr pV2 = colorPointer.AddrOfPinnedObject();
                            //Debug.Log("dataBytesSize="+ dataBytesSize+" / "+ dataBytesSize/4);
                            Marshal.Copy(binReader.ReadBytes(dataBytesSize), 0, pV2, dataBytesSize);
                            byteIndex += dataBytesSize;
                            colorPointer.Free();
                            colorQueue.Enqueue(convertedColors);
                        }
                        pointQueueCount++;
                        currentReadFrame++;

                        // TODO add user rewind/pause

                        // loop from end-of-file
                        if (currentPosition >= fileSize || currentReadFrame >= numberOfFrames)
                        {
                            binReader.BaseStream.Seek(headerSize, 0);
                            currentPosition = headerSize;
                            currentReadFrame = 0;
                        }
                    } // loop data
                } // read file

                Debug.LogWarningFormat(LOG_FORMAT, "Closing streamer..");
                return;
            }
            else // can read with allbytes
            {
                var data = File.ReadAllBytes(fileName);

                // parse header data
                Int32 byteIndex = 0;
                binaryVersion = data[byteIndex];
                if (binaryVersion != 2 && binaryVersion != 3)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "For Animated point cloud, header binaryVersion should have value (2 or 3), received=" + binaryVersion);
                    return;
                }
                byteIndex += sizeof(Byte);
                numberOfFrames = (int)BitConverter.ToInt32(data, byteIndex);
                byteIndex += sizeofInt32;
                frameRate = System.BitConverter.ToInt32(data, byteIndex); // not used
                byteIndex += sizeofInt32;
                containsRGB = BitConverter.ToBoolean(data, byteIndex);
                byteIndex += sizeOfBool;

                numberOfPointsPerFrame = new int[numberOfFrames];

                // if (showDebug)
                {
                    Debug.LogFormat(LOG_FORMAT, "(ReadAll) Animated file header: numberOfFrames=" + numberOfFrames + " hasRGB=" + containsRGB);
                }

                // get each frame point count info
                for (int i = 0; i < numberOfFrames; i++)
                {
                    numberOfPointsPerFrame[i] = (int)BitConverter.ToInt32(data, byteIndex);
                    byteIndex += sizeofInt32;
                    if (numberOfPointsPerFrame[i] > maxPointsPerFrame)
                    {
                        maxPointsPerFrame = numberOfPointsPerFrame[i]; // largest value will be used as a fixed size for point array
                    }
                    totalNumberOfPoints += numberOfPointsPerFrame[i];
                }

                if (binaryVersion == 2)
                {
                    // get frame positions
                    frameBinaryPositionForEachFrame = new Int64[numberOfFrames];
                    for (int i = 0; i < numberOfFrames; i++)
                    {
                        frameBinaryPositionForEachFrame[i] = (Int64)BitConverter.ToInt64(data, byteIndex);
                        byteIndex += sizeofInt64;
                    }
                }

                // if (showDebug)
                {
                    Debug.LogFormat(LOG_FORMAT, "totalNumberOfPoints = " + totalNumberOfPoints);
                }
                // if (showDebug)
                {
                    Debug.LogFormat(LOG_FORMAT, "Maximum frame point count: " + maxPointsPerFrame);
                }

                // init playback arrays
                animatedPointArray = new Vector3[totalNumberOfPoints];
                if (containsRGB == true)
                {
                    animatedColorArray = new Vector3[totalNumberOfPoints];
                }

                animatedOffset = new int[numberOfFrames];
                if (containsRGB == true)
                {
                    pointColors = new Vector3[maxPointsPerFrame];
                }

                if (binaryVersion == 2)
                {
                    // parse points from data, TODO could just convert
                    for (int frame = 0; frame < numberOfFrames; frame++)
                    {
                        animatedOffset[frame] = totalCounter;
                        for (int i = 0; i < numberOfPointsPerFrame[frame]; i++)
                        {
                            tempPoint.x = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]) + transformPos.x;
                            byteIndex += sizeOfSingle;
                            tempPoint.y = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]) + transformPos.y;
                            byteIndex += sizeOfSingle;
                            tempPoint.z = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]) + transformPos.z;
                            byteIndex += sizeOfSingle;
                            animatedPointArray[totalCounter] = tempPoint;

                            if (containsRGB == true)
                            {
                                tempColor.x = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]);
                                byteIndex += sizeOfSingle;
                                tempColor.y = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]);
                                byteIndex += sizeOfSingle;
                                tempColor.z = PointCloudMath.BytesToFloat(data[byteIndex], data[byteIndex + 1], data[byteIndex + 2], data[byteIndex + 3]);
                                byteIndex += sizeOfSingle;
                                animatedColorArray[totalCounter] = tempColor;
                            }

                            totalCounter++;
                        }
                    }
                }
                else // animated format 3
                {
                    int pointIndex = 0;
                    int colorIndex = 0;

                    GCHandle vectorPointer = GCHandle.Alloc(animatedPointArray, GCHandleType.Pinned);
                    IntPtr pV = vectorPointer.AddrOfPinnedObject();

                    GCHandle colorPointer = GCHandle.Alloc(animatedColorArray, GCHandleType.Pinned);
                    IntPtr pV2 = colorPointer.AddrOfPinnedObject();

                    for (int frame = 0; frame < numberOfFrames; frame++)
                    {
                        int dataBytesSize = numberOfPointsPerFrame[frame] * 3 * 4;

                        // xyz
                        Marshal.Copy(data, byteIndex, pV + pointIndex, dataBytesSize);
                        byteIndex += dataBytesSize;
                        pointIndex += dataBytesSize;

                        // rgb
                        if (containsRGB == true)
                        {
                            Marshal.Copy(data, byteIndex, pV2 + colorIndex, dataBytesSize);
                            byteIndex += dataBytesSize;
                            colorIndex += dataBytesSize;
                        }

                        animatedOffset[frame] = totalCounter;
                        totalCounter += numberOfPointsPerFrame[frame];
                    }
                    vectorPointer.Free();
                    colorPointer.Free();
                } // v3
            } // allbytes

            // framebuffer is always max point count
            points = new Vector3[maxPointsPerFrame];

            Debug.LogFormat(LOG_FORMAT, "Finished loading animated point cloud. Frames=" + numberOfFrames + " Total points= " + PointCloudTools.HumanReadableCount(totalCounter));

            totalMaxPoints = maxPointsPerFrame;

            isInitializingBuffers = true;
            _MainThread.Call(InitDX11Buffers);
            while (isInitializingBuffers == true && abortStreamerThread == false)
            {
                Thread.Sleep(100);
            }

            isLoading = false;
            _MainThread.Call(OnLoadingCompleteCallBack, fileName);

            stopwatch.Stop();
            // if (showDebug)
            {
                Debug.LogFormat(LOG_FORMAT, "Loading time: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            stopwatch.Reset();
        } // ReadAnimatedPointcloud

        protected override void InitBuffersForStreaming()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "InitBuffersForStreaming()");

            //if (currentFramePointCount < 1) return;
            //if (useMeshRendering == true) return;

            if (bufferPoints != null)
            {
                bufferPoints.Dispose();
            }
            bufferPoints = new ComputeBuffer(MAXPOINTCOUNT, 12);

            if (bufferColors != null)
            {
                bufferColors.Dispose();
            }
            bufferColors = new ComputeBuffer(MAXPOINTCOUNT, 12);

            if (useCommandBuffer == true)
            {
                commandBuffer.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, MAXPOINTCOUNT, 1);
            }

            if (forceDepthBufferPass == true)
            {
                commandBufferDepth.DrawProcedural(Matrix4x4.identity, cloudMaterial, 0, MeshTopology.Points, MAXPOINTCOUNT, 1);
            }
            isInitializingBuffers = false;
        }

    } // class
} // namespace

#endif