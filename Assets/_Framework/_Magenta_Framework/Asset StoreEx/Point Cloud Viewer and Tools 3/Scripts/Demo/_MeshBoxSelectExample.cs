using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using mgear.tools;

namespace pointcloudviewer.examples
{
    public class _MeshBoxSelectExample : MeshBoxSelectExample
    {
        private static string LOG_FORMAT = "<color=#DAEA1E><b>[_MeshBoxSelectExample]</b></color> {0}";

        // public MeshPointsBoxSelect meshPointsBoxSelect;
        public _MeshPointsBoxSelect _meshPointsBoxSelect
        {
            get
            {
                return meshPointsBoxSelect as _MeshPointsBoxSelect;
            }
        }

        [SerializeField]
        protected Camera _camera;

        protected override void Start()
        {
            // cam = Camera.main;
            cam = _camera;
        }

        protected override void Update()
        {
            // if click mouse
            if (Input.GetMouseButtonDown(0) || isTesting)
            {
                // create ray at mouse pos, if you want to pick using VR hand or other devices, create ray from that position
                var ray = cam.ScreenPointToRay(Input.mousePosition);

                if (isTesting)
                {
                    ray = cam.ScreenPointToRay(new Vector3(522.9f, 260.7f, 0.0f));
                }

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // call point selector
                _meshPointsBoxSelect.Select(selectionBox.GetComponent<BoxCollider>());

                stopwatch.Stop();
                Debug.LogFormat("Timer: {0} ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
            } // mouse down
        } // Update()

        public override void OnPointsPicked(List<PointsData> selectedPoints)
        {
            Debug.LogFormat(LOG_FORMAT, "OnPointsPicked(), found: " + selectedPoints.Count + " points");

            if (measureInfoText != null)
                measureInfoText.text = $"Found {selectedPoints.Count} points";

            bool modified = false;

            // draw point positions
            for (int i = 0, len = selectedPoints.Count; i < len; i++)
            {
                var p = selectedPoints[i];

                // just as example, draw lines to show the points, NOTE this is slow for many points
                float lineSize = 0.015f;
                GLDebug.DrawLine(p.position + Vector3.left * lineSize, p.position + Vector3.right * lineSize, Color.red, 1, true);
                GLDebug.DrawLine(p.position + Vector3.up * lineSize, p.position + Vector3.down * lineSize, Color.green, 1, true);
                GLDebug.DrawLine(p.position + Vector3.forward * lineSize, p.position + Vector3.back * lineSize, Color.blue, 1, true);

                // TODO process all points per mesh at once, not one by one like here
                if (hideSelectedPoints == true)
                {
                    // get target mesh, if new go
                    if (cachedMeshGo != p.meshGameObject)
                    {
                        // get previous mesh ref
                        //previousCachedMeshGo = cachedMeshGo;

                        if (modified)
                        {
                            //cachedMesh.SetColors(colorsTemp);
                            cachedMesh.SetVertices(verticesTemp);
                            modified = false;
                        }

                        // get new mesh ref
                        cachedMeshGo = p.meshGameObject;
                        cachedMesh = p.meshGameObject.GetComponent<MeshFilter>().mesh;
                        //cachedMesh.GetColors(colorsTemp);
                        cachedMesh.GetVertices(verticesTemp);
                    }

                    // to hide point, set color to clear (NOTE: doesnt hide the point, just makes it transparent or black, would have to set vertex positions away, if want to "hide")
                    //colorsTemp[p.vertexIndex] = Color.clear;
                    // move vertices away, NOTE destructive..
                    verticesTemp[p.vertexIndex] = new Vector3(0, -99999, 0);
                    modified = true;

                    // set colors for last mesh
                    if (i == len - 1)
                    {
                        if (modified == true)
                        {
                            //cachedMesh.SetColors(colorsTemp);
                            cachedMesh.SetVertices(verticesTemp);
                            modified = false;
                        }
                    }
                } // if hideSelectedPoints


            } // for each point
        } // OnPointsPicked

        // link this into point picker OnPointNotFound event
        public override void OnPointsNotFound()
        {
            Debug.LogFormat(LOG_FORMAT, "No points found!");
        }
    } // class
} // namespace