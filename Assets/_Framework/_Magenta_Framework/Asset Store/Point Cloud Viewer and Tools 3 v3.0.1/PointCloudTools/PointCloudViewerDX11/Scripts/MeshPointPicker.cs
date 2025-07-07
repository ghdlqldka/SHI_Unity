using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace mgear.tools
{
    public class MeshPointPicker : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask pickLayerMask;
        public bool getPointColor = true;
        public float maxDistanceMeters = 512f;
        public Transform meshesRoot;

        [Header("Events")]
        public PointPickedEvent OnPointPicked;
        public UnityEvent OnPointNotFound;

        protected Renderer[] pointMeshRenders;

        protected List<Vector3> vertsTemp = new List<Vector3>();
        protected List<Color> colorsTemp = new List<Color>();
        //Vector3[] vertsTempArray;

        protected List<MeshPoint> meshPoints = new List<MeshPoint>();
        protected Color closestVertexColor;
        protected int closestVertexIndex;

        protected Plane nearClipPlane;
        protected Camera cam;
        protected Transform camTransform;

        protected virtual void Start()
        {
            cam = Camera.main;
            camTransform = cam.transform;

            UpdateMeshRenderers();
        }

        public void UpdateMeshRenderers()
        {
            pointMeshRenders = meshesRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
        }

        public virtual void Pick(Ray ray)
        {
            // view ray
            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 10);

            meshPoints.Clear();

            // check if ray intersects mesh bounds
            for (int i = 0, len = pointMeshRenders.Length; i < len; i++)
            {
                var meshRenderer = pointMeshRenders[i];
                if (meshRenderer.bounds.IntersectRay(ray))
                {
                    //PointCloudHelpers.PointCloudTools.DrawBounds(meshRenderer.bounds, 4);

                    var meshTransform = meshRenderer.transform;

                    var inverseTransform = meshTransform.worldToLocalMatrix;
                    var localRayOrigin = inverseTransform.MultiplyPoint(ray.origin);
                    var localRayDirection = inverseTransform.MultiplyVector(ray.direction);

                    float localRayDirectionX = localRayDirection.x;
                    float localRayDirectionY = localRayDirection.y;
                    float localRayDirectionZ = localRayDirection.z;
                    float localRayOriginX = localRayOrigin.x;
                    float localRayOriginY = localRayOrigin.y;
                    float localRayOriginZ = localRayOrigin.z;

                    var closestVertexPos = Vector3.zero;
                    var closestVertexDistance = float.MaxValue;
                    closestVertexColor = Color.black;
                    closestVertexIndex = -1;

                    var mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;

                    mesh.GetVertices(vertsTemp);
                    // NOTE this generates gc, but is faster in general?
                    //vertsTempArray = vertsTemp.ToArray();
                    // 2020.1 and later can use https://docs.unity3d.com/ScriptReference/Mesh.AcquireReadOnlyMeshData.html ?

                    // FIXME, doesnt work if mesh is scaled!

                    // loop vertices for this mesh
                    //for (int j = 0, len2 = vertsTempArray.Length; j < len2; j++)
                    for (int j = 0, len2 = vertsTemp.Count; j < len2; j++)
                    {
                        //var localVertex = vertsTempArray[j];
                        var localVertex = vertsTemp[j];
                        float localVertexX = localVertex.x;
                        float localVertexY = localVertex.y;
                        float localVertexZ = localVertex.z;

                        float distanceToRay =
                            (localRayDirectionY * (localVertexZ - localRayOriginZ) - localRayDirectionZ * (localVertexY - localRayOriginY)) *
                            (localRayDirectionY * (localVertexZ - localRayOriginZ) - localRayDirectionZ * (localVertexY - localRayOriginY)) +
                            (localRayDirectionZ * (localVertexX - localRayOriginX) - localRayDirectionX * (localVertexZ - localRayOriginZ)) *
                            (localRayDirectionZ * (localVertexX - localRayOriginX) - localRayDirectionX * (localVertexZ - localRayOriginZ)) +
                            (localRayDirectionX * (localVertexY - localRayOriginY) - localRayDirectionY * (localVertexX - localRayOriginX)) *
                            (localRayDirectionX * (localVertexY - localRayOriginY) - localRayDirectionY * (localVertexX - localRayOriginX));

                        if (distanceToRay < closestVertexDistance && distanceToRay < maxDistanceMeters)
                        {
                            // check if point is front of camera, using plane, TODO optimize
                            var worldVertex = meshTransform.TransformPoint(localVertex);
                            nearClipPlane = new Plane(-camTransform.forward, camTransform.position + camTransform.forward * cam.nearClipPlane);

                            if (nearClipPlane.GetSide(worldVertex))
                            {
                                continue;
                            }

                            closestVertexDistance = distanceToRay;
                            closestVertexIndex = j;
                        }

                    } // for each vertex in mesh

                    // if found closest vertex from this mesh, add it to list
                    if (closestVertexIndex > -1)
                    {
                        var worldVertex = meshTransform.TransformPoint(vertsTemp[closestVertexIndex]);
                        closestVertexPos = worldVertex;

                        if (getPointColor == true)
                        {
                            mesh.GetColors(colorsTemp);
                            closestVertexColor = colorsTemp[closestVertexIndex];
                        }

                        // add best vertex from this mesh to all good vertices list
                        meshPoints.Add(new MeshPoint { distance = closestVertexDistance, position = closestVertexPos, color = closestVertexColor, meshIndex = i });
                    }
                } // if intersect ray

            } // for each mesh renderer

            if (meshPoints.Count > 0)
            {
                // sort meshPoints by distance
                if (meshPoints.Count > 1) meshPoints.Sort((a, b) => a.distance.CompareTo(b.distance));

                // return closest point
                var p = meshPoints[0];
                OnPointPicked?.Invoke(new PointData { position = p.position, color = p.color, vertexIndex = closestVertexIndex, meshGameObject = pointMeshRenders[p.meshIndex].gameObject });
            }
            else
            {
                OnPointNotFound?.Invoke();
            }

        } // Pick()

    } // class

    public struct MeshPoint
    {
        public float distance;
        public Vector3 position;
        public Color color;
        public int meshIndex;
    }

    [System.Serializable]
    public class PointData
    {
        public Vector3 position;
        public Color color;
        public int vertexIndex;
        public GameObject meshGameObject;
    }

    [System.Serializable]
    public class PointPickedEvent : UnityEvent<PointData> { }

} // namespace
