using System;
using System.Collections.Generic;
using System.Linq;
using _Magenta_WebGL;
using EA;
using RTG;
using UnityEngine;

namespace _SHI_BA
{
    [ExecuteInEditMode]
    // [AddComponentMenu("Effects/Line Renderer 3D")]
    public class BA_LineRenderer3D_Gizmo : _Magenta_WebGL.MWGL_LineRenderer3D_Gizmo
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[BA_LineRenderer3D_Gizmo]</b></color> {0}";

        public new bool GizmoMode
        {
            get
            {
                return _gizmoMode;
            }
            set
            {
                _gizmoMode = value;
                Debug.LogFormat(LOG_FORMAT, "@@@@1");
                for (int i = 0; i < _GizmoList.Count; i++)
                {
                    _GizmoList[i].SetEnabled(value);
                }
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            updateNormals = UpdateNormalsMode.Internal; // forcelly set <= For Good quality
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
            // UpdateNormals = updateNormals;
            UpdateNormals = UpdateNormalsMode.Internal; // forcelly set <= For Good quality
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
                //if (!UnityEditor.Selection.objects.Contains(gameObject))
                //    return;

                // validate params

                CirclePoints = circlePoints;
                CapSmoothPoints = capSmoothPoints;
                TurnSmoothPoints = turnSmoothPoints;
                SideVectorRotation = sideVectorRotation;
                // Loop = loop;

                // UpdateNormals = updateNormals;
                UpdateNormals = UpdateNormalsMode.Internal; // forcelly set <= For Good quality
                ScaleUV = scaleUV;

                if (GizmoMode == true)
                {
                    Update_PointListByObjects();
                }

                SetPoints(new List<_LinePoint>(PointList));

                if (_width != _Width)
                {
                    _Width = _width;
                    width = new AnimationCurve(new Keyframe(0, _Width), new Keyframe(1, _Width));
                }

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

                if (_width != _Width)
                {
                    _Width = _width;
                    width = new AnimationCurve(new Keyframe(0, _Width), new Keyframe(1, _Width));
                }

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
                // UpdateNormals = updateNormals;
                UpdateNormals = UpdateNormalsMode.Internal; // forcelly set <= For Good quality
                ScaleUV = scaleUV;

                updateMode = UpdateMode.None;
                Calculate(UpdateMode.All);
            }
        }
#endif

        protected override void Calculate(UpdateMode mode)
        {
            // Debug.LogFormat(LOG_FORMAT, "Calculate(), updateMode : " + updateMode);

            if (ipoints.Count < 2)
            {
                _Mesh.Clear();
                return;
            }

            PrepareForLoop();
            CalculateUniqueNeighbouringPoints();
            CalculateSideVectors();
            CalculatePointsDistance();
            CalculateNodes();

            bool customNormals = UpdateNormals == UpdateNormalsMode.Internal || UpdateNormals == UpdateNormalsMode.InternalFlatFaces;

            //re-allocate arrays if necessary

            if (Application.isPlaying == false)
            {
                Array.Resize(ref vertices, vcount);
                Array.Resize(ref normals, vcount);
            }
            else
            {
                if (vertices == null || vertices.Length < vcount)
                    Array.Resize(ref vertices, vcount);
                if (customNormals && (normals == null || normals.Length < vcount))
                    Array.Resize(ref normals, vcount);
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
                        Array.Resize(ref uvs, vcount);
                    if (uvYs == null || uvYs.Length < ipoints.Count)
                        Array.Resize(ref uvYs, ipoints.Count);
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
                case UpdateNormalsMode.Internal:
                case UpdateNormalsMode.InternalFlatFaces:
                    _Mesh.SetNormals(normals, 0, vcount, flags);
                    break;
                case UpdateNormalsMode.Unity:
                    _Mesh.RecalculateNormals();
                    break;

                default:
                    // _Mesh.SetNormals(normals, 0, 0, flags);
                    Debug.Assert(false);
                    break;
            }

            _Mesh.Optimize();

            if (GizmoMode == false)
            {
                Update_PointObjects(mode);
            }
        }

        protected override void SetPointsInternal(IEnumerable<Vector3> epoints, int count)
        {
            var enumerator = epoints.GetEnumerator();
            int ecount = epoints.Count();

            if (ecount < count)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "{0}({1}, {2}) array has less elements than specified by count field.", nameof(SetPoints), ecount, count);
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

            while (this.PointList.Count < ecount)
            {
                _LinePoint point = new _LinePoint(Vector2.zero, Vector3.zero);
                this.PointList.Add(point);
            }

            if (this.PointList.Count > ecount)
            {
                this.PointList.RemoveRange(ecount, this.PointList.Count - ecount);
            }

            //set points values

            int eindex = 0;

            while (enumerator.MoveNext())
            {
                SetPoint(eindex++, enumerator.Current);

                if (eindex == ecount)
                {
                    break;
                }
            }
        }

        protected override void PrepareForLoop()
        {
            Debug.Assert(Loop == false);
            
            if (PointList.Count < ipoints.Count)
            {
                ipoints.RemoveAt(ipoints.Count - 1);
                dirtyPoints.RemoveAt(dirtyPoints.Count - 1);
            }
        }

        protected override void Update_PointObjects(UpdateMode mode)
        {
            // Debug.LogWarningFormat(LOG_FORMAT, "Update_PointObjects(), mode : <b><color=red>" + mode + "</color></b>");

            if ((updateMode & UpdateMode.PointsCount) != 0) // Number of points in line updated
            {
                if (PointObjList.Count != PointList.Count || PointObjList.Count != _GizmoList.Count)
                {
                    ResetAllPointObjects();
                }
            }

            if ((updateMode & UpdateMode.SinglePoints) != 0) // Single existing points position updated
            {
                Update_PointObjectsPosition();
            }
        }

        protected override void ResetAllPointObjects()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "ResetAllPointObjects()");

            // Clear All
            for (int i = _GizmoList.Count - 1; i >= 0; i--)
            {
                MWGL_RTGizmosEngine.Instance.RemoveGizmo(_GizmoList[i]);
            }
            _GizmoList.Clear();

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

                MWGL_ObjectTransformGizmo transformGizmo = MWGL_RTGizmosEngine.Instance.CreateObjectUniversalGizmo() as MWGL_ObjectTransformGizmo;
                transformGizmo.SetTargetObject(obj);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { obj });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);

                _GizmoList.Add(transformGizmo.Gizmo as MWGL_Gizmo);
                transformGizmo.Gizmo.SetEnabled(GizmoMode);
            }
        }
    }
}