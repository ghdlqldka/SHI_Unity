using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace EA.Line3D
{
    [ExecuteInEditMode]
    [AddComponentMenu("Effects/Line Renderer 3D")]
    public /*sealed*/ class LineRenderer3D : MonoBehaviour
    {
        #region Static

        const float PositionError = .00001f;

        protected const UnityEngine.Rendering.MeshUpdateFlags flags =

#if LINE3D_DEV_MODE
            UnityEngine.Rendering.MeshUpdateFlags.Default;
#else
            UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices
          | UnityEngine.Rendering.MeshUpdateFlags.DontResetBoneBounds
          | UnityEngine.Rendering.MeshUpdateFlags.DontNotifyMeshUsers
            //| UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds
            ;
#endif

        static int GetVertexRoundLoopIndex(int offset, int roundIndex, int roundLength, int roundVertexIndex)
        {
            return offset + (roundIndex * roundLength) + roundVertexIndex % roundLength;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/Effects/Line 3D")]
        static void Create()
        {
            GameObject go = new GameObject("Line3D");
            go.AddComponent<LineRenderer3D>();

            var parent = UnityEditor.Selection.activeGameObject;
            if (parent)
                UnityEditor.GameObjectUtility.SetParentAndAlign(go, parent);

            UnityEditor.Selection.activeGameObject = go;
        }

#endif

        #endregion

        #region Private Fields

        [SerializeField] protected int circlePoints = 4;
        [SerializeField] protected int capSmoothPoints = 0;
        [SerializeField] protected int turnSmoothPoints = 0;
        [SerializeField] protected float sideVectorRotation;
        [SerializeField] protected AnimationCurve width = new AnimationCurve(new Keyframe(0, 0.25f), new Keyframe(1, 0.25f));
        [SerializeField] protected UpdateNormalsMode updateNormals = UpdateNormalsMode.Internal;
        [SerializeField] protected bool loop;
        [SerializeField] protected bool calculateVertexColor;
        [SerializeField] protected Gradient vertexColor = new Gradient();
        [SerializeField] protected bool calculateUV;
        [SerializeField] protected Vector2 scaleUV = Vector2.one;
        [SerializeField] protected List<Vector3> points = new List<Vector3>() { Vector3.zero, Vector3.up };

        // ------------ Serialized Hidden

        [SerializeField] protected List<Node> nodes = new List<Node>();
        [SerializeField, HideInInspector] protected MeshRenderer _meshRenderer;
        [SerializeField, HideInInspector] bool flatFacesApplied;

        // ------------ Non serialize

        [NonSerialized] protected MeshFilter _meshFilter;
        [NonSerialized] protected Mesh _mesh = null;
        [NonSerialized] protected List<Vector3> ipoints = new List<Vector3>();
        [NonSerialized] protected List<bool> dirtyPoints = new List<bool>();
        [NonSerialized] protected Vector3[] vertices = null;
        [NonSerialized] protected Vector3[] normals = null;
        [NonSerialized] protected Vector2[] uvs = null;
        [NonSerialized] protected Color32[] colors = null;
        [NonSerialized] protected int[] indices = null;
        [NonSerialized] protected float[] distances = null;
        [NonSerialized] protected Vector2[] uvYs = null;//contains Y-axis uv value along the line
        [NonSerialized] protected Vector3[] sideVectorPairs = null;
        [NonSerialized] protected UpdateMode updateMode = UpdateMode.None;

        // ------------ Read Only

        protected bool drawMesh = true;
        protected bool drawLine;
        protected bool drawCircles;
        protected bool drawNormals;
        protected bool drawSideVectors;

        // ------------ Read Only

        protected int vcount = 0;
        protected int icount = 0;
        protected float length = 0;

        #endregion

        #region Public Properties

        public int PointsCount
        {
            get => points.Count;
            set
            {
                if (value < 0) return;
                if (points.Count == value) return;

                updateMode |= UpdateMode.PointsCount;

                if (points.Count < value)
                {
                    while (points.Count < value)
                    {
                        points.Add(Vector3.zero);
                        ipoints.Add(Vector3.zero);
                    }
                }
                else
                {
                    ipoints.RemoveRange(value, ipoints.Count - value);
                    points.RemoveRange(value, points.Count - value);
                }
            }
        }

        public int CirclePoints
        {
            get => CalculateFlatFaces ? (circlePoints * 2) : (circlePoints + 1);
            set
            {
                int newValue = Mathf.Max(value, 3);
                if (circlePoints != newValue)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    circlePoints = newValue;
                }
            }
        }

        public int CapSmoothPoints
        {
            get => capSmoothPoints;
            set
            {
                int newValue = Mathf.Max(value, 0);
                if (capSmoothPoints != newValue)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    capSmoothPoints = newValue;
                }
            }
        }

        public int TurnSmoothPoints
        {
            get => turnSmoothPoints;
            set
            {
                int newValue = Mathf.Max(value, 1);
                if (turnSmoothPoints != newValue)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    turnSmoothPoints = newValue;
                }
            }
        }

        public float SideVectorRotation
        {
            get => sideVectorRotation;
            set
            {
                if (sideVectorRotation != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    sideVectorRotation = value;
                }
            }
        }

        public bool Loop
        {
            get => loop;
            set
            {
                if (loop != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    loop = value;
                }
            }
        }

        public UpdateNormalsMode UpdateNormals
        {
            get => updateNormals;
            set
            {
                if (updateNormals != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    updateNormals = value;
                }
            }
        }

        public Vector2 ScaleUV
        {
            get => scaleUV;
            set
            {
                if (scaleUV != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    CalculateUV = true;
                    scaleUV = value;
                }
            }
        }

        public bool CalculateVertexColor
        {
            get => calculateVertexColor;
            set
            {
                if (calculateVertexColor != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    calculateVertexColor = value;
                }
            }
        }

        public bool CalculateUV
        {
            get => calculateUV;
            set
            {
                if (calculateUV != value)
                {
                    updateMode |= UpdateMode.GlobalParams;
                    calculateUV = value;
                }
            }
        }

        // ------------ Read Only

        public int VCount => vcount;
        public int ICount => icount;
        public float Length => length;

        // ------------ Editor Only

#if UNITY_EDITOR

        public bool DrawMesh { get => drawMesh; set => drawMesh = value; }
        public bool DrawLine { get => drawLine; set => drawLine = value; }
        public bool DrawCircles { get => drawCircles; set => drawCircles = value; }
        public bool DrawNormals { get => drawNormals; set => drawNormals = value; }
        public bool DrawSideVectors { get => drawSideVectors; set => drawSideVectors = value; }

#endif

        #endregion

        #region Private Properties

        MeshFilter MeshFilter
        {
            get
            {
                Debug.LogError("@1");
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();

                    if (_meshFilter == null)
                        _meshFilter = gameObject.AddComponent<MeshFilter>();

                    _meshFilter.hideFlags = HideFlags.HideInInspector;
                }

                return _meshFilter;
            }
        }

        MeshRenderer MeshRenderer
        {
            get
            {
                Debug.LogError("@2");
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();

                    if (_meshRenderer == null)
                        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

                    _meshRenderer.hideFlags = HideFlags.HideInInspector;
                }

                return _meshRenderer;
            }
        }

        Mesh Mesh
        {
            get
            {
                Debug.LogError("@3");
                if (_mesh == null)
                {
                    _mesh = new Mesh();
                    _mesh.name = Guid.NewGuid().ToString();
                    _mesh.hideFlags = HideFlags.HideAndDontSave;
                    _mesh.MarkDynamic();

                    MeshFilter.sharedMesh = _mesh;
                }

                return _mesh;
            }
        }

        protected bool CalculateFlatFaces => updateNormals == UpdateNormalsMode.InternalFlatFaces;

        #endregion

        #region Public Methods

        public virtual void SetPoint(int index, Vector3 point)
        {
            if (index < 0 || index >= points.Count)
            {
                LogError("{0}({1}, {2}) invalid index!", nameof(SetPoint), index, point);
                return;
            }

            //set point

            ipoints[index] = point;
            points[index] = point;

            //check if point dirties the node

            bool isDirty = false;

            if (index >= nodes.Count)
                isDirty = true;
            else
            {
                var node = nodes[index];
                node.SetPoint(point);
                isDirty = node.IsDirty;
            }

            if (isDirty) updateMode |= UpdateMode.SinglePoints;
        }

        public virtual void SetPoints(Vector3[] points) => SetPointsInternal(points, points.Length);

        public virtual void SetPoints(Vector3[] points, int length) => SetPointsInternal(points, length);

        public virtual void SetPoints(List<Vector3> points) => SetPointsInternal(points, points.Count);

        public virtual void SetPoints(List<Vector3> points, int count) => SetPointsInternal(points, count);

        public virtual void SetWidthCurve(AnimationCurve curve)
        {
            width = curve;
            updateMode |= UpdateMode.GlobalParams;
        }

        public void SetVertexColorGradient(Gradient gradient)
        {
            CalculateVertexColor = true;
            vertexColor = gradient;
            updateMode |= UpdateMode.GlobalParams;
        }

        [Obsolete]
        void ForceImmediateUpdate()
        {
            Calculate(updateMode);
            updateMode = UpdateMode.None;
        }

        #endregion

        #region Monobehaviour

        protected virtual void Reset() => _mesh = null;

        protected virtual void OnEnable() => MeshRenderer.enabled = true;

        protected virtual void Start()
        {
            CirclePoints = circlePoints;
            CapSmoothPoints = capSmoothPoints;
            TurnSmoothPoints = turnSmoothPoints;
            SideVectorRotation = sideVectorRotation;
            Loop = loop;
            UpdateNormals = updateNormals;
            ScaleUV = scaleUV;

            //clear nodes

            nodes.ForEach(Node.Push);
            nodes.Clear();

            //refresh

            SetPoints(new List<Vector3>(points));
            Calculate(UpdateMode.All);
        }

#if UNITY_EDITOR
        protected virtual void Update()
        {
            if (!Application.isPlaying)
            {
                //if (!UnityEditor.Selection.objects.Contains(gameObject))
                //    return;

                // validate params

                CirclePoints = circlePoints;
                CapSmoothPoints = capSmoothPoints;
                TurnSmoothPoints = turnSmoothPoints;
                SideVectorRotation = sideVectorRotation;
                Loop = loop;
                UpdateNormals = updateNormals;
                ScaleUV = scaleUV;

                SetPoints(points.ToArray(), points.Count);

                updateMode = UpdateMode.All;
                Calculate(updateMode);
            }
        }

        [NonSerialized] protected List<Vector3> listVertices = new List<Vector3>();
        [NonSerialized] protected List<Vector3> listNormals = new List<Vector3>();

        protected virtual void OnDrawGizmos()
        {
            if (vertices == null || vertices.Length == 0) return;
            if (indices == null || indices.Length == 0) return;
            if (ipoints == null || ipoints.Count == 0) return;

            MeshRenderer.enabled = drawMesh;

            Vector3 last;

            if (drawLine)
            {
                last = transform.TransformPoint(ipoints[0]);
                Gizmos.color = Color.white;
                for (int a = 1; a < ipoints.Count + (Loop ? 0 : 0); a++)
                {
                    Vector3 next = transform.TransformPoint(ipoints[a % ipoints.Count]);
                    Gizmos.DrawLine(last, next);
                    last = next;
                }
            }

            //draw normals

            if (drawNormals && Mesh != null)
            {
                listVertices.Clear();
                listNormals.Clear();
                Mesh.GetVertices(listVertices);
                Mesh.GetNormals(listNormals);

                if (listNormals.Count != 0)
                {
                    Gizmos.color = Color.yellow;
                    for (int a = 0, c = listNormals.Count; a < c; a++)
                    {
                        Vector3 p1 = transform.TransformPoint(listVertices[a]);
                        Vector3 p2 = p1 + transform.TransformDirection(listNormals[a] * 0.1f);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }

            //draw circles

            if (drawCircles)
            {
                var lastSegment = nodes.FindLast(x => x.Type != NodeType.Unused);
                if (lastSegment != null)
                {
                    int vcount = lastSegment.vOffset + lastSegment.vCount;
                    int cyclesCount = (Loop ? vcount : (vcount - 2)) / CirclePoints;
                    int offset = Loop ? 0 : 1;

                    for (int a = 0; a < cyclesCount; a++)
                    {
                        last = transform.TransformPoint(vertices[(a * CirclePoints) + offset]);
                        for (int b = 1; b <= CirclePoints; b++)
                        {
                            Vector3 next = transform.TransformPoint(vertices[(a * CirclePoints) + (b % CirclePoints) + offset]);
                            {
                                Gizmos.color = Color.Lerp(Color.yellow, Color.red, b / (float)CirclePoints);
                                Gizmos.DrawLine(last, next);
                            }
                            last = next;
                        }

                        //draw side vector

                        if (a < (Loop ? cyclesCount : (cyclesCount - 1)))
                        {
                            Vector3 v1 = transform.TransformPoint(vertices[(a * CirclePoints) + offset]);
                            Vector3 v2 = transform.TransformPoint(vertices[((a + 1).Loop(cyclesCount) * CirclePoints) + offset]);
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(v1, v2);
                        }
                    }
                }
            }

            if (drawSideVectors)
            {
                Gizmos.color = Color.green;
                for (int a = 0; a < ipoints.Count; a++)
                {
                    Vector3 point = transform.TransformPoint(ipoints[a]);
                    Vector3 p1 = point + sideVectorPairs[2 * a] * .5f;
                    Vector3 p2 = point + sideVectorPairs[(2 * a) + 1] * .5f;

                    Gizmos.DrawLine(point, p1);
                    Gizmos.DrawLine(point, p2);
                }
            }
        }

        

        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
            {
                CirclePoints = circlePoints;
                CapSmoothPoints = capSmoothPoints;
                TurnSmoothPoints = turnSmoothPoints;
                SideVectorRotation = sideVectorRotation;
                Loop = loop;
                UpdateNormals = updateNormals;
                ScaleUV = scaleUV;

                updateMode = UpdateMode.None;
                Calculate(UpdateMode.All);
            }
        }

#endif

        protected virtual void LateUpdate()
        {
            if (Application.isPlaying)
            {
                if (updateMode != UpdateMode.None)
                {
                    Calculate(updateMode);
                    updateMode = UpdateMode.None;
                }
            }
        }

        protected virtual void OnDisable() => MeshRenderer.enabled = false;

        #endregion

        #region Private Methods

        protected virtual void SetPointsInternal(IEnumerable<Vector3> points, int count)
        {
            var enumerator = points.GetEnumerator();
            int ecount = points.Count();

            if (ecount < count)
            {
                LogError("{0}({1}, {2}) array has less elements than specified by count field.",
                    nameof(SetPoints), ecount, count);
                return;
            }

            ecount = count;

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

            while (this.points.Count < ecount)
                this.points.Add(Vector2.zero);

            if (this.points.Count > ecount)
                this.points.RemoveRange(ecount, this.points.Count - ecount);

            //set points values

            int eindex = 0;

            while (enumerator.MoveNext())
            {
                SetPoint(eindex++, enumerator.Current);

                if (eindex == ecount)
                    break;
            }
        }

        protected virtual void Calculate(UpdateMode updateMode)
        {
            if (ipoints.Count < 2)
            {
                Mesh.Clear();
                return;
            }

            PrepareForLoop();
            CalculateUniqueNeighbouringPoints();
            CalculateSideVectors();
            CalculatePointsDistance();
            CalculateNodes();

            bool customNormals = UpdateNormals == UpdateNormalsMode.Internal || UpdateNormals == UpdateNormalsMode.InternalFlatFaces;

            //re-allocate arrays if necessary

            if (vertices == null || vertices.Length < vcount || !Application.isPlaying) Array.Resize(ref vertices, vcount);
            if (customNormals && ((normals == null || normals.Length < vcount) || !Application.isPlaying)) Array.Resize(ref normals, vcount);
            if (calculateUV)
            {
                if (uvs == null || uvs.Length < vcount || !Application.isPlaying) Array.Resize(ref uvs, vcount);
                if (uvYs == null || uvYs.Length < ipoints.Count || !Application.isPlaying) Array.Resize(ref uvYs, ipoints.Count);
            }
            if (colors == null || colors.Length < vcount || !Application.isPlaying) Array.Resize(ref colors, vcount);
            if (indices == null || indices.Length < icount) Array.Resize(ref indices, icount);

            //draw mesh parts

            bool firstModifiedNodeFound = false;

            for (int a = 0, c = nodes.Count; a < c; a++)
            {
                var node = nodes[a];

                //if calculate UV's need to update all nodes starting with first modified one.

                if (calculateUV && !firstModifiedNodeFound)
                    firstModifiedNodeFound = true;

                if (firstModifiedNodeFound)
                    node.SetDirty();

                //update node

                node.Draw(
                    ipoints, sideVectorPairs, vertices,
                    customNormals ? normals : null,
                    calculateUV ? uvs : null, calculateUV ? uvYs : null, distances,
                    CirclePoints, CapSmoothPoints, TurnSmoothPoints,
                    scaleUV, length, CalculateFlatFaces);

                //update node colors

                if (calculateVertexColor)
                    node.UpdateColors(colors);
            }

            if ((updateMode & UpdateMode.AffectedIndices) != UpdateMode.None)
                CalculateIndices();

            Mesh.Clear();
            Mesh.SetVertices(vertices, 0, vcount, flags);
            Mesh.SetIndices(indices, 0, icount, MeshTopology.Triangles, 0);

            Mesh.SetUVs(0, uvs, 0, calculateUV ? vcount : 0, flags);
            Mesh.SetColors(colors, 0, calculateVertexColor ? vcount : 0, flags);

            switch (UpdateNormals)
            {
                case UpdateNormalsMode.Internal:
                case UpdateNormalsMode.InternalFlatFaces: Mesh.SetNormals(normals, 0, vcount, flags); break;
                case UpdateNormalsMode.Unity: Mesh.RecalculateNormals(); break;
                default: Mesh.SetNormals(normals, 0, 0, flags); break;
            }

            Mesh.Optimize();
        }

        protected virtual void PrepareForLoop()
        {
            if (Loop)
            {
                if (points.Count > 2)
                {
                    if (points.Count == ipoints.Count)
                    {
                        ipoints.Add(Vector3.zero);
                        dirtyPoints.Add(true);
                    }

                    Vector3 v0 = points[0];
                    Vector3 vL = points[points.Count - 1];
                    float distance = Mathf.Max(Vector3.Distance(v0, vL), .001f);
                    ipoints[ipoints.Count - 1] = Vector3.Lerp(vL, v0, Mathf.Clamp01((distance - .0001f) / distance));
                }
            }
            else
            {
                if (points.Count < ipoints.Count)
                {
                    ipoints.RemoveAt(ipoints.Count - 1);
                    dirtyPoints.RemoveAt(dirtyPoints.Count - 1);
                }
            }
        }

        protected virtual void CalculateNodes()
        {
            vcount = 0;
            icount = 0;

            //prepare dirties flags

            while (dirtyPoints.Count < ipoints.Count)
                dirtyPoints.Add(false);

            if (dirtyPoints.Count > ipoints.Count)
                dirtyPoints.RemoveRange(ipoints.Count, dirtyPoints.Count - ipoints.Count);

            //preapre line skeleton

            for (int a = 0, c = ipoints.Count; a < c; a++)
            {
                Node node;

                if (a == 0)//starting cap
                {
                    if (Loop)
                    {
                        Vector3 dir1 = (ipoints[ipoints.Count - 1] - ipoints[a]).normalized;
                        Vector3 dir2 = (ipoints[a + 1] - ipoints[a]).normalized;
                        float dot = Vector3.Dot(dir1, dir2);
                        bool isTurn = dot > -.9999f;

                        node = isTurn ? AddTurn(a) : AddLine(a);
                    }
                    else
                    {
                        node = AddStartingCap();
                    }
                }
                else if (a == c - 1)//ending cap
                {
                    if (Loop)
                    {
                        Vector3 dir1 = (ipoints[a - 1] - ipoints[a]).normalized;
                        Vector3 dir2 = (ipoints[0] - ipoints[a]).normalized;
                        float dot = Vector3.Dot(dir1, dir2);
                        bool isTurn = dot > -.9999f;
                        node = isTurn ? AddTurn(a) : AddLine(a);
                    }
                    else
                    {
                        node = AddEndingCap(a);
                    }

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
                    node.SetDirty();

                dirtyPoints[a] = node.IsDirty;
            }

            //force dirty if left/right neighbour is dirty

            if ((updateMode & UpdateMode.GlobalParams) == UpdateMode.None && dirtyPoints.Contains(true))
                for (int a = 0, c = ipoints.Count; a < c; a++)
                {
                    Node node = nodes[a];
                    if (!dirtyPoints[a])
                    {
                        if (Loop)
                        {
                            if (dirtyPoints[(a - 1).Loop(c)] || dirtyPoints[(a + 1).Loop(c)])
                                node.SetDirty();
                        }
                        else
                        {
                            if (a > 0 && dirtyPoints[(a - 1).Loop(c)]) node.SetDirty();
                            else if (a < c - 1 && dirtyPoints[(a + 1).Loop(c)]) node.SetDirty();
                        }
                    }
                }

            //remove excess nodes

            int pointCount = ipoints.Count;// + (Loop ? 1 : 0);

            for (int a = pointCount; a < nodes.Count; a++) Node.Push(nodes[a]);
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

        protected void CalculateUniqueNeighbouringPoints()
        {
            for (int a = 0, c = ipoints.Count; a < c; a++)
            {
                Vector3 p = ipoints[a];
                Vector3 pNext = ipoints[(a + 1).Loop(c)];

                bool changed = false;

                //if (p == pPrev)
                //{
                //    changed = true;
                //    p += URandom.onUnitSphere * PositionError;
                //}

                if (p == pNext)
                {
                    changed = true;
                    p.x += URandom.Range(.5f, 1f) * PositionError;
                }

                if (changed)
                    ipoints[a] = p;
            }
        }

        protected void CalculateSideVectors()
        {
            if (sideVectorPairs == null || sideVectorPairs.Length != 2 * ipoints.Count)
                Array.Resize(ref sideVectorPairs, 2 * ipoints.Count);

            Vector3 d1, d2, ds;

            if (Loop)
            {
                d1 = (ipoints[0] - ipoints[1]).normalized;
                d2 = (ipoints[0] - ipoints[ipoints.Count - 1]).normalized;
            }
            else
            {
                d1 = (ipoints[1] - ipoints[0]).normalized;
                d2 = -d1;
            }

            ds = Vector3.Slerp(d1, d2, 0.5f);
            ds = Quaternion.AngleAxis(SideVectorRotation * 360f, d1) * ds;

            sideVectorPairs[0] = ds;
            sideVectorPairs[1] = ds;

            int c1 = ipoints.Count;
            int c2 = ipoints.Count * 2;

            for (int a = 1, c = ipoints.Count; a < c; a++)
            {
                Vector3 vprev = ipoints[(a - 1).Loop(c1)];
                Vector3 vcurr = ipoints[a.Loop(c1)];
                Vector3 vnext = ipoints[(a + 1).Loop(c1)];

                Vector3 lastSide = sideVectorPairs[((2 * a) - 1).Loop(c2)];

                d1 = (vcurr - vprev).normalized;
                d2 = (vnext - vcurr).normalized;

                Vector3 newSide = Quaternion.FromToRotation(d1, d2) * lastSide;

                sideVectorPairs[(2 * a).Loop(c2)] = lastSide;
                sideVectorPairs[((2 * a) + 1).Loop(c2)] = newSide;
            }

            if (Loop)
            {
                sideVectorPairs[0] = sideVectorPairs[sideVectorPairs.Length - 1];
            }
            else
            {
                sideVectorPairs[sideVectorPairs.Length - 1] = sideVectorPairs[sideVectorPairs.Length - 2];
            }
        }

        protected void CalculatePointsDistance()
        {
            if (distances == null || distances.Length != ipoints.Count)
                Array.Resize(ref distances, ipoints.Count);

            length = 0;
            distances[0] = 0;
            Vector3 last = ipoints[0];
            for (int a = 1, c = ipoints.Count; a < c; a++)
            {
                Vector3 v = ipoints[a];
                length += Vector3.Distance(last, v);
                distances[a] = length;
                last = v;
            }

            for (int a = 1, c = ipoints.Count; a < c; a++)
                distances[a] = distances[a] / length;
        }

        protected void CalculateIndices()
        {
            if (vcount <= 0) return;

            int cyclesCount = (Loop ? vcount : (vcount - 2)) / CirclePoints;
            int offset = Loop ? 0 : 1;
            int iIndex = 0;

            //starting cap

            if (!Loop)
                for (int b = 0, c2 = CirclePoints; b < c2; b++)
                {
                    indices[iIndex++] = 0;
                    indices[iIndex++] = GetVertexRoundLoopIndex(offset, 0, c2, b + 1);
                    indices[iIndex++] = GetVertexRoundLoopIndex(offset, 0, c2, b);
                }

            //cycles

            if (Loop)
            {
                for (int a = 0, c1 = cyclesCount - 1; a <= c1; a++)
                {
                    for (int b = 0, c2 = CirclePoints; b < c2; b++)
                    {
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, (a + 1) % cyclesCount, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, (a + 1) % cyclesCount, CirclePoints, b);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, (a + 1) % cyclesCount, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b);
                    }
                }
            }
            else
            {
                for (int a = 0, c1 = cyclesCount - 1; a < c1; a++)
                {
                    for (int b = 0, c2 = CirclePoints; b < c2; b++)
                    {
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a + 1, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a + 1, CirclePoints, b);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a + 1, CirclePoints, b + 1);
                        indices[iIndex++] = GetVertexRoundLoopIndex(offset, a, CirclePoints, b);
                    }
                }
            }

            //ending cap

            if (!Loop)
                for (int b = 0, c2 = CirclePoints; b < c2; b++)
                {
                    indices[iIndex++] = GetVertexRoundLoopIndex(offset, cyclesCount - 1, c2, b);
                    indices[iIndex++] = GetVertexRoundLoopIndex(offset, cyclesCount - 1, c2, b + 1);
                    indices[iIndex++] = vcount - 1;
                }
        }

        [ContextMenu("Clear")]
        void Clear()
        {
            ipoints.Clear();
            dirtyPoints.Clear();

            vertices = null;
            normals = null;
            uvs = null;
            colors = null;
            indices = null;
            distances = null;
            sideVectorPairs = null;
        }

        void Log(string format, params object[] args) => Debug.LogFormat($"[{nameof(LineRenderer3D)}]" + format, args);

        void LogError(string format, params object[] args) => Debug.LogErrorFormat($"[{nameof(LineRenderer3D)}]" + format, args);

        #endregion

        [Serializable]
        public class Node
        {
            //static access

            static Stack<Node> instances = new Stack<Node>();

            public static Node Pop(int index)
            {
                Node node = instances.Count == 0 ? new Node() : instances.Pop();
                node.index = index;
                return node;
            }

            public static void Push(Node node)
            {
                node.SetType(NodeType.Unused);
                instances.Push(node);
            }

            //fields

            [SerializeField] string id;//used in editor only
            [SerializeField] NodeType type;
            [SerializeField] Vector3 point;
            [SerializeField] float width;

            public int index;

            public int vOffset;
            public int vCount;

            public int iOffset;
            public int iCount;

            public float distance;
            public Color32 color;

            [SerializeField, GUIDisabled] bool isDirty;

            //runtime only

            [NonSerialized] List<Vector3> points;
            [NonSerialized] Vector3[] sideVectors;
            [NonSerialized] Vector3[] vertices;
            [NonSerialized] Vector3[] normals;
            [NonSerialized] Vector2[] uvs;
            [NonSerialized] Vector2[] uvYs;
            [NonSerialized] float[] distances;
            [NonSerialized] int circlePoints;
            [NonSerialized] int capSmoothPoints;
            [NonSerialized] int turnSmoothPoints;
            [NonSerialized] Vector2 scaleUV;
            [NonSerialized] float length;
            [NonSerialized] bool flatFaces;

            [NonSerialized] bool calculateNormals;
            [NonSerialized] bool calculateUVs;

            //properties

            public NodeType Type => type;
            public bool IsDirty => isDirty;

            //constructors

            Node() => SetType(NodeType.Unused);

            //methods

            public void SetType(NodeType type)
            {
                if (this.type != type)
                {
                    this.type = type;
                    isDirty = true;
#if UNITY_EDITOR
                    id = $"{index}.{type}";
#endif
                }

                if (type == NodeType.Unused)
                {
                    index = -1;
                    isDirty = false;
                    point = Vector3.positiveInfinity;
                    width = float.PositiveInfinity;

                    points = null;
                    sideVectors = null;
                    vertices = null;
                    normals = null;
                    uvs = null;
                }
            }

            public void SetPoint(Vector3 point)
            {
                if (this.point != point)
                {
                    isDirty = true;
                    this.point = point;
                }
            }

            public void SetWidth(float width)
            {
                if (!Mathf.Approximately(this.width, width))
                {
                    isDirty = true;
                    this.width = width;
                }
            }

            public void SetDirty() => isDirty = true;

            public void Draw(
                List<Vector3> points, Vector3[] sideVectors, Vector3[] vertices,
                Vector3[] normals, Vector2[] uvs, Vector2[] uvYs, float[] distances,
                int circlePoints, int capSmoothPoints, int turnSmoothPoints,
                Vector2 scaleUV, float length, bool flatFaces)
            {
                if (isDirty)
                {
                    isDirty = false;

                    this.points = points;
                    this.sideVectors = sideVectors;
                    this.vertices = vertices;
                    this.normals = normals;
                    this.uvs = uvs;
                    this.uvYs = uvYs;
                    this.distances = distances;

                    this.circlePoints = circlePoints;
                    this.capSmoothPoints = capSmoothPoints;
                    this.turnSmoothPoints = turnSmoothPoints;
                    this.scaleUV = scaleUV;
                    this.length = length;
                    this.flatFaces = flatFaces;

                    calculateNormals = normals != null;
                    calculateUVs = uvs != null;

                    switch (type)
                    {
                        case NodeType.StartingCap: DrawStartingCap(); break;
                        case NodeType.Line: DrawLine(); break;
                        case NodeType.Turn: DrawTurn(); break;
                        case NodeType.EndingCap: DrawEndingCap(); break;
                    }
                }
            }

            public void UpdateColors(Color32[] colors)
            {
                if (colors != null)
                    for (int a = vOffset, c = vOffset + vCount; a < c; a++)
                        colors[a] = color;
            }

            void DrawStartingCap()
            {
                Vector3 point0 = points[index + 1];
                Vector3 point1 = points[index];

                Vector3 dirUp = (point1 - point0).normalized;
                Vector3 dirDown = (point0 - point1).normalized;
                Vector3 dirRight = Vector3.ProjectOnPlane(sideVectors[2 * index], dirDown).normalized;
                Vector3 dirForward = Vector3.Cross(dirDown, dirRight).normalized;

                int vIndex = vOffset;

                float uvStart = 0;
                float uvEnd = 0;

                if (calculateUVs)
                {
                    uvStart = Mathf.PI * .5f * -width * scaleUV.y;
                    uvEnd = 0;
                    uvYs[0] = new Vector2(uvStart, uvEnd);
                }

                Vector3 vertex = point1 + (capSmoothPoints == 0 ? Vector3.zero : (dirUp * width));
                vertices[vIndex++] = vertex;

                if (calculateNormals) normals[vIndex - 1] = (point1 - point0).normalized;
                if (calculateUVs) uvs[vIndex - 1] = new Vector2(0.5f, uvStart);

                for (int a = 0; a <= Mathf.Max(0, capSmoothPoints - 1); a++)
                {
                    float fraction = (a + 1f) / capSmoothPoints;
                    float angleRad = (capSmoothPoints == 0 ? 90 : (90 * fraction)) * Mathf.Deg2Rad;
                    float height = this.width * Mathf.Cos(angleRad);
                    float width = this.width * Mathf.Sin(angleRad);
                    Vector3 v1 = dirRight * width;
                    Vector3 v2 = dirForward * width;
                    Vector3 v3 = dirUp * height;

                    for (int b = 0, c = flatFaces ? ((circlePoints / 2) + 1) : circlePoints; b < c; b++)
                    {
                        Vector3 v = Vector3.SlerpUnclamped(v1, v2, (4f * b) / (c - 1f));
                        vertex = point1 + v + v3;

                        if (flatFaces && b > 0 && b < c - 1)
                        {
                            vertices[vIndex] = vertex;
                            vertices[vIndex + 1] = vertex;

                            if (calculateNormals)
                            {
                                Vector3 normal = (vertex - point1).normalized;
                                float angle = 180f / (c - 1f);

                                normals[vIndex] = Quaternion.AngleAxis(angle, dirUp) * normal;
                                normals[vIndex + 1] = Quaternion.AngleAxis(-angle, dirUp) * normal;
                            }

                            if (calculateUVs)
                            {
                                Vector2 uv = new Vector2((-b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, fraction));
                                uvs[vIndex] = uv;
                                uvs[vIndex + 1] = uv;
                            }
                            vIndex += 2;
                        }
                        else
                        {
                            vertices[vIndex] = vertex;
                            if (calculateNormals)
                            {
                                Vector3 normal = (vertex - point1).normalized;

                                if (flatFaces)
                                {
                                    float angle = 180f / (c - 1f);
                                    angle *= b == 0 ? -1 : 1;
                                    normal = Quaternion.AngleAxis(angle, dirUp) * normal;
                                }

                                normals[vIndex] = normal;
                            }
                            if (calculateUVs)
                            {
                                uvs[vIndex] = new Vector2((-b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, fraction));
                            }

                            vIndex++;
                        }
                    }
                }
            }

            void DrawLine()
            {
                Vector3 point0 = points[index];
                Vector3 point1 = points[(index + 1).Loop(points.Count)];

                int vIndex = vOffset;

                Vector3 dir = (point1 - point0).normalized;
                Vector3 dirUp = Vector3.ProjectOnPlane(sideVectors[2 * index + 1], dir).normalized;
                Vector3 dirRight = Vector3.Cross(dir, dirUp).normalized;

                float uvEnd = 0;

                if (calculateUVs)
                {
                    float uvEndPref = uvYs[(index - 1).Loop(points.Count)].y;
                    float uvStart = index == 0 ? 0 : (uvEndPref + ((distances[index] - distances[(index - 1).Loop(points.Count)]) * length * scaleUV.y));
                    uvEnd = uvStart;
                    uvYs[index] = new Vector2(uvStart, uvEnd);
                }

                for (int b = 0, c = flatFaces ? ((circlePoints / 2) + 1) : circlePoints; b < c; b++)
                {
                    Vector3 vertex = Vector3.SlerpUnclamped(dirUp, dirRight, (4f * b) / (c - 1f)) * width;
                    vertex += point0;

                    if (flatFaces && b > 0 && b < c - 1)
                    {
                        vertices[vIndex] = vertex;
                        vertices[vIndex + 1] = vertex;

                        if (calculateNormals)
                        {
                            Vector3 normal = (vertex - point0).normalized;
                            float angle = 180f / (c - 1f);

                            normals[vIndex] = Quaternion.AngleAxis(-angle, dir) * normal;
                            normals[vIndex + 1] = Quaternion.AngleAxis(angle, dir) * normal;
                        }

                        if (calculateUVs)
                        {
                            Vector2 uv = new Vector2(-(b * scaleUV.x) / (c - 1f), uvEnd);
                            uvs[vIndex] = uv;
                            uvs[vIndex + 1] = uv;
                        }

                        vIndex += 2;
                    }
                    else
                    {
                        vertices[vIndex] = vertex;

                        if (calculateNormals)
                        {
                            Vector3 normal = (vertex - point0).normalized;

                            if (flatFaces)
                            {
                                float angle = 180f / (c - 1f);
                                angle *= b == 0 ? 1 : -1;
                                normal = Quaternion.AngleAxis(angle, dir) * normal;
                            }

                            normals[vIndex] = normal;
                        }
                        if (calculateUVs) uvs[vIndex] = new Vector2(-(b * scaleUV.x) / (c - 1f), uvEnd);

                        vIndex++;
                    }
                }
            }

            void DrawTurn()
            {
                int vIndex = vOffset;

                Vector3 point0 = points[(index - 1).Loop(points.Count)];
                Vector3 point1 = points[index];
                Vector3 point2 = points[(index + 1).Loop(points.Count)];
                Vector3 dir1 = (point0 - point1).normalized;
                Vector3 dir2 = (point2 - point1).normalized;
                Vector3 origin = point1;
                Vector3 side1 = sideVectors[2 * index];
                Vector3 side2 = sideVectors[(2 * index) + 1];
                Vector3 dirCross = Vector3.Cross(dir2, dir1).normalized;

                Vector3 side1Proj = Vector3.ProjectOnPlane(side1, dirCross).normalized;
                Vector3 side2Proj = Vector3.ProjectOnPlane(side2, dirCross).normalized;
                float angle = Vector3.Angle(side1Proj, side2Proj);

                Vector3 v1 = Quaternion.AngleAxis(-90, dirCross) * dir1;
                Vector3 v2 = Quaternion.AngleAxis(90, dirCross) * dir2;

                float uvStart = 0;
                float uvEnd = 0;

                if (calculateUVs)
                {
                    float angleTurn = Vector3.Angle(dir1, dir2);
                    float uvEndPref = uvYs[(index - 1).Loop(points.Count)].y;
                    uvStart = index == 0 ? 0 : (uvEndPref + ((distances[index] - distances[(index - 1).Loop(points.Count)]) * length * scaleUV.y));
                    uvEnd = uvStart + (Mathf.PI * ((180f - angleTurn) / 180f) * width * scaleUV.y);
                    uvYs[index] = new Vector2(uvStart, uvEnd);
                }

                for (int a = 0; a < turnSmoothPoints; a++)
                {
                    float fraction = turnSmoothPoints == 1 ? 0.5f : (a / (turnSmoothPoints - 1f));
                    float angleCross = fraction * angle;
                    Vector3 side = Quaternion.AngleAxis(angleCross, dirCross) * side1;

                    Vector3 dir = Vector3.SlerpUnclamped(v1, v2, fraction);
                    Vector3 normal = Vector3.Cross(dir, dirCross);

                    Vector3 projection = Vector3.ProjectOnPlane(side, normal).normalized;
                    Vector3 projection90 = (Quaternion.AngleAxis(90, normal) * projection).normalized;
                    Vector3 dirN = Vector3.Cross(projection, projection90).normalized;

                    for (int b = 0, c = flatFaces ? ((circlePoints / 2) + 1) : circlePoints; b < c; b++)
                    {
                        Vector3 d = Vector3.SlerpUnclamped(projection, projection90, (4f * b) / (c - 1f)) * width;
                        Vector3 vertex = origin + d;

                        if (flatFaces && b > 0 && b < c - 1)
                        {
                            vertices[vIndex] = vertex;
                            vertices[vIndex + 1] = vertex;

                            if (calculateNormals)
                            {
                                Vector3 normalN = (vertex - point1).normalized;
                                float angleN = 180f / (c - 1f);

                                normals[vIndex] = Quaternion.AngleAxis(-angleN, dirN) * normalN;
                                normals[vIndex + 1] = Quaternion.AngleAxis(angleN, dirN) * normalN;
                            }

                            if (calculateUVs)
                            {
                                Vector2 uv = new Vector2(-(b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, fraction));
                                uvs[vIndex] = uv;
                                uvs[vIndex + 1] = uv;
                            }

                            vIndex += 2;
                        }
                        else
                        {
                            vertices[vIndex] = vertex;

                            if (calculateNormals)
                            {
                                Vector3 normalN = (vertex - point1).normalized;

                                if (flatFaces)
                                {
                                    float angleN = 180f / (c - 1f);
                                    angleN *= b == 0 ? 1 : -1;
                                    normalN = Quaternion.AngleAxis(angleN, dirN) * normalN;
                                }

                                normals[vIndex] = normalN;
                            }
                            if (calculateUVs) uvs[vIndex] = new Vector2(-(b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, fraction));

                            vIndex++;
                        }
                    }
                }
            }

            void DrawEndingCap()
            {
                Vector3 point0 = points[index];
                Vector3 point1 = points[index - 1];

                Vector3 dirUp = (point0 - point1).normalized;
                Vector3 dirDown = (point1 - point0).normalized;
                Vector3 dirRight = Vector3.ProjectOnPlane(sideVectors[2 * index], dirDown).normalized;
                Vector3 dirForward = -Vector3.Cross(dirDown, dirRight).normalized;

                int vIndex = vOffset;

                float uvStart = 0;
                float uvEnd = 0;

                if (calculateUVs)
                {
                    float uvEndPref = uvYs[index - 1].y;
                    uvStart = uvEndPref + ((distances[index] - distances[index - 1]) * length * scaleUV.y);
                    uvEnd = uvStart + (Mathf.PI * .5f * width * scaleUV.y);
                    uvYs[index] = new Vector2(uvStart, uvEnd);
                }

                Vector3 vertex;

                for (int a = 0; a <= Mathf.Max(0, capSmoothPoints - 1); a++)
                {
                    float fraction = 1f - (a / Mathf.Max(capSmoothPoints, 1f));
                    float angleRad = (capSmoothPoints == 0 ? 90 : (90 * fraction)) * Mathf.Deg2Rad;
                    float height = this.width * Mathf.Cos(angleRad);
                    float width = this.width * Mathf.Sin(angleRad);
                    Vector3 v1 = dirRight * width;
                    Vector3 v2 = dirForward * width;
                    Vector3 v3 = dirUp * height;

                    for (int b = 0, c = flatFaces ? ((circlePoints / 2) + 1) : circlePoints; b < c; b++)
                    {
                        Vector3 v = Vector3.SlerpUnclamped(v1, v2, (4f * b) / (c - 1));
                        vertex = point0 + v + v3;

                        if (flatFaces && b > 0 && b < c - 1)
                        {
                            vertices[vIndex] = vertex;
                            vertices[vIndex + 1] = vertex;

                            if (calculateNormals)
                            {
                                Vector3 normal = (vertex - point0).normalized;
                                float angle = 180f / (c - 1f);

                                normals[vIndex] = Quaternion.AngleAxis(-angle, dirUp) * normal;
                                normals[vIndex + 1] = Quaternion.AngleAxis(angle, dirUp) * normal;
                            }

                            if (calculateUVs)
                            {
                                Vector2 uv = new Vector2(-(b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, 1f - fraction));
                                uvs[vIndex] = uv;
                                uvs[vIndex + 1] = uv;
                            }
                            vIndex += 2;
                        }
                        else
                        {
                            vertices[vIndex] = vertex;

                            if (calculateNormals)
                            {

                                Vector3 normal = (vertex - point0).normalized;

                                if (flatFaces)
                                {
                                    float angle = 180f / (c - 1f);
                                    angle *= b == 0 ? 1 : -1;
                                    normal = Quaternion.AngleAxis(angle, dirUp) * normal;
                                }

                                normals[vIndex] = normal;
                            }
                            if (calculateUVs) uvs[vIndex] = new Vector2(-(b * scaleUV.x) / (c - 1f), Mathf.Lerp(uvStart, uvEnd, 1f - fraction));

                            vIndex++;
                        }
                    }
                }

                //add cap vertex

                vertex = point0 + (capSmoothPoints == 0 ? Vector3.zero : (dirUp * width)); ;
                vertices[vIndex] = vertex;

                if (calculateNormals) normals[vIndex] = (vertex - point1).normalized;
                if (calculateUVs) uvs[vIndex] = new Vector2(.5f, uvEnd);
            }
        }

        public enum NodeType
        {
            Unused = 0,

            StartingCap,
            EndingCap,
            Line,
            Turn,
        }

        [Flags]
        public enum UpdateMode
        {
            None = 0,

            /// <summary>
            /// Number of points in line updated
            /// </summary>
            PointsCount = 1 << 0,
            /// <summary>
            /// Main Params like: circle points, cap smooth points, turn smooth points, Loop flag, etc modified.
            /// </summary>
            GlobalParams = 1 << 1,
            /// <summary>
            /// Single existing points position updated
            /// </summary>
            SinglePoints = 1 << 2,
            /// <summary>
            /// Changes which affect triangle indicies rebuild
            /// </summary>
            AffectedIndices = PointsCount | GlobalParams,
            /// <summary>
            /// New points added or existing point vector modified
            /// </summary>
            AffectedPoints = PointsCount | SinglePoints,

            All = ~0
        }

        public enum UpdateNormalsMode
        {
            None = 0,
            Internal = 1,
            InternalFlatFaces = 2,
            Unity = 3,
        }
    }
}