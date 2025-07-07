using System;
using System.Collections.Generic;
using System.Linq;
using EA;
using UnityEngine;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    // [AddComponentMenu("Effects/Line Renderer 3D")]
    public class LineRenderer3D_Ex : EA.Line3D._LineRenderer3D
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[LineRenderer3DEx]</b></color> {0}";

        protected new Mesh _Mesh
        {
            get
            {
                if (_mesh == null)
                {
                    _mesh = new Mesh();
                    _mesh.name = "Magenta_Mesh_" + Guid.NewGuid().ToString();
                    // _mesh.hideFlags = HideFlags.HideAndDontSave;
                    _mesh.hideFlags = HideFlags.DontSave;
                    _mesh.MarkDynamic();

                    _MeshFilter.sharedMesh = _mesh;

                    Debug.LogWarningFormat(LOG_FORMAT, "===> <color=red>Mesh CREATED!!!!! and set to MeshFilter!!!!!!</color> <===, this.gameObject : " + this.gameObject.name);
                }

                return _mesh;
            }
        }

        public new List<_LinePoint> PointList
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
                updateMode = UpdateMode.All;
            }
        }
        
        [SerializeField]
        protected GameObject pointObjPrefab;
        [ReadOnly]
        [SerializeField]
        protected List<GameObject> _pointObjList = new List<GameObject>();
        public List<GameObject> PointObjList
        {
            get
            {
                return _pointObjList;
            }
        }

        [SerializeField]
        protected bool _gizmoMode = false;
        public bool GizmoMode
        {
            get
            {
                return _gizmoMode;
            }
            set
            {
                _gizmoMode = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            calculateVertexColor = false; // forcelly set!!!
            loop = false; // forcelly set!!!
        }

        protected override void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable()");

            _MeshRenderer.enabled = true;
        }

        protected override void OnDisable()
        {
            _MeshRenderer.enabled = false;
        }

        protected override void Start()
        {
            CirclePoints = circlePoints;
            CapSmoothPoints = capSmoothPoints;
            TurnSmoothPoints = turnSmoothPoints;
            SideVectorRotation = sideVectorRotation;
            // Loop = loop;
            UpdateNormals = updateNormals;
            ScaleUV = scaleUV;

            //clear nodes

            nodeList.ForEach(Node.Push);
            nodeList.Clear();

            //refresh

            SetPoints(new List<_LinePoint>(PointList));
            Calculate(UpdateMode.All);
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            // base.Update();

            if (Application.isPlaying == false)
            {
                // validate params

                CirclePoints = circlePoints;
                CapSmoothPoints = capSmoothPoints;
                TurnSmoothPoints = turnSmoothPoints;
                SideVectorRotation = sideVectorRotation;
                // Loop = loop;

                UpdateNormals = updateNormals;
                ScaleUV = scaleUV;

                if (GizmoMode == true)
                {
                    Update_PointListByObjects();
                }

                SetPoints(new List<_LinePoint>(PointList));

                updateMode = UpdateMode.All;
                Calculate(updateMode);
            }
        }
#endif

        protected override void LateUpdate()
        {
            // base.LateUpdate();

            if (Application.isPlaying == true)
            {
#if false //
                if (updateMode != UpdateMode.None)
                {
                    Debug.LogFormat(LOG_FORMAT, "@1");

                    Calculate(updateMode);
                    updateMode = UpdateMode.None;
                }
                else
                {
                    // Debug.LogFormat(LOG_FORMAT, "skip");
                }
#else
                if (GizmoMode == true)
                {
                    Update_PointListByObjects();
                }

                // SetPoints(PointList.ToArray(), PointList.Count);
                SetPoints(new List<_LinePoint>(PointList));

                updateMode = UpdateMode.All;
                Calculate(updateMode);
#endif
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            if (vertices == null || vertices.Length == 0)
                return;
            if (indices == null || indices.Length == 0)
                return;
            if (ipoints == null || ipoints.Count == 0)
                return;

            _MeshRenderer.enabled = drawMesh;

            Vector3 last;

            if (drawLine == true)
            {
                last = this.transform.TransformPoint(ipoints[0]);
                Gizmos.color = Color.white;


#if false //
                for (int a = 1; a < ipoints.Count + (Loop ? 0 : 0); a++)
#else
                Debug.Assert(loop == false);
                for (int a = 1; a < ipoints.Count; a++)
#endif
                {
                    Vector3 next = this.transform.TransformPoint(ipoints[a % ipoints.Count]);
                    Gizmos.DrawLine(last, next);
                    last = next;
                }
            }

            //draw normals
            if (drawNormals && _Mesh != null)
            {
                listVertices.Clear();
                listNormals.Clear();
                _Mesh.GetVertices(listVertices);
                _Mesh.GetNormals(listNormals);

                if (listNormals.Count != 0)
                {
                    Gizmos.color = Color.yellow;
                    for (int a = 0, c = listNormals.Count; a < c; a++)
                    {
                        Vector3 p1 = this.transform.TransformPoint(listVertices[a]);
                        Vector3 p2 = p1 + this.transform.TransformDirection(listNormals[a] * 0.1f);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }

            //draw circles
            if (drawCircles == true)
            {
                var lastSegment = nodeList.FindLast(x => x.Type != NodeType.Unused);
                if (lastSegment != null)
                {
                    int vcount = lastSegment.vOffset + lastSegment.vCount;
#if false //
                    int cyclesCount = (Loop ? vcount : (vcount - 2)) / CirclePoints;
                    int offset = Loop ? 0 : 1;
#else
                    Debug.Assert(loop == false);
                    int cyclesCount = (vcount - 2) / CirclePoints;
                    int offset = 1;
#endif

                    for (int a = 0; a < cyclesCount; a++)
                    {
                        last = this.transform.TransformPoint(vertices[(a * CirclePoints) + offset]);
                        for (int b = 1; b <= CirclePoints; b++)
                        {
                            Vector3 next = this.transform.TransformPoint(vertices[(a * CirclePoints) + (b % CirclePoints) + offset]);
                            {
                                Gizmos.color = Color.Lerp(Color.yellow, Color.red, b / (float)CirclePoints);
                                Gizmos.DrawLine(last, next);
                            }
                            last = next;
                        }

                        //draw side vector

#if false //
                        if (a < (Loop ? cyclesCount : (cyclesCount - 1)))
                        {
                            Vector3 v1 = this.transform.TransformPoint(vertices[(a * CirclePoints) + offset]);
                            Vector3 v2 = this.transform.TransformPoint(vertices[((a + 1).Loop(cyclesCount) * CirclePoints) + offset]);
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(v1, v2);
                        }
#else
                        Debug.Assert(loop == false);
                        if (a < (cyclesCount - 1))
                        {
                            Vector3 v1 = this.transform.TransformPoint(vertices[(a * CirclePoints) + offset]);
                            Vector3 v2 = this.transform.TransformPoint(vertices[((a + 1).Loop(cyclesCount) * CirclePoints) + offset]);
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(v1, v2);
                        }
#endif
                    }
                }
            }

            if (drawSideVectors)
            {
                Gizmos.color = Color.green;
                for (int a = 0; a < ipoints.Count; a++)
                {
                    Vector3 point = this.transform.TransformPoint(ipoints[a]);
                    Vector3 p1 = point + sideVectorPairs[2 * a] * .5f;
                    Vector3 p2 = point + sideVectorPairs[(2 * a) + 1] * .5f;

                    Gizmos.DrawLine(point, p1);
                    Gizmos.DrawLine(point, p2);
                }
            }
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying == true)
            {
                CirclePoints = circlePoints;
                CapSmoothPoints = capSmoothPoints;
                TurnSmoothPoints = turnSmoothPoints;
                SideVectorRotation = sideVectorRotation;
                // Loop = loop;
                Loop = false;
                UpdateNormals = updateNormals;
                ScaleUV = scaleUV;

                updateMode = UpdateMode.None;
                Calculate(UpdateMode.All);
            }
        }
#endif

        protected override void Calculate(UpdateMode mode)
        {
            // Debug.LogFormat(LOG_FORMAT, "Calculate(), updateMode : <b>" + mode + "</b>");
            // base.Calculate(mode);

            Debug.Assert(mode != UpdateMode.None);

            if (ipoints.Count < 2)
            {
                // Debug.Assert(false);
                _Mesh.Clear();
                return;
            }

            PrepareForLoop();
            CalculateUniqueNeighbouringPoints();
            CalculateSideVectors();
            CalculatePointsDistance();
            CalculateNodes();

            bool customNormals = ((UpdateNormals == UpdateNormalsMode.Internal) || (UpdateNormals == UpdateNormalsMode.InternalFlatFaces));

            //re-allocate arrays if necessary

            if (Application.isPlaying == false)
            {
                Array.Resize(ref vertices, vcount);
                Array.Resize(ref normals, vcount);
            }
            else
            {
                if (vertices == null || vertices.Length < vcount)
                {
                    Array.Resize(ref vertices, vcount);
                }
                if (customNormals && (normals == null || normals.Length < vcount))
                {
                    Array.Resize(ref normals, vcount);
                }
            }

            if (calculateUV == true)
            {
                if (Application.isPlaying == false)
                {
                    Array.Resize(ref uvs, vcount);
                    Array.Resize(ref uvYs, ipoints.Count);
                }
                else
                {
                    if (uvs == null || uvs.Length < vcount)
                    {
                        Array.Resize(ref uvs, vcount);
                    }
                    if (uvYs == null || uvYs.Length < ipoints.Count)
                    {
                        Array.Resize(ref uvYs, ipoints.Count);
                    }
                }
            }

#if false //
            if (Application.isPlaying == false)
            {
                Array.Resize(ref colors, vcount);
            }
            else
            {
                if (colors == null || colors.Length < vcount)
                {
                    Array.Resize(ref colors, vcount);
                }
            }
#else
            Debug.Assert(calculateVertexColor == false);
#endif

            if (indices == null || indices.Length < icount)
            {
                Array.Resize(ref indices, icount);
            }

            //draw mesh parts
            bool firstModifiedNodeFound = false;

            int count = nodeList.Count;
            for (int index = 0; index < count; index++)
            {
                Node node = nodeList[index];

                //if calculate UV's need to update all nodes starting with first modified one.

                if (calculateUV == true && firstModifiedNodeFound == false)
                {
                    firstModifiedNodeFound = true;
                }

                if (firstModifiedNodeFound == true)
                {
                    node.SetDirty();
                }

                //update node

                Vector3[] _normals = customNormals ? normals : null;
                Vector2[] _uvs = calculateUV ? uvs : null;
                Vector2[] _uvYs = calculateUV ? uvYs : null;

                node.Draw(ipoints, sideVectorPairs, vertices,
                    _normals,
                    _uvs, _uvYs, distances,
                    CirclePoints, CapSmoothPoints, TurnSmoothPoints,
                    scaleUV, length, CalculateFlatFaces);

                //update node colors

                Debug.Assert(calculateVertexColor == false);
                /*
                if (calculateVertexColor)
                {
                    node.UpdateColors(colors);
                }
                */
            }

            if ((mode & UpdateMode.AffectedIndices) != UpdateMode.None)
            {
                CalculateIndices();
            }

            _Mesh.Clear();
            _Mesh.SetVertices(vertices, 0, vcount, flags);
            _Mesh.SetIndices(indices, 0, icount, MeshTopology.Triangles, 0);

            int _length = calculateUV ? vcount : 0;
            _Mesh.SetUVs(0, uvs, 0, _length, flags);
#if false //
            _length = calculateVertexColor ? vcount : 0;
            _Mesh.SetColors(colors, 0, _length, flags);
#else
            _length = 0;
#endif

            switch (UpdateNormals)
            {
                case UpdateNormalsMode.None:
                    _Mesh.SetNormals(normals, 0, 0, flags);
                    break;
                case UpdateNormalsMode.Internal:
                case UpdateNormalsMode.InternalFlatFaces:
                    _Mesh.SetNormals(normals, 0, vcount, flags);
                    break;
                case UpdateNormalsMode.Unity:
                    _Mesh.RecalculateNormals();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            _Mesh.Optimize();

            if (GizmoMode == false)
            {
                Update_PointObjects(mode);
            }
        }

        protected override void SetPointsInternal(IEnumerable<_LinePoint> epoints)
        {
            // base.SetPointsInternal(epoints, count);

            int ecount = epoints.Count();

            if (ipoints.Count < ecount)
            {
                updateMode |= UpdateMode.PointsCount;
                while (ipoints.Count < ecount)
                {
                    ipoints.Add(Vector2.zero);
                    dirtyPoints.Add(true);
                }
            }

            if (ipoints.Count > ecount)
            {
                updateMode |= UpdateMode.PointsCount;

                int icount = ipoints.Count;
                ipoints.RemoveRange(ecount, icount - ecount);
                dirtyPoints.RemoveRange(ecount, icount - ecount);
            }

            while (PointList.Count < ecount)
            {
                _LinePoint point = new _LinePoint(Vector2.zero, Vector3.zero);
                PointList.Add(point);
            }

            if (PointList.Count > ecount)
            {
                PointList.RemoveRange(ecount, PointList.Count - ecount);
            }

            //set points values

            int eindex = 0;
            IEnumerator<_LinePoint> enumerator = epoints.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SetPoint(eindex++, enumerator.Current);

                if (eindex == ecount)
                {
                    break;
                }
            }
        }

        public override void SetPoints(List<_LinePoint> pointList)
        {
            // SetPointsInternal(pointList, pointList.Count);
            SetPointsInternal(pointList);
        }

        
        protected override void PrepareForLoop()
        {
            // base.PrepareForLoop();

            Debug.Assert(Loop == false);
            /*
            if (Loop == true)
            {
                if (PointList.Count > 2)
                {
                    if (PointList.Count == ipoints.Count)
                    {
                        ipoints.Add(Vector3.zero);
                        dirtyPoints.Add(true);
                    }

                    Vector3 v0 = PointList[0];
                    Vector3 vL = PointList[PointList.Count - 1];
                    float distance = Mathf.Max(Vector3.Distance(v0, vL), .001f);
                    ipoints[ipoints.Count - 1] = Vector3.Lerp(vL, v0, Mathf.Clamp01((distance - .0001f) / distance));
                }
            }
            else
            */
            {
                if (PointList.Count < ipoints.Count)
                {
                    ipoints.RemoveAt(ipoints.Count - 1);
                    dirtyPoints.RemoveAt(dirtyPoints.Count - 1);
                }
            }
        }

        protected override void CalculateNodes()
        {
            vcount = 0;
            icount = 0;

            //prepare dirties flags

            while (dirtyPoints.Count < ipoints.Count)
            {
                dirtyPoints.Add(false);
            }

            if (dirtyPoints.Count > ipoints.Count)
            {
                dirtyPoints.RemoveRange(ipoints.Count, dirtyPoints.Count - ipoints.Count);
            }

            //preapre line skeleton
            Debug.Assert(Loop == false);
            for (int a = 0, c = ipoints.Count; a < c; a++)
            {
                Node node;

                if (a == 0)//starting cap
                {
                    node = AddStartingCap();
                }
                else if (a == c - 1)//ending cap
                {
                    node = AddEndingCap(a);

                    vcount = node.vOffset + node.vCount;
                }
                else//intermediar segments
                {
                    Vector3 dir1 = (ipoints[a - 1] - ipoints[a]).normalized;
                    Vector3 dir2 = (ipoints[a + 1] - ipoints[a]).normalized;
                    float dot = Vector3.Dot(dir1, dir2);
                    bool isTurn = dot > -.9999f;

                    node = isTurn ? AddTurn(a) : AddLine(a);
                }

                node.SetPoint(ipoints[a]);
                node.SetWidth(width.Evaluate(distances[a]));

                if ((updateMode & UpdateMode.GlobalParams) != UpdateMode.None)
                {
                    node.SetDirty();
                }

                dirtyPoints[a] = node.IsDirty;
            }

            //force dirty if left/right neighbour is dirty

            if ((updateMode & UpdateMode.GlobalParams) == UpdateMode.None && dirtyPoints.Contains(true))
                for (int a = 0, c = ipoints.Count; a < c; a++)
                {
                    Node node = nodes[a];
                    if (dirtyPoints[a] == false)
                    {
#if false //
                        if (Loop)
                        {
                            if (dirtyPoints[(a - 1).Loop(c)] || dirtyPoints[(a + 1).Loop(c)])
                                node.SetDirty();
                        }
                        else
#endif
                        {
                            if (a > 0 && dirtyPoints[(a - 1).Loop(c)])
                                node.SetDirty();
                            else if (a < c - 1 && dirtyPoints[(a + 1).Loop(c)])
                                node.SetDirty();
                        }
                    }
                }

            //remove excess nodes

            int pointCount = ipoints.Count;// + (Loop ? 1 : 0);

            for (int a = pointCount; a < nodes.Count; a++)
                Node.Push(nodes[a]);
            nodes.RemoveRange(pointCount, nodes.Count - pointCount);

            //nested functions

            Node GetOrAdd(int index)
            {
                if (index >= nodes.Count)
                    nodes.Add(Node.Pop(index));

                return nodes[index];
            }

            Node AddStartingCap()
            {
                Node node = GetOrAdd(0);
                node.SetType(NodeType.StartingCap);
                node.index = 0;
                node.vCount = (CirclePoints * Mathf.Max(0, CapSmoothPoints - 1)) + CirclePoints + 1;
                node.vOffset = 0;
                node.iOffset = 0;
                node.iCount = (CirclePoints * Mathf.Max(0, CapSmoothPoints - 1) * 6) + (CirclePoints * 3) + (CirclePoints * 6);
                node.distance = distances[0];
                node.color = vertexColor.Evaluate(0);

                icount += node.iCount;

                return node;
            }

            Node AddLine(int index)
            {
                Node nodePrev = index == 0 ? null : nodes[index - 1];
                Node node = GetOrAdd(index);
                node.SetType(NodeType.Line);
                node.index = index;
                node.vCount = CirclePoints;
                node.vOffset = nodePrev == null ? 0 : (nodePrev.vOffset + nodePrev.vCount);
                node.iOffset = icount;
                node.iCount = CirclePoints * 6;
                node.distance = distances[index] * length;
                node.color = vertexColor.Evaluate(distances[index]);

                icount += node.iCount;

                return node;
            }

            Node AddTurn(int index)
            {
                Node nodePrev = index > 0 ? nodes[index - 1] : null;
                Node node = GetOrAdd(index);
                node.SetType(NodeType.Turn);
                node.index = index;
                node.vCount = CirclePoints * Mathf.Max(TurnSmoothPoints, 1);//zero vertex count means updating existing vertices
                node.vOffset = nodePrev == null ? 0 : (nodePrev.vOffset + nodePrev.vCount);
                node.iOffset = icount;
                node.iCount = CirclePoints * Mathf.Max(TurnSmoothPoints, 1) * 6;
                node.distance = distances[index] * length;
                node.color = vertexColor.Evaluate(distances[index]);

                icount += node.iCount;

                return node;
            }

            Node AddEndingCap(int index)
            {
                Node nodePrev = nodes[index - 1];
                Node node = GetOrAdd(index);
                node.SetType(NodeType.EndingCap);
                node.index = index;
                node.vOffset = nodePrev.vOffset + nodePrev.vCount;
                node.vCount = (CirclePoints * Mathf.Max(0, CapSmoothPoints - 1)) + CirclePoints + 1;
                node.iOffset = icount;
                node.iCount = (CirclePoints * Mathf.Max(0, CapSmoothPoints - 1) * 6) + (CirclePoints * 3);
                node.distance = distances[index] * length;
                node.color = vertexColor.Evaluate(distances[index]);

                icount += node.iCount;

                return node;
            }
        }

        protected virtual void Update_PointListByObjects()
        {
            if (PointObjList.Count == PointList.Count)
            {
                for (int i = 0; i < PointObjList.Count; i++)
                {
                    PointList[i].position = PointObjList[i].transform.localPosition;
                    PointList[i].eulerAngles = PointObjList[i].transform.localEulerAngles;
                }
            }
        }

        protected virtual void Update_PointObjects(UpdateMode mode)
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "Update_PointObjects(), mode : <b><color=red>" + mode + "</color></b>");

            if ((updateMode & UpdateMode.PointsCount) != 0) // Number of points in line updated
            {
                if (PointObjList.Count != PointList.Count)
                {
                    ResetAllPointObjects();
                }
            }

            if ((updateMode & UpdateMode.SinglePoints) != 0) // Single existing points position updated
            {
                Update_PointObjectsPosition();
            }
        }

        protected virtual void ResetAllPointObjects()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ResetAllPointObjects()");

            // Clear All
            for (int i = PointObjList.Count - 1; i >= 0; i--)
            {
                if (Application.isPlaying == true)
                {
                    Destroy(PointObjList[i]);
                }
                else
                {
                    DestroyImmediate(PointObjList[i]);
                }
            }
            PointObjList.Clear();

            // Create All
            for (int i = 0; i < PointList.Count; i++)
            {
                GameObject obj = GameObject.Instantiate(pointObjPrefab);
                obj.name = "P" + i;
                obj.transform.SetParent(this.transform, false);
                // obj.transform.position = new Vector3(PointList[i].x, PointList[i].y, PointList[i].z);
                obj.transform.localPosition = new Vector3(PointList[i].position.x, PointList[i].position.y, PointList[i].position.z);
                obj.transform.localEulerAngles = new Vector3(PointList[i].eulerAngles.x, PointList[i].eulerAngles.y, PointList[i].eulerAngles.z);

                PointObjList.Add(obj);
            }
        }

        protected virtual void Update_PointObjectsPosition()
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "Update_PointObjectsPosition()");

            for (int i = 0; i < PointList.Count; i++)
            {
                GameObject obj = PointObjList[i];
                // obj.transform.position = new Vector3(PointList[i].x, PointList[i].y, PointList[i].z);
                obj.transform.localPosition = new Vector3(PointList[i].position.x, PointList[i].position.y, PointList[i].position.z);
                obj.transform.localEulerAngles = new Vector3(PointList[i].eulerAngles.x, PointList[i].eulerAngles.y, PointList[i].eulerAngles.z);
            }
        }
    }
}