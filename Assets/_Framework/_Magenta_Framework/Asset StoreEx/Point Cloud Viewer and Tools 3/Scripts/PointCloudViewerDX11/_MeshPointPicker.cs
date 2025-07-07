using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace mgear.tools
{
    public class _MeshPointPicker : MeshPointPicker
    {
        private static string LOG_FORMAT = "<color=#DAEA1E><b>[_MeshPointPicker]</b></color> {0}";

        [SerializeField]
        protected Camera _camera;

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // cam = Camera.main;
            cam = _camera;
            camTransform = cam.transform;

            UpdateMeshRenderers();
        }

        public override void Pick(Ray ray)
        {
            Debug.LogFormat(LOG_FORMAT, "Pick()");

            // view ray
            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 10);

            meshPoints.Clear();

            // check if ray intersects mesh bounds
            int len = pointMeshRenders.Length;
            for (int i = 0; i < len; i++)
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
                    int len2 = vertsTemp.Count;
                    for (int j = 0; j < len2; j++)
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
                        // meshPoints.Add(new MeshPoint { distance = closestVertexDistance, position = closestVertexPos, color = closestVertexColor, meshIndex = i });
                        MeshPoint _point = new MeshPoint();
                        _point.distance = closestVertexDistance;
                        _point.position = closestVertexPos;
                        _point.color = closestVertexColor;
                        _point.meshIndex = i;
                        meshPoints.Add(_point);
                    }
                } // if intersect ray

            } // for each mesh renderer

            if (meshPoints.Count > 0)
            {
                // sort meshPoints by distance
                if (meshPoints.Count > 1) 
                    meshPoints.Sort((a, b) => a.distance.CompareTo(b.distance));

                // return closest point
                MeshPoint p = meshPoints[0];
                // OnPointPicked?.Invoke(new PointData { position = p.position, color = p.color, vertexIndex = closestVertexIndex, meshGameObject = pointMeshRenders[p.meshIndex].gameObject });
                PointData data = new PointData();
                data.position = p.position;
                data.color = p.color;
                data.vertexIndex = closestVertexIndex;
                data.meshGameObject = pointMeshRenders[p.meshIndex].gameObject;
                OnPointPicked?.Invoke(data);
            }
            else
            {
                OnPointNotFound?.Invoke();
            }

        } // Pick()
    } // class


} // namespace
