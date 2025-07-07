using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace mgear.tools
{
    public class MeshPointsBoxSelect : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask pickLayerMask;
        public bool getPointColor = true;
        public Transform meshesRoot;

        [Header("Events")]
        public PointsPickedEvent OnPointsSelected;
        public UnityEvent OnPointsNotFound;

        protected Renderer[] pointMeshRenders;

        protected List<Vector3> vertsTemp = new List<Vector3>();
        protected List<Color> colorsTemp = new List<Color>();
        protected int currentMeshIndex = -1;

        protected List<PointsData> selectedPoints = new List<PointsData>();

        protected Camera cam;
        protected Color colorClear = Color.clear;


        protected virtual void Start()
        {
            cam = Camera.main;
            UpdateMeshRenderers();
        }

        public void UpdateMeshRenderers()
        {
            pointMeshRenders = meshesRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
        }

        public virtual void Select(BoxCollider boxCollider)
        {
            var obbWorld = boxCollider.bounds;

            var boxColliderTransform = boxCollider.transform;

            var boxCenterLocal = boxCollider.center;
            var boxSizeLocal = boxCollider.size;
            var boxSizeHalfLocal = boxSizeLocal * 0.5f;

            selectedPoints.Clear();

            currentMeshIndex = -1;

            // check if ray intersects mesh bounds
            for (int i = 0, len = pointMeshRenders.Length; i < len; i++)
            {
                var meshRenderer = pointMeshRenders[i];

                // check if bounds intersects/contains obb bounds
                if (meshRenderer.bounds.Intersects(obbWorld) == true)
                {
                    Transform meshTransform = meshRenderer.transform;
                    //var inverseTransform = meshTransform.worldToLocalMatrix;
                    var mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
                    mesh.GetVertices(vertsTemp);

                    // NOTE this generates gc, but is faster in general?
                    //vertsTempArray = vertsTemp.ToArray();
                    // 2020.1 and later can use https://docs.unity3d.com/ScriptReference/Mesh.AcquireReadOnlyMeshData.html ?

                    // loop vertices for this mesh
                    for (int j = 0, len2 = vertsTemp.Count; j < len2; j++)
                    {
                        var localVertex = vertsTemp[j];
                        var woldVertex = meshTransform.TransformPoint(localVertex);

                        // TODO could skip contains, if just check x,y,z within aabb bounds world manually
                        if (obbWorld.Contains(woldVertex) && PointInOABB(woldVertex, boxColliderTransform, boxCenterLocal, boxSizeHalfLocal))
                        {
                            Color vertColor = colorClear;
                            if (getPointColor == true)
                            {
                                // if new mesh, get colors
                                if (currentMeshIndex != i)
                                {
                                    currentMeshIndex = i;
                                    mesh.GetColors(colorsTemp);
                                }
                                vertColor = colorsTemp[j];
                            }

                            selectedPoints.Add(new PointsData { position = woldVertex, color = vertColor, meshGameObject = meshRenderer.gameObject, vertexIndex = j });
                        }
                    } // for each vertex in mesh
                } // if intersect ray
            } // for each mesh renderer

            if (selectedPoints.Count > 0)
            {
                OnPointsSelected?.Invoke(selectedPoints);
            }
            else
            {
                OnPointsNotFound?.Invoke();
            }

        } // Select() 

        // https://discussions.unity.com/t/test-to-see-if-a-vector3-point-is-within-a-boxcollider/17385/4
        protected bool PointInOABB(Vector3 worldPoint, Transform boxTransform, Vector3 boxCenterLocal, Vector3 boxSizeHalfLocal)
        {
            // note 2022 has batch, https://docs.unity3d.com/ScriptReference/Transform.InverseTransformPoints.html
            worldPoint = boxTransform.InverseTransformPoint(worldPoint);
            worldPoint.x -= boxCenterLocal.x;
            worldPoint.y -= boxCenterLocal.y;
            worldPoint.z -= boxCenterLocal.z;
            return !(worldPoint.x > boxSizeHalfLocal.x || worldPoint.x < -boxSizeHalfLocal.x || worldPoint.y > boxSizeHalfLocal.y || worldPoint.y < -boxSizeHalfLocal.y || worldPoint.z > boxSizeHalfLocal.z || worldPoint.z < -boxSizeHalfLocal.z);
        }
    } // class

    [System.Serializable]
    public class PointsData
    {
        public Vector3 position;
        public Color color;
        public GameObject meshGameObject;
        public int vertexIndex;
    }

    [System.Serializable]
    public class PointsPickedEvent : UnityEvent<List<PointsData>> { }

} // namespace
