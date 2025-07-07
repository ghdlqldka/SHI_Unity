using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace mgear.tools
{
    public class _MeshPointsBoxSelect : MeshPointsBoxSelect
    {
        private static string LOG_FORMAT = "<color=#DAEA1E><b>[_MeshPointsBoxSelect]</b></color> {0}";

        protected override void Start()
        {
            // cam = Camera.main;
            cam = null; // Do not use!!!!!!
            UpdateMeshRenderers();
        }

        public override void Select(BoxCollider boxCollider)
        {
            Debug.LogFormat(LOG_FORMAT, "Select()");

            Bounds obbWorld = boxCollider.bounds;

            Transform boxColliderTransform = boxCollider.transform;

            Vector3 boxCenterLocal = boxCollider.center;
            Vector3 boxSizeLocal = boxCollider.size;
            Vector3 boxSizeHalfLocal = boxSizeLocal * 0.5f;

            selectedPoints.Clear();

            currentMeshIndex = -1;

            // check if ray intersects mesh bounds
            int len = pointMeshRenders.Length;
            for (int i = 0; i < len; i++)
            {
                Renderer meshRenderer = pointMeshRenders[i];

                // check if bounds intersects/contains obb bounds
                if (meshRenderer.bounds.Intersects(obbWorld) == true)
                {
                    Transform meshTransform = meshRenderer.transform;
                    //var inverseTransform = meshTransform.worldToLocalMatrix;
                    Mesh mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
                    mesh.GetVertices(vertsTemp);

                    // NOTE this generates gc, but is faster in general?
                    //vertsTempArray = vertsTemp.ToArray();
                    // 2020.1 and later can use https://docs.unity3d.com/ScriptReference/Mesh.AcquireReadOnlyMeshData.html ?

                    // loop vertices for this mesh
                    int len2 = vertsTemp.Count;
                    for (int j = 0; j < len2; j++)
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
    } // class

} // namespace
