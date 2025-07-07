// point cloud manager (currently used for point picking and loading new clouds manually)
// unitycoder.com

using GK;
using PointCloudHelpers;
using pointcloudviewer.binaryviewer;
using pointcloudviewer.tools;
using PointCloudViewer.Structs;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PointCloudViewer
{
    public class _PointCloudManager : PointCloudManager
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[_PointCloudManager]</b></color> {0}";

        [Space(10)]
        [SerializeField]
        protected Camera _camera;
        public Camera _Camera
        {
            get
            {
                return _camera;
            }
        }

        public static _PointCloudManager Instance
        {
            get
            {
                return instance as _PointCloudManager;
            }
            protected set
            {
                instance = value;
            }
        }

        // public List<PointCloudViewerDX11> viewers = new List<PointCloudViewerDX11>();
        public List<PointCloudViewerDX11> _Viewers
        {
            get
            {
                return viewers;
            }
        }

        // public List<Cloud> clouds; // all clouds
        [ReadOnly]
        [SerializeField]
        protected List<_Cloud> cloudList; // all clouds
        public List<_Cloud> _CloudList
        {
            get
            {
                return cloudList;
            }
            protected set
            {
                cloudList = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            Debug.Assert(_camera != null);

            if (Instance == null)
            {
                Instance = this;

                clouds = null; // Do not use!!!!!!!

                if (_MainThread.instanceCount == 0)
                {
                    if (_Viewers != null)
                    {
                        for (int i = 0; i < _Viewers.Count; i++)
                        {
                            if (_Viewers[i] != null)
                            {
                                ((_PointCloudViewerDX11)_Viewers[i]).FixMainThreadHelper();
                            }
                        }
                    }
                }

                _CloudList = new List<_Cloud>();

                // wait for loading complete event (for automatic registration)
                if (_Viewers != null)
                {
                    for (int i = 0; i < _Viewers.Count; i++)
                    {
                        if (_Viewers[i] != null)
                        {
                            // viewers[i].OnLoadingComplete -= CloudIsReady;
                            // ((PointCloudViewerDX11)_Viewers[i]).OnLoadingComplete -= OnLoadingComplete;

                            // viewers[i].OnLoadingComplete += CloudIsReady;
                            ((_PointCloudViewerDX11)_Viewers[i]).OnLoadingComplete += OnLoadingComplete;
                        }
                    }
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "PointCloudManager: No viewers..");
                }
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "=======> OnDestroy() <=======");

            if (Instance != this)
            {
                return;
            }

            // base.OnDestroy();
            if (pointPickingThread != null && pointPickingThread.IsAlive == true)
            {
                pointPickingThread.Abort();
            }

            if (_Viewers != null)
            {
                for (int i = 0; i < _Viewers.Count; i++)
                {
                    if (_Viewers[i] != null)
                    {
                        // viewers[i].OnLoadingComplete -= CloudIsReady;
                        ((_PointCloudViewerDX11)_Viewers[i]).OnLoadingComplete -= OnLoadingComplete;
                    }
                }
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "PointCloudManager: No viewers..");
            }

            Instance = null;
        }

        public override void FindClosestPoint(object rawRay)
        {
            // base.FindClosestPoint(rawRay);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int nearestIndex = -1;
            float nearestPointRayDist = Mathf.Infinity;
            int viewerIndex = -1; // which viewer has this point data

            Ray ray = (Ray)rawRay;
            Vector3 rayDirection = ray.direction;
            Vector3 rayOrigin = ray.origin;
            Vector3 rayEnd = rayOrigin + rayDirection * maxPickDistance;
            Vector3 point;

            // check all clouds
            int cloudsLen = _CloudList.Count;
            for (int cloudIndex = 0; cloudIndex < cloudsLen; cloudIndex++)
            {
                // check all nodes from each cloud
                int nodesLen = _CloudList[cloudIndex].nodes.Length;
                for (int nodeIndex = 0; nodeIndex < nodesLen; nodeIndex++)
                {
                    _Cloud _cloud = _CloudList[cloudIndex];
                    NodeBox _nodeBox = _cloud.nodes[nodeIndex];
                    // check if this box intersects ray, or that camera is inside node
                    if (_nodeBox.bounds.IntersectRay(ray) == true)
                    {
                        // then check all points from that node
                        int nodePointLen = _nodeBox.points.Count;
                        for (int nodePointIndex = 0; nodePointIndex < nodePointLen; nodePointIndex++)
                        {
                            int pointIndex = _nodeBox.points[nodePointIndex];
                            point = ((_PointCloudViewerDX11)_Viewers[_cloud.viewerIndex])._Points[pointIndex];

                            // limit picking angle
                            float dotAngle = Vector3.Dot(rayDirection, (point - rayOrigin).normalized);
                            if (dotAngle > 0.99f)
                            {
                                float camDist = PointCloudMath.Distance(rayOrigin, point);
                                float pointRayDist = PointCloudMath.SqDistPointSegment(rayOrigin, rayEnd, point);
                                float normCamDist = (camDist / maxPickDistance) * pointRayDist * pointRayDist; // best so far, but still too eager to pick nearby points

                                if (normCamDist < nearestPointRayDist)
                                {
                                    // skip point if too far
                                    if (pointRayDist > maxPickDistance)
                                    {
                                        continue;
                                    }
                                    nearestPointRayDist = normCamDist;
                                    nearestIndex = pointIndex;
                                    viewerIndex = _cloud.viewerIndex;
                                }
                            }
                        } // each point inside box
                    } // if ray hits box
                } // all boxes
            } // all clouds

            if (nearestIndex > -1)
            {
                // UnityLibrary.MainThread.Call(HighLightPoint);
                _PointCloudViewerDX11 viewer = (_PointCloudViewerDX11)_Viewers[viewerIndex];
                _MainThread.Call(PointCallBack, viewer._Points[nearestIndex]);
#if DEBUG
                Debug.LogFormat(LOG_FORMAT, "Selected Point #:" + (nearestIndex) + " Position:" +
                    viewer._Points[nearestIndex] + " from " + 
                    (System.IO.Path.GetFileName(viewer.fileName)));
#endif
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "No points found..");
            }

            stopwatch.Stop();
            Debug.LogFormat(LOG_FORMAT, "PickTimer: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Reset();

            if (pointPickingThread != null && pointPickingThread.IsAlive == true)
            {
                pointPickingThread.Abort();
            }
        } // FindClosestPoint

        public override void RunPointPickingThread(Ray ray)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=cyan>RunPointPickingThread()</color></b>");

            ParameterizedThreadStart start = new ParameterizedThreadStart(FindClosestPoint);
            pointPickingThread = new Thread(start);
            pointPickingThread.IsBackground = true;
            pointPickingThread.Start(ray);
        }

        public override void RegisterCloudManually(PointCloudViewerDX11 newViewer)
        {
            // base.RegisterCloudManually(newViewer);

            for (int i = 0; i < _Viewers.Count; i++)
            {
                // remove previous same instance cloud, if already in the list  
                int viewerLen = _Viewers.Count;
                for (int vv = 0; vv < viewerLen; vv++)
                {
                    if (_Viewers[vv].fileName == newViewer.fileName)
                    {
                        Debug.LogFormat(LOG_FORMAT, "Removed duplicate cloud from viewers: " + newViewer.fileName);
                        _CloudList.RemoveAt(vv);
                        break;
                    }
                }
            }

            // add new cloud
            _Viewers.Add(newViewer);

            // manually call cloud to be processed
            // CloudIsReady(newViewer.fileName);
            OnLoadingComplete(newViewer.fileName);
        }

        public override void CloudIsReady(object cloudFilePath)
        {
            throw new System.NotSupportedException("");
        }

        protected virtual void OnLoadingComplete(object cloudFilePath)
        {
            Debug.LogFormat(LOG_FORMAT, "OnLoadingComplete(), cloudFilePath : <b>" + (string)cloudFilePath + "</b>");

            ProcessCloud((string)cloudFilePath);
        }

        protected override void ProcessCloud(string cloudPath)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ProcessCloud(), cloudPath : <b>" + cloudPath + "</b>");

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int viewerIndex = -1;
            // find index
            for (int vv = 0, viewerLen = _Viewers.Count; vv < viewerLen; vv++)
            {
                if (_Viewers[vv].fileName == cloudPath)
                {
                    viewerIndex = vv;
                    break;
                }
            }

            if (viewerIndex == -1)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Failed to find matching cloud for indexing..");
            }

            // Bounds cloudBounds = ((_PointCloudViewerDX11)_Viewers[viewerIndex]).GetBounds();
            Bounds cloudBounds = ((_PointCloudViewerDX11)_Viewers[viewerIndex]).CloudBounds;

            float minX = cloudBounds.min.x - 0.5f;// add sóme buffer for float imprecisions
            float minY = cloudBounds.min.y - 0.5f;
            float minZ = cloudBounds.min.z - 0.5f;

            float maxX = cloudBounds.max.x + 0.5f;
            float maxY = cloudBounds.max.y + 0.5f;
            float maxZ = cloudBounds.max.z + 0.5f;

            // cloud total size
            float width = (maxX - minX);
            float height = (maxY - minY);
            float depth = (maxZ - minZ);

            float stepX = width / (float)slices;
            float stepY = height / (float)slices;
            float stepZ = depth / (float)slices;

            // NOTE need to clamp to minimum 1?
            //if (stepY < 1) stepY += 32;

            float stepXInverted = 1f / stepX;
            float stepYInverted = 1f / stepY;
            float stepZInverted = 1f / stepZ;

            int totalBoxes = slices * slices * slices;

            // create new cloud object
            _Cloud newCloud = new _Cloud();
            // add to total clouds
            // init child node boxes
            newCloud.nodes = new NodeBox[totalBoxes];
            newCloud.viewerIndex = viewerIndex;
            newCloud.bounds = cloudBounds;

            float xs = minX;
            float ys = minY;
            float zs = minZ;

            float halfStepX = stepX * 0.5f;
            float halfStepY = stepY * 0.5f;
            float halfStepZ = stepZ * 0.5f;

            Vector3 _p;
            Vector3 tempCenter = Vector3.zero;
            Vector3 tempSize = Vector3.zero;
            Bounds boxBoundes = new Bounds();

            // build node boxes
            for (int y = 0; y < slices; y++) // <=========
            {
                tempSize.y = stepY;
                tempCenter.y = ys + halfStepY;
                for (int z = 0; z < slices; z++)  // <=========
                {
                    tempSize.z = stepZ;
                    tempCenter.z = zs + halfStepZ;
                    int slicesMulYZ = slices * (y + slices * z);
                    for (int x = 0; x < slices; x++)  // <=========
                    {
                        tempSize.x = stepX;
                        tempCenter.x = xs + halfStepX;

                        NodeBox np = new NodeBox();
                        boxBoundes.center = tempCenter;
                        boxBoundes.size = tempSize;
                        np.bounds = boxBoundes;
                        np.points = new List<int>(); // for struct
#if DEBUG
                        // PointCloudTools.DrawBounds(np.bounds, 20);
#endif

                        newCloud.nodes[x + slicesMulYZ] = np;
                        xs += stepX;
                    }
                    xs = minX;
                    zs += stepZ;
                }
                zs = minZ;
                ys += stepY;
            }

            stopwatch.Stop();
            stopwatch.Reset();

            stopwatch.Start();

            // pick step resolution
            int pointStep = 1;
            switch (pointIndexPrecision)
            {
                case PointIndexPrecision.Full:
                    pointStep = 1;
                    break;
                case PointIndexPrecision.Half:
                    pointStep = 2;
                    break;
                case PointIndexPrecision.Quarter:
                    pointStep = 4;
                    break;
                case PointIndexPrecision.Eighth:
                    pointStep = 8;
                    break;
                case PointIndexPrecision.Sixteenth:
                    pointStep = 16;
                    break;
                case PointIndexPrecision.TwoHundredFiftySixth:
                    pointStep = 256;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            // collect points to boxes
            _PointCloudViewerDX11 _viewer = (_PointCloudViewerDX11)_Viewers[newCloud.viewerIndex];
            int pointLen = _viewer._Points.Length;
            Debug.LogFormat(LOG_FORMAT, "_viewer._Points.Length : <b>" + _viewer._Points.Length + "</b>");
            for (int j = 0; j < pointLen; j += pointStep)
            {
                _p = _viewer._Points[j];
                // http://www.reactiongifs.com/r/mgc.gif
                int sx = (int)((_p.x - minX) * stepXInverted);
                int sy = (int)((_p.y - minY) * stepYInverted);
                int sz = (int)((_p.z - minZ) * stepZInverted);
                // Debug.LogFormat(LOG_FORMAT, "sx : " + sx + ", sy : " + sy + ", sz : " + sz + ", _p.z : " + _p.z);

                int boxIndex = sx + slices * (sy + slices * sz);
                // Debug.LogFormat(LOG_FORMAT, "newCloud.nodes : " + newCloud.nodes.Length + ", boxIndex : " + boxIndex);
                newCloud.nodes[boxIndex].points.Add(j);
            }
            // add to clouds list
            cloudList.Add(newCloud);

            stopwatch.Stop();
            //Debug.Log("Collect: " + stopwatch.ElapsedMilliseconds + " ms");
            stopwatch.Reset();
        }

        public override bool BoundsIntersectsCloud(Bounds box)
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "BoundsIntersectsCloud()");

            // check all clouds
            int length2 = _CloudList.Count;
            for (int cloudIndex = 0; cloudIndex < length2; cloudIndex++)
            {
                _Cloud _cloud = _CloudList[cloudIndex];

                // exit if outside whole cloud bounds
                if (_cloud.bounds.Contains(box.center) == false)
                {
                    return false;
                }

                // get full cloud bounds
                float minX = _cloud.bounds.min.x;
                float minY = _cloud.bounds.min.y;
                float minZ = _cloud.bounds.min.z;
                float maxX = _cloud.bounds.max.x;
                float maxY = _cloud.bounds.max.y;
                float maxZ = _cloud.bounds.max.z;

                // helpers
                float width = Mathf.Ceil(maxX - minX);
                float height = Mathf.Ceil(maxY - minY);
                float depth = Mathf.Ceil(maxZ - minZ);
                float stepX = width / slices;
                float stepY = height / slices;
                float stepZ = depth / slices;
                float stepXInverted = 1f / stepX;
                float stepYInverted = 1f / stepY;
                float stepZInverted = 1f / stepZ;

                // get collider box min node index
                int colliderX = (int)((box.center.x - minX) * stepXInverted);
                int colliderY = (int)((box.center.y - minY) * stepYInverted);
                int colliderZ = (int)((box.center.z - minZ) * stepZInverted);
                int BoxIndex = colliderX + slices * (colliderY + slices * colliderZ);
                //PointCloudHelpers.PointCloudTools.DrawBounds(clouds[cloudIndex].nodes[BoxIndex].bounds);

                // check if we hit within that area
                int l = _cloud.nodes[BoxIndex].points.Count;
                for (int j = 0; j < l; j++)
                {
                    // each point
                    int pointIndex = _cloud.nodes[BoxIndex].points[j];
                    Vector3 p = ((_PointCloudViewerDX11)_Viewers[_cloud.viewerIndex])._Points[pointIndex];

                    // check if within bounds distance
                    if (box.Contains(p) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        } // BoundsIntersectsCloud

        public override List<CollectedPoint> ConvexHullSelectPoints(GameObject go, List<Vector3> area)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ConvexHullSelectPoints(), go : " + go.name + ", area.Count : " + area.Count);
            Debug.Assert(go != null);

            // build area bounds
            Bounds areaBounds = new Bounds();
            for (int i = 0, len = area.Count; i < len; i++)
            {
                areaBounds.Encapsulate(area[i]);
            }

            // check bounds
            //PointCloudTools.DrawBounds(areaBounds, 100);

            // build area hull
            ConvexHullCalculator calc = new ConvexHullCalculator();

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            MeshFilter mf = go.GetComponent<MeshFilter>();
            /*
            if (go == null)
                Debug.LogError("Missing MeshFilter from " + go.name, go);
            */
            Mesh mesh = new Mesh();
            mf.sharedMesh = mesh;

            calc.GenerateHull(area, false, ref verts, ref tris, ref normals);

            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);

            List<CollectedPoint> results = new List<CollectedPoint>();
            // all clouds
            int length2 = _CloudList.Count;
            for (int cloudIndex = 0; cloudIndex < length2; cloudIndex++)
            {
                _Cloud _cloud = _CloudList[cloudIndex];

                // exit if outside whole cloud bounds
                if (_cloud.bounds.Intersects(areaBounds) == false)
                {
                    return null;
                }

                // check all nodes from this cloud
                for (int nodeIndex = 0; nodeIndex < _cloud.nodes.Length; nodeIndex++)
                {
                    // early exit if bounds doesnt hit this node?
                    if (_cloud.nodes[nodeIndex].bounds.Intersects(areaBounds) == false)
                    {
                        continue;
                    }

                    // loop points
                    int l = _cloud.nodes[nodeIndex].points.Count;
                    for (int j = 0; j < l; j++)
                    {
                        // check all points from that node
                        int pointIndex = _cloud.nodes[nodeIndex].points[j];
                        // get actual point
                        Vector3 p = ((_PointCloudViewerDX11)_Viewers[_cloud.viewerIndex])._Points[pointIndex];

                        // check if inside hull
                        if (IsPointInsideMesh(mesh, p) == true)
                        {
                            var temp = new CollectedPoint();
                            temp.cloudIndex = cloudIndex;
                            temp.pointIndex = pointIndex;
                            results.Add(temp);
                        }
                    }
                }

            } // for clouds

            return results;
        }

    } // class
} // namespace